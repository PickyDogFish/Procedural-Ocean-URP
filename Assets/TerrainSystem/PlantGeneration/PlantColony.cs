using System.Collections.Generic;
using UnityEngine;

namespace PlantGeneration {

    /// <summary>
    /// Represents a colony of one species of plant that can grow. Instantiates one plant on initialization.
    /// </summary>
    public class PlantColony {
        PlantSpecies species;
        List<GameObject> corals = new List<GameObject>();
        public GameObject gameObject;
        TerrainGenerator terrainGenerator;
        PlantSpawner coralSpawner;
        public int coralCount { get { return corals.Count; } }
        /* public event System.Action<CoralColony, bool> onVisibilityChanged; */
        public bool IsVisible() {
            return gameObject.activeSelf;
        }

        public void SetVisible(bool visible) {
            gameObject.SetActive(visible);
        }



        public PlantColony(PlantSpecies species, Vector3 startingLocation, TerrainGenerator terrainGenerator) {
            gameObject = new GameObject();
            gameObject.name = "CoralColony";
            gameObject.transform.position = startingLocation;
            SetVisible(false);
            this.species = species;
            this.terrainGenerator = terrainGenerator;
            this.coralSpawner = terrainGenerator.coralSpawner;
            GrowCoral(new Vector2(startingLocation.x, startingLocation.z));
            UpdateVisibility();
        }

        private void GrowCoral(Vector2 pos) {
            float height = terrainGenerator.GetHeightAt(pos);
            if (species.settings.minSpawnHeight < height && height < species.settings.maxSpawnHeight) {
                GameObject newCoral = species.GetRandomCoralInstance();
                newCoral.transform.parent = gameObject.transform;
                newCoral.transform.position = new Vector3(pos.x, height, pos.y);
                corals.Add(newCoral);
            }
        }

        public void Grow() {
            int numOfCorals = coralCount;
            for (int i = 0; i < numOfCorals; i++) {
                GameObject coralGO = corals[i];
                if (species.settings.growChance > Random.Range(0f, 1f)) {
                    GrowCoral(new Vector2(coralGO.transform.position.x, coralGO.transform.position.z) + RandomUtils.RandomVector2(coralSpawner.random) * species.settings.growSpread);
                }
            }
        }

        public void GrowNTimes(int n) {
            for (int i = 0; i < n; i++) {
                Grow();
            }
        }

        public void UpdateVisibility() {
            float viewerDist = Vector2.Distance(terrainGenerator.viewerPosition, new Vector2(gameObject.transform.position.x, gameObject.transform.position.z));
            bool wasVisible = IsVisible();
            bool visible = viewerDist <= coralSpawner.coralVisibilityRange;

            if (wasVisible != visible) {
                SetVisible(visible);
                /* if (onVisibilityChanged != null) {
                    onVisibilityChanged(this, visible);
                } */
            }
        }
    }
}
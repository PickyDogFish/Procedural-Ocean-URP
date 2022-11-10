using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoralSpawner : MonoBehaviour {
    public float coralVisibilityRange = 50;
    [SerializeField] private CoralSpeciesCollection coralSpeciesCollection;
    [SerializeField] private TerrainGenerator terrainGenerator;
    private Transform viewer;

    public int seed = 0;
    public System.Random random;
    private List<CoralColony> coralColonies = new List<CoralColony>();
    List<CoralColony> visibleCoralColonies = new List<CoralColony>();

    void Awake() {
        random = new System.Random(seed);
        viewer = terrainGenerator.viewer;
    }

    public void UpdateVisibleColonies() {
        foreach (CoralColony colony in coralColonies) {
            colony.UpdateVisibility();
        }
    }

    /*     void OnColonyVisibilityChanged(CoralColony colony, bool isVisible) {
            if (isVisible) {
                visibleCoralColonies.Add(colony);
            } else {
                visibleCoralColonies.Remove(colony);
            }
        }
     */

    public void SpawnColony(Vector2 offset, int growth) {
        Vector2 randomPos = offset + RandomUtils.RandomVector2(random) * terrainGenerator.meshSettings.meshWorldSize/2;
        float height = terrainGenerator.GetHeightAt(randomPos);
        if (height < 0) {
            CoralColony newColony = new CoralColony(coralSpeciesCollection.GetRandomCoralSpecies(), new Vector3(randomPos.x, height, randomPos.y), terrainGenerator);
            //newColony.onVisibilityChanged += OnColonyVisibilityChanged;
            newColony.gameObject.transform.parent = transform;
            newColony.GrowNTimes(growth);
            coralColonies.Add(newColony);
        }
    }
    public void SpawnColony(TerrainChunk chunk, int growth) {
        Vector2 offset = chunk.position;
        SpawnColony(offset, growth);
    }

    public void GrowAllColonies() {
        foreach (CoralColony colony in coralColonies) {
            colony.Grow();
        }
    }

    public void GrowAllColoniesNTimes(int n) {
        foreach (CoralColony colony in coralColonies) {
            colony.GrowNTimes(n);
        }
    }
}

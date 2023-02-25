using UnityEngine;
using PlantGeneration.SpaceColonisation;

namespace PlantGeneration {

    /// <summary>
    /// Holds coralCount amount of corals generated with the same settings but with random seeds.
    /// </summary>
    public class PlantSpecies{
        public PlantGenSettings settings;
        private GameObject[] corals;
        private int coralCount;

        public PlantSpecies(int coralCount, PlantGenSettings settings) {
            this.coralCount = coralCount;
            this.settings = settings;
            corals = new GameObject[coralCount];
            GenerateCorals();
        }

        private void GenerateCorals() {
            for (int i = 0; i < coralCount; i++) {
                corals[i] = CreateGameObject();
                corals[i].name = "Coral" + i.ToString();
            }
        }

        private GameObject CreateGameObject(){
            GameObject newGO = new GameObject();
            newGO.AddComponent<MeshFilter>();
            newGO.AddComponent<MeshRenderer>();
            settings.GetGenerator().Initialize(settings);
            newGO.GetComponent<MeshFilter>().mesh = settings.GetGenerator().Generate(settings, Random.Range(-10000, 10000));
            newGO.GetComponent<MeshRenderer>().material = settings.material;
            return newGO;
        }

        public GameObject GetCoralInstance(int index) {
            if (index > coralCount) {
                return null;
            } else {
                return GameObject.Instantiate(corals[index]);
            }
        }

        public GameObject GetRandomCoralInstance() {
            return GetCoralInstance(Random.Range(0, coralCount));
        }
    }
}
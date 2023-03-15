using UnityEngine;
using PlantGeneration.SpaceColonisation;

namespace PlantGeneration {

    /// <summary>
    /// Creates a CoralSpecies with x corals for every PlantGenSettings object
    /// </summary>
    public class PlantSpeciesCollection : MonoBehaviour {

        [HideInInspector] public PlantSpecies[] species;

        public PlantGenSettings[] speciesSettings;

        [SerializeField] private int coralsPerSpecies = 3;

        void Awake() {
            species = new PlantSpecies[speciesSettings.Length];
            for (int i = 0; i < speciesSettings.Length; i++) {
                GameObject speciesGO = new GameObject();
                speciesGO.transform.parent = transform;
                PlantSpecies newSpecies = speciesGO.AddComponent<PlantSpecies>();
                newSpecies.Initialize(coralsPerSpecies, speciesSettings[i], speciesGO.transform);
                species[i] = newSpecies;
            }
        }

        public PlantSpecies GetRandomCoralSpecies() {
            return species[Random.Range(0, species.Length)];
        }
    }
}
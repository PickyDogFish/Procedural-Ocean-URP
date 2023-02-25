using UnityEngine;
using PlantGeneration.SpaceColonisation;

namespace PlantGeneration {

    /// <summary>
    /// Creates a CoralSpecies with x corals for every CoralSCSettings object
    /// </summary>
    public class PlantSpeciesCollection : MonoBehaviour {

        [HideInInspector] public PlantSpecies[] species;

        public PlantGenSettings[] speciesSettings;

        [SerializeField] private int coralsPerSpecies = 3;

        void Awake() {
            species = new PlantSpecies[speciesSettings.Length];
            for (int i = 0; i < speciesSettings.Length; i++) {
                PlantSpecies newSpecies = new PlantSpecies(coralsPerSpecies, speciesSettings[i]);
                species[i] = newSpecies;
            }
        }

        public PlantSpecies GetRandomCoralSpecies() {
            return species[Random.Range(0, species.Length)];
        }
    }
}
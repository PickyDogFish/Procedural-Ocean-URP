using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates a CoralSpecies with x corals for every CoralSCSettings object
/// </summary>
public class CoralSpeciesCollection : MonoBehaviour {

    [HideInInspector] public CoralSpecies[] species;

    public CoralSCSettings[] speciesSettings;

    [SerializeField] private int coralsPerSpecies = 3;

    void Awake(){
        species = new CoralSpecies[speciesSettings.Length];
        for (int i = 0; i < speciesSettings.Length; i++)
        {
            CoralSCSettings settings = speciesSettings[i];
            CoralSpecies newSpecies = new CoralSpecies(coralsPerSpecies, settings);
            species[i] = newSpecies;
        }
    }

    public CoralSpecies GetRandomCoralSpecies(){
        return species[Random.Range(0, species.Length)];
    }
}

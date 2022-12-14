using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds coralCount amount of corals generated with the same settings but with random seeds.
/// </summary>
public class CoralSpecies{
    public CoralSCSettings settings;
    private GameObject[] corals;
    private int coralCount;
    private GameObject gameObject;

    public CoralSpecies(int coralCount, CoralSCSettings settings){
        gameObject = new GameObject();
        gameObject.name = "CoralSpecies";
        gameObject.SetActive(false);
        this.coralCount = coralCount;
        this.settings = settings;
        corals = new GameObject[coralCount];
        GenerateCorals();
    }

    private void GenerateCorals(){
        for (int i = 0; i < coralCount; i++)
        {
            corals[i] = SpaceColonization.GenerateCoral(settings, Random.Range(-10000,10000));
            corals[i].name = "Coral" + i.ToString();
            corals[i].transform.parent = gameObject.transform;
        }
    }

    public GameObject GetCoralInstance(int index){
        if (index > coralCount){
            return null;
        } else {
            return GameObject.Instantiate(corals[index]);
        }
    }

    public GameObject GetRandomCoralInstance(){
        return GetCoralInstance(Random.Range(0, coralCount));
    }
}

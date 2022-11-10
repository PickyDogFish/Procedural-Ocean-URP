using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarkupAttributes;

[CreateAssetMenu()]
public class CoralSCSettings : UpdatableData {
    [Box("Spawn settings")]
    [Range(0,1), Tooltip("Change to grow another coral when colony is growing")] public float growChance = 0.3f;
    public float maxSpawnHeight = 0;
    public float minSpawnHeight = float.MinValue;

    public float growSpread = 5;
    
    [Box("Coral settings")]
    public Material material;
    public int maxIterations = 500;
    public float radius = 1;

    [Box("Attractor settings")]
    public int attractorCount = 100;
    public float killRange = 0.1f;
    public float attractionRange = 0.5f;
    public Vector3 attractorFieldOffset = Vector3.up;
    //public bool useNoise = false;
    //public float noiseScale = 1;


    [Box("Branch settings")]
    public float branchLength = 0.2f;
    public int branchRadialSubdivisions = 4;
    public float maxAngleDegrees = 90;
    [Tooltip("diameter should be less than branchLength")] public float branchBaseDiameter = 0.1f;
    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
[CreateAssetMenu(fileName = "New Heightmap Settings", menuName = "Terrain/Heightmap Settings")]
public class HeightMapSettings : UpdatableData
{
    public NoiseSettings noiseSettings;

    public float heightMultiplier = 1;
    public AnimationCurve heightCurve;

    public float minHeight{
        get{
            return heightMultiplier * heightCurve.keys[0].value;
        }
    }

    public float maxHeight{
        get {
            return heightMultiplier * heightCurve.keys.Last().value;
        }
    }

    #if UNITY_EDITOR
    protected override void OnValidate() {
        noiseSettings.ValidateValues();
        base.OnValidate();
    }

    #endif
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator {

    public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCenter) {
        float[,] values = Noise.GenerateNoiseMap(width, height, settings.noiseSettings, sampleCenter);
        AnimationCurve heightCurveTSafe = new AnimationCurve(settings.heightCurve.keys);
        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                values[i,j] *= heightCurveTSafe.Evaluate(values[i,j]) * settings.heightMultiplier;

                if (values[i,j] > maxValue){
                    maxValue = values[i,j];
                }
                if (values[i,j] < minValue){
                    minValue = values[i,j];
                }
                
            }
        }
        return new HeightMap(values, minValue, maxValue);
    }

    public static float GetHeight(HeightMapSettings settings, Vector2 position){
        float val = Noise.GetNoiseValue(settings.noiseSettings, position);
        AnimationCurve heightCurveTSafe = new AnimationCurve(settings.heightCurve.keys);
        return val * heightCurveTSafe.Evaluate(val) * settings.heightMultiplier;
    }

}

public struct HeightMap {
    public readonly float[,] values;
    public readonly float minValue;
    public readonly float maxValue;


    public HeightMap(float[,] values, float minValue, float maxValue) {
        this.values = values;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }

}

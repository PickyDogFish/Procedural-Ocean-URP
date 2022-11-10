using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise {

    public enum NormalizeMode { Local, Global };
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, NoiseSettings noiseSettings, Vector2 sampleCenter) {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random random = new System.Random(noiseSettings.seed);
        Vector2[] octaveOffsets = GetOctaveOffsets(noiseSettings, sampleCenter);

        float maxPossibleHeight = GetMaxPossibleHeight(noiseSettings);

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;


        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < noiseSettings.octaves; i++) {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / noiseSettings.scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / noiseSettings.scale * frequency;
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;
                    amplitude *= noiseSettings.persistance;
                    frequency *= noiseSettings.lacunarity;
                }
                if (noiseHeight > maxLocalNoiseHeight) {
                    maxLocalNoiseHeight = noiseHeight;
                }
                if (noiseHeight < minLocalNoiseHeight) {
                    minLocalNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;

                if (noiseSettings.normalizeMode == NormalizeMode.Global) {
                    //the 0.8f factor is estimated
                    float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight + 0.8f);
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, 1);
                }
            }
        }

        //normalizing the map

        //Debug.Log((maxLocalNoiseHeight+1) / (maxPossibleHeight + 0.8f));
        //Debug.Log((minLocalNoiseHeight+1) / (maxPossibleHeight + 0.8f));
        if (noiseSettings.normalizeMode == NormalizeMode.Local) {
            for (int y = 0; y < mapHeight; y++) {
                for (int x = 0; x < mapWidth; x++) {
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                }
            }
        }

        return noiseMap;
    }

    //returns noise value at pos, normalized globally
    public static float GetNoiseValue(NoiseSettings noiseSettings, Vector2 pos){
        float[,] hmap = GenerateNoiseMap(1,1, noiseSettings, pos);
        return hmap[0,0];
    }

    private static float GetMaxPossibleHeight(NoiseSettings noiseSettings){
        float maxPossibleHeight = 0;
        float amplitude = 1;
        for (int i = 0; i < noiseSettings.octaves; i++) {
            maxPossibleHeight += amplitude;
            amplitude *= noiseSettings.persistance;
        }
        return maxPossibleHeight;
    }

    private static Vector2[] GetOctaveOffsets(NoiseSettings noiseSettings, Vector2 offset){
        System.Random random = new System.Random(noiseSettings.seed);
        Vector2[] octaveOffsets = new Vector2[noiseSettings.octaves];

        for (int i = 0; i < noiseSettings.octaves; i++) {
            float offsetX = random.Next(-10000, 10000) + noiseSettings.offset.x + offset.x;
            float offsetY = random.Next(-10000, 10000) - noiseSettings.offset.y - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }
        return octaveOffsets;
    }
}

[System.Serializable]
public class NoiseSettings {
    public float scale = 50f;
    public int octaves = 4;
    [Range(0, 1)]
    public float persistance = 0.5f;
    public float lacunarity = 2;
    public int seed = 0;
    public Vector2 offset = new Vector2(0, 0);
    public Noise.NormalizeMode normalizeMode;

    public void ValidateValues() {
        scale = Mathf.Max(scale, 0.001f);
        octaves = Mathf.Max(octaves, 1);
        lacunarity = Mathf.Max(lacunarity, 1);
        persistance = Mathf.Clamp01(persistance);
    }
}
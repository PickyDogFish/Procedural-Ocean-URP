using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarkupAttributes;

namespace PlantGeneration {

    public abstract class PlantGenSettings : UpdatableData {
        public abstract PlantGenerator GetGenerator();
        public Material material;
        public string speciesName = "GenericPlant";

        [Header("Spawn settings")]
        [Range(0, 1), Tooltip("Change to grow another coral when colony is growing")] public float growChance = 0.3f;
        public float maxSpawnHeight = 0;
        public float minSpawnHeight = float.MinValue;
        public float growSpread = 5;

        [Header("Variation")]
        public float maxSize = 1;
        public float minSize = 0.75f;
    }
}
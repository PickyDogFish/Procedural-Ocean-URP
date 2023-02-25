using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarkupAttributes;

namespace PlantGeneration.SpaceColonisation {

    [CreateAssetMenu(fileName = "New SC Settings", menuName = "Flora/SC Settings")]
    public class CoralSCSettings : PlantGenSettings {
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

        public override PlantGenerator GetGenerator()
        {
            return FindObjectOfType<SpaceColonization>();
        }

    }
}
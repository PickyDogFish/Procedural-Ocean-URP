using UnityEngine;

namespace PlantGeneration.SpaceColonisation {
    public class SCPreview : MonoBehaviour {
        public CoralSCSettings coralSettings;
        private GameObject coral = null;

        void OnValuesUpdated() {
            GenerateCoral();
        }
        void OnValidate() {

            if (coralSettings != null) {
                coralSettings.OnValuesUpdated -= OnValuesUpdated;
                coralSettings.OnValuesUpdated += OnValuesUpdated;
            }
        }

        public void GenerateCoral() {
            if (!Application.isPlaying) {
                if (coral != null) {
                    DestroyImmediate(coral);
                    coral = null;
                }
                coral = SpaceColonization.GenerateCoral(coralSettings, Random.Range(-10000, 10000));
                coral.transform.parent = transform;
            }
        }
    }
}
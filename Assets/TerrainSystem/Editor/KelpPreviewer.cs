using UnityEngine;
using UnityEditor;
using MarkupAttributes.Editor;

namespace PlantGeneration.Kelp {

    [CustomEditor(typeof(KelpPreview))]
    public class KelpPreviewer : MarkedUpEditor {
        public override void OnInspectorGUI() {
            KelpPreview kelpGen = (KelpPreview)target;
            DrawDefaultInspector();
            if (GUILayout.Button("Generate")) {
                kelpGen.Generate();
            }
        }
    }
}
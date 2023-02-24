using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MarkupAttributes.Editor;


namespace PlantGeneration {
    [CustomEditor(typeof(GeneratorPreview))]
    public class GeneratorPreviewer : MarkedUpEditor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            GeneratorPreview plantGenerator = (GeneratorPreview)target;
            if (GUILayout.Button("Generate")) {
                plantGenerator.Generate();
            }
        }
    }
}
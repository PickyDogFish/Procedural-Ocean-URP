using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MarkupAttributes.Editor;


namespace PlantGeneration.SpaceColonisation {
    [CustomEditor(typeof(SCPreview))]
    public class SCPreviewer : MarkedUpEditor {
        public override void OnInspectorGUI() {
            SCPreview coralGen = (SCPreview)target;
            if (GUILayout.Button("Generate")) {
                coralGen.GenerateCoral();
            }
        }
    }
}
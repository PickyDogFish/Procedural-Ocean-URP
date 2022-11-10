using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MarkupAttributes.Editor;

[CustomEditor(typeof(SCPreview))]
public class CoralEditor : MarkedUpEditor {
    public override void OnInspectorGUI() {
        SCPreview coralGen = (SCPreview) target;
        if (GUILayout.Button("Generate")){
            coralGen.GenerateCoral();
        }
    }
}

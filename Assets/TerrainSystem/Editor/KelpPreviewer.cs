using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MarkupAttributes.Editor;

[CustomEditor(typeof(KelpPreview))]
public class KelpPreviewer : MarkedUpEditor {
    public override void OnInspectorGUI() {
        KelpPreview kelpGen = (KelpPreview) target;
        DrawDefaultInspector();
        if (GUILayout.Button("Generate")){
            kelpGen.Generate();
        }
    }
}

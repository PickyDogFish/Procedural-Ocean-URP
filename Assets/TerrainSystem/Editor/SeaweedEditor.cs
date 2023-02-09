using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MarkupAttributes.Editor;

[CustomEditor(typeof(GiantKelpGen))]
public class SeaweedEditor : MarkedUpEditor {
    public override void OnInspectorGUI() {
        GiantKelpGen seaweedGen = (GiantKelpGen) target;
        DrawDefaultInspector();
        if (GUILayout.Button("Generate")){
            seaweedGen.GenerateSeaweed();
        }
    }
}

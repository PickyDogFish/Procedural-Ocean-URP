using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MarkupAttributes.Editor;

[CustomEditor(typeof(TallSeaweedGen))]
public class SeaweedEditor : MarkedUpEditor {
    public override void OnInspectorGUI() {
        TallSeaweedGen seaweedGen = (TallSeaweedGen) target;
        DrawDefaultInspector();
        if (GUILayout.Button("Generate")){
            seaweedGen.GenerateSeaweed();
        }
    }
}

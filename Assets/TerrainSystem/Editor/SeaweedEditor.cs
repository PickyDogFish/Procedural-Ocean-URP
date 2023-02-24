using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MarkupAttributes.Editor;

[CustomEditor(typeof(GiantKelpGen))]
public class SeaweedEditor : MarkedUpEditor {
    public override void OnInspectorGUI() {
        GiantKelpGen kelpGen = (GiantKelpGen) target;
        DrawDefaultInspector();
        if (GUILayout.Button("Generate")){
            MeshFilter meshFilter = kelpGen.GetComponentInChildren<MeshFilter>();
            if (meshFilter == null){
                Debug.Log("Please attach a child with Mesh Filter component");
                return;
            }
            meshFilter.mesh = kelpGen.Generate();
        }
    }
}

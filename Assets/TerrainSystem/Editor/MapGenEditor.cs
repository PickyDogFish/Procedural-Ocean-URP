using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MarkupAttributes.Editor;

[CustomEditor (typeof(TerrainPreview))]
public class MapGenEditor : MarkedUpEditor
{
    public override void OnInspectorGUI() {
        TerrainPreview mapGen = (TerrainPreview) target;
        if (DrawDefaultInspector()){
            if (mapGen.autoUpdate){
                mapGen.DrawMapInEditor();
            }
        }

        if (GUILayout.Button("Generate")){
            mapGen.DrawMapInEditor();
        }
    }
}

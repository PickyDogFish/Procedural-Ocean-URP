using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MarkupAttributes.Editor;
namespace ReactionDiffusion
{

    [CustomEditor(typeof(RDLayered))]
    public class RDLEditor : MarkedUpEditor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            RDLayered coralGen = (RDLayered)target;
            if (GUILayout.Button("Generate"))
            {
                coralGen.gameObject.GetComponent<MeshFilter>().mesh = coralGen.GenerateCoral();
                coralGen.CleanUp();
            }
        }
    }

}
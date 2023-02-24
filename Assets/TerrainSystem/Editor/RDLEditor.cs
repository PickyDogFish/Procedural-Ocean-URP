using UnityEngine;
using UnityEditor;
using MarkupAttributes.Editor;

namespace PlantGeneration.ReactionDiffusion
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
                coralGen.gameObject.GetComponentInChildren<MeshFilter>().mesh = coralGen.GenerateCoral();
                coralGen.CleanUp();
            }
        }
    }

}
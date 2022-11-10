using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MarkupAttributes.Editor;

[CustomEditor(typeof(UpdatableData), true)]
public class UpdatableDataEditor : MarkedUpEditor
{
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        UpdatableData data = (UpdatableData) target;
        if (GUILayout.Button("Update")){
            data.NotifyOfUpdatedValues();
            EditorUtility.SetDirty(target);
        }
    }
}

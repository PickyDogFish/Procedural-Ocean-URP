using UnityEngine;

public class CausticsMaterialUpdater : MonoBehaviour
{
    [SerializeField] private Material causticsMaterial;

    void Update(){
        SetCausticsParameters();
    }

    void SetCausticsParameters(){
        Matrix4x4 mainLightMatrix = RenderSettings.sun.transform.localToWorldMatrix;
        causticsMaterial.SetMatrix("_MainLightDirection", mainLightMatrix);
    }


}
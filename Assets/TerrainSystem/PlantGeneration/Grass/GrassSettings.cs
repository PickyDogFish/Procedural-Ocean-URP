using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "New Procedural Grass Settings", menuName = "Flora/Procedural Grass Settings")]
public class GrassSettings : ScriptableObject
{
    [Header("Rendering Properties")]

    [Tooltip("Compute shader for generating transformation matrices.")]
    public ComputeShader computeShader;

    private Mesh terrainMesh;
    [Tooltip("Mesh for individual grass blades.")]
    public Mesh grassMesh;
    [Tooltip("Material for rendering each grass blade.")]
    public Material material;

    [Space(10)]

    [Header("Lighting and Shadows")]

    [Tooltip("Should the procedural grass cast shadows?")]
    public ShadowCastingMode castShadows = ShadowCastingMode.On;
    [Tooltip("Should the procedural grass receive shadows from other objects?")]
    public bool receiveShadows = true;

    [Space(10)]

    [Header("Grass Blade Properties")]

    [Range(0.0f, 5.0f)]
    [Tooltip("Base size of grass blades in all three axes.")]
    public float scale = 0.1f;
    [Range(0.0f, 5.0f)]
    [Tooltip("Minimum y scale multiplier.")]
    public float minBladeHeight = 0.5f;
    [Range(0.0f, 5.0f)]
    [Tooltip("Maximum y scale multiplier.")]
    public float maxBladeHeight = 1.5f;

    [Range(-1.0f, 1.0f)]
    [Tooltip("Minimum random offset in the x- and z-directions.")]
    public float minOffset = -0.1f;
    [Range(-1.0f, 1.0f)]
    [Tooltip("Maximum random offset in the x- and z-directions.")]
    public float maxOffset = 0.1f;
}

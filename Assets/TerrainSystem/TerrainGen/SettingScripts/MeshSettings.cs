using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "New Mesh Settings", menuName = "Terrain/Mesh Settings")]
public class MeshSettings : UpdatableData {
    public float meshScale = 0.1f;
    public const int numOfSupportedLOD = 5;
    public const int numSupportedChunkSizes = 9;
    public static readonly int[] supportedChunkSizes = {48, 72, 96, 120, 144, 168, 192, 216, 240};
    [Range(0, numSupportedChunkSizes-1)] public int chunkSizeIndex;

    // number of verts per line at LOD = 0. Includes 2 extra vertices for normal calculation which are excluded from final mesh
    public int numVertsPerLine{
        get {
            return supportedChunkSizes[chunkSizeIndex] + 5;
        }
    }

    public float meshWorldSize{
        get {
            return (numVertsPerLine - 3) * meshScale;
        }
    }

}

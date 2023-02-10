using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {
    const float viewerMoveChunkUpdateThreshold = 25f;
    const float sqrViewerMoveChunkUpdateThreshold = viewerMoveChunkUpdateThreshold * viewerMoveChunkUpdateThreshold;

    public float verticalOffset = 0;

    public int colliderLODIndex;
    public LODInfo[] detailLevels;

    public HeightMapSettings heightMapSettings;
    public MeshSettings meshSettings;
    public TextureData textureSettings;

    public Material terrainMaterial;

    public CoralSpawner coralSpawner;

    public Transform viewer;
    public Vector2 viewerPosition;
    Vector2 viewerPositionOld;

    float meshWorldSize;
    int chunksVisibleInViewDist;

    Dictionary<Vector2, TerrainChunk> terrainChunkDict = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    void Start() {
        textureSettings.ApplyToMaterial(terrainMaterial);
        textureSettings.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        float maxViewDist = detailLevels[detailLevels.Length - 1].visibleDistThreshold;
        meshWorldSize = meshSettings.meshWorldSize;
        chunksVisibleInViewDist = Mathf.RoundToInt(maxViewDist / meshWorldSize);
        UpdateVisibleChunks();
        coralSpawner.UpdateVisibleColonies();
    }

    void Update() {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if (viewerPosition != viewerPositionOld) {
            foreach (TerrainChunk chunk in visibleTerrainChunks) {
                chunk.UpdateCollisionMesh();
            }
        }
        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveChunkUpdateThreshold) {
            UpdateVisibleChunks();
            coralSpawner.UpdateVisibleColonies();
            viewerPositionOld = viewerPosition;
        }
    }

    void UpdateVisibleChunks() {
        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();
        for (int i = visibleTerrainChunks.Count - 1; i >= 0; i--) {
            alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
            visibleTerrainChunks[i].UpdateTerrainChunk();
        }

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

        for (int yOffset = -chunksVisibleInViewDist; yOffset < chunksVisibleInViewDist; yOffset++) {
            for (int xOffset = -chunksVisibleInViewDist; xOffset < chunksVisibleInViewDist; xOffset++) {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord)) {
                    if (terrainChunkDict.ContainsKey(viewedChunkCoord)) {
                        terrainChunkDict[viewedChunkCoord].UpdateTerrainChunk();
                    } else {
                        TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, verticalOffset, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, transform, viewer, terrainMaterial);
                        terrainChunkDict.Add(viewedChunkCoord, newChunk);
                        newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
                        coralSpawner.SpawnColony(newChunk, 5);
                        newChunk.Load();
                    }
                }
            }

        }
    }

    void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible) {
        if (isVisible) {
            visibleTerrainChunks.Add(chunk);
        } else {
            visibleTerrainChunks.Remove(chunk);
        }
    }

    ///<summary>
    /// Returns the theoretical height of the terrain at the given global position. Because the terrain's resolution is limited there can be differences between theoretical and actual height at the given point.
    ///</summary>
    public float GetHeightAt(Vector2 pos) {
        float height = HeightMapGenerator.GetHeight(heightMapSettings, pos / meshSettings.meshScale) + verticalOffset * meshSettings.meshWorldSize;
        return height;
    }
}

[System.Serializable]
public struct LODInfo {
    [Range(0, MeshSettings.numOfSupportedLOD - 1)] public int lod;
    public float visibleDistThreshold;

    public float sqrVisibleDstThreshold {
        get {
            return visibleDistThreshold * visibleDistThreshold;
        }
    }
}

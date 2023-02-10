using System.Collections;
using UnityEngine;

public class GiantKelpGen : MonoBehaviour
{
    [ExecuteInEditMode]
    public float maxHeight = 3f;
    public float segmentLength = 0.1f;
    private int stemSegmentCount = 0;
    public float branchAngle = 0.3f;
    public float leafScale = 1;
    public int radialSubdivs = 3;
    public float diameter = 0.05f;
    [SerializeField] private Mesh leaf;
    [SerializeField] private bool showGizmos;
    public float minAngle,maxAngle = 135;

    private struct Segment
    {
        public Vector3 from;
        public Vector3 to;
    }

    private ArrayList segments;

    void Start()
    {

    }

    public void GenerateSeaweed()
    {
        segments = new ArrayList();
        GenerateStem();
        GenerateBranches();

        GetComponentInChildren<MeshFilter>().mesh = GenerateMesh();
    }

    private void GenerateStem()
    {
        stemSegmentCount = (int)(maxHeight / segmentLength);
        for (float i = 0; i < maxHeight; i += segmentLength)
        {
            Segment newSegment = new Segment();
            newSegment.from = new Vector3(0, i, 0);
            newSegment.to = new Vector3(0, i + segmentLength, 0);
            segments.Add(newSegment);
        }
    }

    private void GenerateBranches()
    {
        for (int i = 0; i < stemSegmentCount; i++)
        {
            Segment newSegment = new Segment();
            newSegment.from = ((Segment)segments[i]).to;
            Vector3 to = Quaternion.Euler(0, Random.Range(0.0f, 360f), Random.Range(0.0f, 75f)) * new Vector3(0, 0.1f, 0);
            newSegment.to = newSegment.from + to;
            segments.Add(newSegment);
        }
    }

    private Mesh GenerateMesh()
    {
        Mesh mesh = new Mesh();
        int leafCount = segments.Count - stemSegmentCount;
        int vertexCount = leafCount * leaf.vertexCount + stemSegmentCount * radialSubdivs;
        int triIndexCount = leafCount * leaf.triangles.Length + stemSegmentCount * radialSubdivs * 6;
        MeshData meshData = new MeshData(vertexCount, triIndexCount);
        for (int i = stemSegmentCount + 1; i < segments.Count; i++)
        {
            GenerateLeafMesh((Segment)segments[i], ref meshData);
        }
        GenerateStemMesh(ref meshData);
        mesh.vertices = meshData.vertices;
        mesh.triangles = meshData.triangles;
        mesh.normals = meshData.normals;
        mesh.uv = meshData.uvs;
        mesh.RecalculateBounds();
        return mesh;
    }

    private void GenerateStemMesh(ref MeshData meshData)
    {
        AddVertCircle((Segment)segments[0], radialSubdivs, ref meshData);
        for (int i = 1; i < stemSegmentCount; i++)
        {
            int firstCircleIndex = meshData.vertexIndex - radialSubdivs;
            AddVertCircle((Segment)segments[i], radialSubdivs, ref meshData);
            int secondCircleIndex = meshData.vertexIndex - radialSubdivs;
            for (int quad = 0; quad < radialSubdivs-1; quad++)
            {
                meshData.triangles[meshData.triangleIndex++] = firstCircleIndex + quad;
                meshData.triangles[meshData.triangleIndex++] = firstCircleIndex + quad + 1;
                meshData.triangles[meshData.triangleIndex++] = secondCircleIndex + quad;
                
                meshData.triangles[meshData.triangleIndex++] = firstCircleIndex + quad + 1;
                meshData.triangles[meshData.triangleIndex++] = secondCircleIndex + quad + 1;
                meshData.triangles[meshData.triangleIndex++] = secondCircleIndex + quad;
            }
            meshData.triangles[meshData.triangleIndex++] = firstCircleIndex + radialSubdivs -1 ;
            meshData.triangles[meshData.triangleIndex++] = firstCircleIndex;
            meshData.triangles[meshData.triangleIndex++] = secondCircleIndex + radialSubdivs - 1;
            
            meshData.triangles[meshData.triangleIndex++] = firstCircleIndex;
            meshData.triangles[meshData.triangleIndex++] = secondCircleIndex;
            meshData.triangles[meshData.triangleIndex++] = secondCircleIndex + radialSubdivs - 1;
        }
    }

    //creates a circle of vertices at the end of segment and inserts them into the vertices array
    private void AddVertCircle(Segment seg, int radialSubdivisions, ref MeshData meshData)
    {
        for (int circularIndex = 0; circularIndex < radialSubdivisions; circularIndex++)
        {
            float alpha = ((float)circularIndex / radialSubdivisions) * Mathf.PI * 2f;

            Vector3 pos = new Vector3(Mathf.Cos(alpha) * diameter, 0, Mathf.Sin(alpha) * diameter);

            pos += seg.to;

            meshData.vertices[meshData.vertexIndex] = pos;// - transform.position; // from tree object coordinates to [0; 0; 0]
            meshData.uvs[meshData.vertexIndex] = new Vector2(0.5f,0.5f);
            meshData.vertexIndex++;
        }
    }

    private void GenerateLeafMesh(Segment seg, ref MeshData meshData)
    {
        for (int i = 0; i < leaf.triangles.Length; i++)
        {
            meshData.triangles[meshData.triangleIndex + i] = leaf.triangles[i] + meshData.vertexIndex;
        }
        meshData.triangleIndex += leaf.triangles.Length;
        Quaternion randomAngle = Quaternion.Euler(Random.Range(minAngle, maxAngle), Random.Range(0f, 360f), Random.Range(-5f, 5f));
        for (int i = 0; i < leaf.vertices.Length; i++)
        {
            meshData.vertices[meshData.vertexIndex] = randomAngle * (leaf.vertices[i] * leafScale) + seg.from;
            meshData.normals[meshData.vertexIndex] = randomAngle * -leaf.normals[i];
            meshData.uvs[meshData.vertexIndex] = leaf.uv[i];
            meshData.vertexIndex++;
        }


    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        if (showGizmos && segments != null)
        {

            foreach (Segment seg in segments)
            {
                Gizmos.DrawLine(seg.from, seg.to);
            }
        }
    }

    void PrintSegments()
    {
        string arrString = "";
        for (int i = 0; i < segments.Count; i++)
        {
            Segment seg = (Segment)segments[i];
            arrString += "seg " + i + " " + seg.from + " " + seg.to + "\n";
        }
        Debug.Log(arrString);
    }

    class MeshData{
        public Vector3[] vertices;
        public Vector3[] normals;
        public Vector2[] uvs;
        public int[] triangles;
        public int vertexIndex = 0;
        public int triangleIndex = 0;
        public MeshData(int vertexCount, int triangleIndexCount){
            vertices = new Vector3[vertexCount];
            normals = new Vector3[vertexCount];
            uvs = new Vector2[vertexCount];
            triangles = new int[triangleIndexCount];
        }
    }
}


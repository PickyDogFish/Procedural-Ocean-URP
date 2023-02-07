using System.Collections;
using UnityEngine;

public class TallSeaweedGen : MonoBehaviour
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
        Vector3[] verts = new Vector3[leafCount * leaf.vertexCount + stemSegmentCount * radialSubdivs];
        Vector3[] normals = new Vector3[leafCount * leaf.vertexCount + stemSegmentCount * radialSubdivs];
        int[] tris = new int[leafCount * leaf.triangles.Length + stemSegmentCount * radialSubdivs * 6];
        int vertIndex = 0;
        int triIndex = 0;
        for (int i = stemSegmentCount + 1; i < segments.Count; i++)
        {
            GenerateLeafMesh((Segment)segments[i], ref verts, ref normals, ref tris, ref vertIndex, ref triIndex);
        }
        GenerateStemMesh(ref verts, ref normals, ref tris, ref vertIndex, ref triIndex);
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.normals = normals;
        return mesh;
    }

    private void GenerateStemMesh(ref Vector3[] verts, ref Vector3[] normals, ref int[] tris, ref int vertIndex, ref int triIndex)
    {
        AddVertCircle((Segment)segments[0], radialSubdivs, ref verts, ref vertIndex);
        for (int i = 1; i < stemSegmentCount; i++)
        {
            int firstCircleIndex = vertIndex - radialSubdivs;
            AddVertCircle((Segment)segments[i], radialSubdivs, ref verts, ref vertIndex);
            int secondCircleIndex = vertIndex - radialSubdivs;
            for (int quad = 0; quad < radialSubdivs-1; quad++)
            {
                tris[triIndex++] = firstCircleIndex + quad;
                tris[triIndex++] = firstCircleIndex + quad + 1;
                tris[triIndex++] = secondCircleIndex + quad;
                
                tris[triIndex++] = firstCircleIndex + quad + 1;
                tris[triIndex++] = secondCircleIndex + quad + 1;
                tris[triIndex++] = secondCircleIndex + quad;
            }

            tris[triIndex++] = firstCircleIndex + radialSubdivs -1 ;
            tris[triIndex++] = firstCircleIndex;
            tris[triIndex++] = secondCircleIndex + radialSubdivs - 1;
            
            tris[triIndex++] = firstCircleIndex;
            tris[triIndex++] = secondCircleIndex;
            tris[triIndex++] = secondCircleIndex + radialSubdivs - 1;
        }
    }

    //creates a circle of vertices at the end of segment and inserts them into the vertices array
    private void AddVertCircle(Segment seg, int radialSubdivisions, ref Vector3[] vertices, ref int vertIndex)
    {
        for (int circularIndex = 0; circularIndex < radialSubdivisions; circularIndex++)
        {
            float alpha = ((float)circularIndex / radialSubdivisions) * Mathf.PI * 2f;

            Vector3 pos = new Vector3(Mathf.Cos(alpha) * diameter, 0, Mathf.Sin(alpha) * diameter);

            pos += seg.to;

            vertices[vertIndex] = pos;// - transform.position; // from tree object coordinates to [0; 0; 0]
            vertIndex++;
        }
    }

    private void GenerateLeafMesh(Segment seg, ref Vector3[] verts, ref Vector3[] normals, ref int[] tris, ref int vertIndex, ref int triIndex)
    {
        for (int i = 0; i < leaf.triangles.Length; i++)
        {
            tris[triIndex + i] = leaf.triangles[i] + vertIndex;
        }
        triIndex += leaf.triangles.Length;
        Quaternion randomAngle = Quaternion.Euler(135, Random.Range(0f, 360f), 0);
        for (int i = 0; i < leaf.vertices.Length; i++)
        {
            verts[vertIndex] = randomAngle * (leaf.vertices[i] * leafScale) + seg.from;
            normals[vertIndex] = randomAngle * -leaf.normals[i];
            vertIndex++;
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
}

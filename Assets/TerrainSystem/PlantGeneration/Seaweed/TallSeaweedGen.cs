using System.Collections;
using UnityEngine;

public class TallSeaweedGen : MonoBehaviour
{
    [ExecuteInEditMode]
    public float maxHeight = 3f;
    public float segmentLength = 0.1f;
    private int stemSegmentCount = 0;
    public float branchAngle = 0.3f;
    [SerializeField] private Mesh leaf;

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
        PrintSegments();

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

    private void GenerateBranches(){
        for (int i = 0; i < stemSegmentCount; i++)
        {
            Segment newSegment = new Segment();
            newSegment.from = ((Segment)segments[i]).to;
            Vector3 to = Quaternion.Euler(0, Random.Range(0.0f,360f), Random.Range(0.0f,75f)) * new Vector3(0,0.1f,0);
            newSegment.to = newSegment.from + to;
            segments.Add(newSegment);
        }
    }

    private Mesh GenerateMesh(){
        Mesh mesh = new Mesh();
        int leafCount = segments.Count - stemSegmentCount;
        Vector3[] verts = new Vector3[3 * leafCount];
        int[] tris = new int[3 * leafCount];
        int index = 0;
        for (int i = stemSegmentCount+1; i < segments.Count; i++)
        {
            GenerateLeaf((Segment)segments[i], ref verts, ref tris, ref index);
        }
        mesh.vertices = verts;
        mesh.triangles = tris;
        return mesh;
    }

    private void GenerateLeaf(Segment seg, ref Vector3[] verts, ref int[] tris, ref int startIndex){
        
        verts[startIndex] = seg.from;
        tris[startIndex] = startIndex;
        startIndex++;
        verts[startIndex] = seg.to;
        tris[startIndex] = startIndex;
        startIndex++;
        verts[startIndex] = seg.from + Quaternion.Euler(0, 30f, 0) * (seg.to - seg.from);
        tris[startIndex] = startIndex;
        startIndex++;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        if (segments != null)
        {
            
            foreach (Segment seg in segments)
            {
                Gizmos.DrawLine(seg.from, seg.to);
            }
        }
    }

    void PrintSegments(){
        string arrString = "";
        for (int i = 0; i < segments.Count; i++)
        {
            Segment seg = (Segment)segments[i];
            arrString += "seg " + i + " " + seg.from + " " + seg.to + "\n";
        }
        Debug.Log(arrString);
    }
}

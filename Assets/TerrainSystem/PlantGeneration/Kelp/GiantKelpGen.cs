using System.Collections;
using UnityEngine;

namespace PlantGeneration.Kelp {

    public class GiantKelpGen : PlantGenerator {
        [ExecuteInEditMode]
        private int stemSegmentCount = 0;

        private struct Segment {
            public Vector3 from;
            public Vector3 to;
        }

        private ArrayList segments;

        public override void Initialize(PlantGenSettings settings){
            segments = new ArrayList();
        }

        private void GenerateStemSkeleton(KelpSettings settings) {
            stemSegmentCount = (int)(settings.maxHeight / settings.segmentLength);
            for (float i = 0; i < settings.maxHeight; i += settings.segmentLength) {
                Segment newSegment = new Segment();
                newSegment.from = new Vector3(0, i, 0);
                newSegment.to = new Vector3(0, i + settings.segmentLength, 0);
                segments.Add(newSegment);
            }
        }

        private void GenerateBranchSkeleton() {
            for (int i = 0; i < stemSegmentCount; i++) {
                Segment newSegment = new Segment();
                newSegment.from = ((Segment)segments[i]).to;
                Vector3 to = Quaternion.Euler(0, Random.Range(0.0f, 360f), Random.Range(0.0f, 75f)) * new Vector3(0, 0.1f, 0);
                newSegment.to = newSegment.from + to;
                segments.Add(newSegment);
            }
        }

        public override Mesh Generate(PlantGenSettings plantSettings, int seed) {
            KelpSettings settings = (KelpSettings)plantSettings;
            GenerateStemSkeleton(settings);
            GenerateBranchSkeleton();
            
            Mesh mesh = new Mesh();
            int leafCount = segments.Count - stemSegmentCount;
            int vertexCount = leafCount * settings.leaf.vertexCount + stemSegmentCount * settings.radialSubdivs;
            int triIndexCount = leafCount * settings.leaf.triangles.Length + stemSegmentCount * settings.radialSubdivs * 6;
            MeshData meshData = new MeshData(vertexCount, triIndexCount);
            for (int i = stemSegmentCount + 1; i < segments.Count; i++) {
                GenerateLeafMesh(settings, (Segment)segments[i], ref meshData);
            }
            GenerateStemMesh(settings, ref meshData);
            mesh.vertices = meshData.vertices;
            mesh.triangles = meshData.triangles;
            mesh.normals = meshData.normals;
            mesh.uv = meshData.uvs;
            mesh.RecalculateBounds();
            return mesh;
        }

        private void GenerateStemMesh(KelpSettings settings, ref MeshData meshData) {
            AddVertCircle(settings, (Segment)segments[0], settings.radialSubdivs, ref meshData);
            for (int i = 1; i < stemSegmentCount; i++) {
                int firstCircleIndex = meshData.vertexIndex - settings.radialSubdivs;
                AddVertCircle(settings, (Segment)segments[i], settings.radialSubdivs, ref meshData);
                int secondCircleIndex = meshData.vertexIndex - settings.radialSubdivs;
                for (int quad = 0; quad < settings.radialSubdivs - 1; quad++) {
                    meshData.triangles[meshData.triangleIndex++] = firstCircleIndex + quad;
                    meshData.triangles[meshData.triangleIndex++] = firstCircleIndex + quad + 1;
                    meshData.triangles[meshData.triangleIndex++] = secondCircleIndex + quad;

                    meshData.triangles[meshData.triangleIndex++] = firstCircleIndex + quad + 1;
                    meshData.triangles[meshData.triangleIndex++] = secondCircleIndex + quad + 1;
                    meshData.triangles[meshData.triangleIndex++] = secondCircleIndex + quad;
                }
                meshData.triangles[meshData.triangleIndex++] = firstCircleIndex + settings.radialSubdivs - 1;
                meshData.triangles[meshData.triangleIndex++] = firstCircleIndex;
                meshData.triangles[meshData.triangleIndex++] = secondCircleIndex + settings.radialSubdivs - 1;

                meshData.triangles[meshData.triangleIndex++] = firstCircleIndex;
                meshData.triangles[meshData.triangleIndex++] = secondCircleIndex;
                meshData.triangles[meshData.triangleIndex++] = secondCircleIndex + settings.radialSubdivs - 1;
            }
        }

        //creates a circle of vertices at the end of segment and inserts them into the vertices array
        private void AddVertCircle(KelpSettings settings, Segment seg, int radialSubdivisions, ref MeshData meshData) {
            for (int circularIndex = 0; circularIndex < radialSubdivisions; circularIndex++) {
                float alpha = ((float)circularIndex / radialSubdivisions) * Mathf.PI * 2f;

                Vector3 pos = new Vector3(Mathf.Cos(alpha) * settings.diameter, 0, Mathf.Sin(alpha) * settings.diameter);

                pos += seg.to;

                meshData.vertices[meshData.vertexIndex] = pos;// - transform.position; // from tree object coordinates to [0; 0; 0]
                meshData.normals[meshData.vertexIndex] = -new Vector3(pos.x, 0, pos.z).normalized;
                meshData.uvs[meshData.vertexIndex] = new Vector2(0.5f, 0.5f);
                meshData.vertexIndex++;
            }
        }

        private void GenerateLeafMesh(KelpSettings settings, Segment seg, ref MeshData meshData) {
            for (int i = 0; i < settings.leaf.triangles.Length; i++) {
                meshData.triangles[meshData.triangleIndex + i] = settings.leaf.triangles[i] + meshData.vertexIndex;
            }
            meshData.triangleIndex += settings.leaf.triangles.Length;
            Quaternion randomAngle = Quaternion.Euler(Random.Range(settings.minAngle, settings.maxAngle), Random.Range(0f, 360f), Random.Range(-5f, 5f));
            for (int i = 0; i < settings.leaf.vertices.Length; i++) {
                meshData.vertices[meshData.vertexIndex] = randomAngle * (settings.leaf.vertices[i] * settings.leafScale) + seg.from;
                meshData.normals[meshData.vertexIndex] = randomAngle * settings.leaf.normals[i];
                meshData.uvs[meshData.vertexIndex] = settings.leaf.uv[i];
                meshData.vertexIndex++;
            }


        }

        class MeshData {
            public Vector3[] vertices;
            public Vector3[] normals;
            public Vector2[] uvs;
            public int[] triangles;
            public int vertexIndex = 0;
            public int triangleIndex = 0;
            public MeshData(int vertexCount, int triangleIndexCount) {
                vertices = new Vector3[vertexCount];
                normals = new Vector3[vertexCount];
                uvs = new Vector2[vertexCount];
                triangles = new int[triangleIndexCount];
            }
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace PlantGeneration.SpaceColonisation {
    public class SpaceColonization : PlantGenerator{
        //public NOISE_TYPE noiseType = NOISE_TYPE.WORLEY;

        private static List<Vector3> attractorList = new List<Vector3>();
        private static List<Vector3> activeAttractors = new List<Vector3>();
        private static List<Vector3> killedAttractors = new List<Vector3>();

        //List of outer branches - those still growing
        private static List<Branch> newBranches = new List<Branch>();
        private static List<Branch> branches = new List<Branch>();


        /*     private INoise GetNoise(int seed) {
                switch (noiseType) {
                    case NOISE_TYPE.PERLIN:
                        return new PerlinNoise(seed, 20);

                    case NOISE_TYPE.VALUE:
                        return new ValueNoise(seed, 20);

                    case NOISE_TYPE.SIMPLEX:
                        return new SimplexNoise(seed, 20);

                    case NOISE_TYPE.VORONOI:
                        return new VoronoiNoise(seed, 20);

                    case NOISE_TYPE.WORLEY:
                        return new WorleyNoise(seed, 20, 1.0f);

                    default:
                        return new PerlinNoise(seed, 20);
                }
            } */

        public override void Initialize(PlantGenSettings settings){
            attractorList.Clear();
            activeAttractors.Clear();
            killedAttractors.Clear();
            branches.Clear();
            newBranches.Clear();
        }

        public override Mesh Generate(PlantGenSettings plantSettings, int seed) {
            CoralSCSettings settings = (CoralSCSettings) plantSettings;
            /* if (settings.useNoise){
                GenerateAttractorsNoiseSphere(settings.attractorCount, settings.radius, settings.attractorFieldOffset, settings.seed, settings.noiseScale);
            } else{ */
            GenerateAttractorsSphere(settings.attractorCount, settings.radius, settings.attractorFieldOffset, seed);
            //}
            Colonize(settings);
            Mesh mesh = ToMesh(settings);
            return mesh;
        }

        static void GenerateAttractorsNoiseSphere(int attractorCount, float radius, Vector3 offset, int seed, float noiseScale) {
            /* System.Random random = new System.Random(seed);
            INoise noise = GetNoise(seed);
            int notAdded = 0;
            while (attractorCount > 0 && notAdded < 10000) {
                float distFromCenter = (float)random.NextDouble();
                distFromCenter = Mathf.Pow(Mathf.Sin(distFromCenter * Mathf.PI / 2f), 0.8f);
                distFromCenter *= radius;

                //calculating random angles for direction
                float alpha = (float)random.NextDouble() * Mathf.PI;
                float theta = (float)random.NextDouble() * Mathf.PI * 2f;
                Vector3 pos = new Vector3(
                    distFromCenter * Mathf.Cos(theta) * Mathf.Sin(alpha),
                    distFromCenter * Mathf.Sin(theta) * Mathf.Sin(alpha),
                    distFromCenter * Mathf.Cos(alpha)
                );
                pos += offset;
                float noiseValueAtPos = noise.Sample3D(pos.x * noiseScale, pos.y * noiseScale, pos.z * noiseScale);
                if (noiseValueAtPos > 0.5) {
                    Debug.Log("Adding attractor");
                    attractorList.Add(pos);
                    attractorCount--;
                } else {
                    Debug.Log("Not adding attractor");
                    Debug.Log(notAdded++);
                }
            } */
        }

        static void GenerateAttractorsSphere(int attractorCount, float radius, Vector3 offset, int seed) {
            System.Random random = new System.Random(seed);
            for (int i = 0; i < attractorCount; i++) {
                //calculating random distance from center of sphere
                float distFromCenter = (float)random.NextDouble();
                distFromCenter = Mathf.Pow(Mathf.Sin(distFromCenter * Mathf.PI / 2f), 0.8f);
                distFromCenter *= radius;

                //calculating random angles for direction
                float alpha = (float)random.NextDouble() * Mathf.PI;
                float theta = (float)random.NextDouble() * Mathf.PI * 2f;
                Vector3 pos = new Vector3(
                    distFromCenter * Mathf.Cos(theta) * Mathf.Sin(alpha),
                    distFromCenter * Mathf.Sin(theta) * Mathf.Sin(alpha),
                    distFromCenter * Mathf.Cos(alpha)
                );
                pos += offset;
                attractorList.Add(pos);
            }
        }

        static void Colonize(CoralSCSettings settings) {
            branches.Add(new Branch(Vector3.zero, Vector3.up * settings.branchLength, length: settings.branchLength, maxAngleDegrees: settings.maxAngleDegrees));
            //while (attractorList.Count > 0) {
            for (int _loopCount = 0; attractorList.Count > 0 && _loopCount < settings.maxIterations; _loopCount++) {
                //assign attractors to branches
                activeAttractors.Clear();
                foreach (Vector3 currentAttractor in attractorList) {
                    Branch b = FindClosestBranch(currentAttractor, settings.attractionRange);
                    if (b != null) {
                        b.attractors.Add(currentAttractor);
                        activeAttractors.Add(currentAttractor);
                    }
                }
                //grow branches
                newBranches.Clear();
                foreach (Branch branch in branches) {
                    if (branch.attractors.Count > 0) {
                        Branch newBranch = branch.AddChild();
                        newBranches.Add(newBranch);
                        branch.attractors.Clear();
                    }
                }
                branches.AddRange(newBranches);

                //remove attractors within kill range
                for (int i = attractorList.Count - 1; i >= 0; i--) {
                    foreach (Branch b in branches) {
                        if (Vector3.Distance(b.end, attractorList[i]) < settings.killRange) {
                            killedAttractors.Add(attractorList[i]);
                            attractorList.Remove(attractorList[i]);
                            break;
                        }
                    }
                }

                if (activeAttractors.Count == 0) {
                    UnityEngine.Debug.Log("No active attractors");
                    break;
                }
            }
        }

        //returns closest branch or null if no branch is in attractionRange
        static Branch FindClosestBranch(Vector3 attractor, float range) {
            float minDist = float.MaxValue;
            Branch closest = null;
            foreach (Branch branch in branches) {
                float dist = Vector3.Distance(attractor, branch.end);
                if (dist < minDist) {
                    minDist = dist;
                    if (minDist < range) {
                        closest = branch;
                    }
                }
            }
            return closest;
        }

        //makes a mesh from the branches. The first (starting, bottom) branch does not get a mesh
        static Mesh ToMesh(CoralSCSettings settings) {
            Mesh treeMesh = new Mesh();
            int branchEndingCount = 0;
            int haveChildren = 0;
            // we first compute the size of each branch 
            for (int i = branches.Count - 1; i >= 0; i--) {
                branches[i].diameter = settings.branchBaseDiameter;
                if (!branches[i].hasChildren) {
                    branchEndingCount++;
                } else {
                    haveChildren++;
                }
            }

            //initializing vertices and triangles array
            Vector3[] vertices = new Vector3[(branches.Count + 1) * settings.branchRadialSubdivisions + branchEndingCount];
            int[] indices = new int[branches.Count * settings.branchRadialSubdivisions * 6 + branchEndingCount * settings.branchRadialSubdivisions * 3];

            int extraBranchEndVertices = 0;

            // construction of the vertices 
            for (int i = 0; i < branches.Count; i++) {
                Branch b = branches[i];

                // the index position of the vertices
                int currentVertexID = settings.branchRadialSubdivisions * (i + 1) + extraBranchEndVertices;
                b.verticeId = currentVertexID;
                b.AddVerticesToArray(settings.branchRadialSubdivisions, vertices);

                if (!b.hasChildren) {
                    Vector3 pos = (vertices[currentVertexID] + vertices[currentVertexID + settings.branchRadialSubdivisions / 2]) / 2;
                    vertices[currentVertexID + settings.branchRadialSubdivisions] = pos;
                    extraBranchEndVertices++;
                }
            }

            // faces construction; this is done in another loop because we need the parent vertices to be computed
            int indicesCounter = 0;
            foreach (Branch branch in branches) {
                indicesCounter = branch.AddIndicesToArray(indices, indicesCounter, settings.branchRadialSubdivisions);
            }

            treeMesh.vertices = vertices;
            treeMesh.triangles = indices;
            treeMesh.RecalculateNormals();
            return treeMesh;
        }

        //Represents segments between two points, one step of growth
        public class Branch {
            public Vector3 start;
            public Vector3 end;
            //direction = start - end, here to avoid frequent recalculation
            public Vector3 direction;
            public Branch parent;
            public float diameter;
            public float lastDiameter;
            public List<Branch> children;
            public List<Vector3> attractors;
            public int generation;
            public int verticeId;
            public bool finishedGrowing;
            public float maxAngleDegrees;
            public float length;
            public bool hasChildren {
                get {
                    return children.Count != 0;
                }
            }

            public Branch(Vector3 start, Vector3 end, Branch parent = null, int generation = 0, Vector3 direction = default(Vector3), float maxAngleDegrees = 90, float length = 0.05f, float diameter = 0.1f) {
                this.start = start;
                this.end = end;
                this.parent = parent;
                this.generation = generation;
                this.direction = direction;
                this.maxAngleDegrees = maxAngleDegrees;
                this.length = length;
                this.diameter = diameter;
                this.lastDiameter = diameter;
                children = new List<Branch>();
                attractors = new List<Vector3>();
                finishedGrowing = false;
            }
            public Vector3 CalculateDirection() {
                Vector3 dirSum = new Vector3();
                foreach (Vector3 currentAttractor in attractors) {
                    dirSum += currentAttractor - end;
                }
                return dirSum.normalized;
            }

            public Branch AddChild() {
                Vector3 dir = CalculateDirection();
                dir = ClampDirectionChange(this.direction, dir);
                Branch newBranch = new Branch(this.end, this.end + dir * length, this, generation + 1, dir, maxAngleDegrees, length, diameter);
                this.children.Add(newBranch);
                return newBranch;
            }

            private Vector3 ClampDirectionChange(Vector3 previousDir, Vector3 newDir) {

                Vector3 clampedDir = newDir;
                float angle = Vector3.Angle(previousDir, newDir);
                if (angle > maxAngleDegrees) {

                    clampedDir = (previousDir + newDir).normalized;
                }
                return clampedDir;
            }

            //creates a circle of vertices and inserts them into the vertices array
            public void AddVerticesToArray(int radialSubdivisions, Vector3[] vertices) {
                Quaternion quat = Quaternion.FromToRotation(Vector3.up, direction);
                for (int circularIndex = 0; circularIndex < radialSubdivisions; circularIndex++) {
                    float alpha = ((float)circularIndex / radialSubdivisions) * Mathf.PI * 2f;

                    Vector3 pos = new Vector3(Mathf.Cos(alpha) * diameter, 0, Mathf.Sin(alpha) * diameter);

                    pos = quat * pos; // rotating the circle of verts

                    pos += end;

                    vertices[verticeId + circularIndex] = pos;// - transform.position; // from tree object coordinates to [0; 0; 0]

                    if (parent == null) {
                        vertices[circularIndex] = pos - end + start;
                    }
                }
            }

            //creates the triangles. indicesCounter is where in the array it will start adding, indicesCounter+numberOfIndicesAdded
            public int AddIndicesToArray(int[] indices, int indicesCounter, int radialSubdivisions) {
                int count = indicesCounter;
                if (parent != null) {

                    int bottomVertexID = parent.verticeId;
                    int topVertexID = verticeId;

                    // construction of the faces triangles
                    for (int s = 0; s < radialSubdivisions; s++) {
                        // the triangles 
                        indices[count++] = bottomVertexID + s;
                        indices[count++] = topVertexID + s;
                        //if last in circle connect to the beginning
                        if (s == radialSubdivisions - 1) {
                            indices[count++] = topVertexID;
                            indices[count++] = bottomVertexID + s;
                            indices[count++] = topVertexID;
                            indices[count++] = bottomVertexID;
                        } else {
                            indices[count++] = topVertexID + s + 1;
                            indices[count++] = bottomVertexID + s;
                            indices[count++] = topVertexID + s + 1;
                            indices[count++] = bottomVertexID + s + 1;
                        }
                        //adding branch ending faces from the end of the indices array
                        if (!hasChildren) {
                            int endVertIndex = topVertexID + radialSubdivisions;

                            if (s == radialSubdivisions - 1) {
                                indices[count++] = topVertexID;
                                indices[count++] = topVertexID + s;
                                indices[count++] = endVertIndex;
                            } else {
                                indices[count++] = topVertexID + s + 1;
                                indices[count++] = topVertexID + s;
                                indices[count++] = endVertIndex;
                            }
                        }

                    }
                } else {
                    for (int s = 0; s < radialSubdivisions; s++) {
                        int topVertexID = verticeId;
                        indices[count++] = s;
                        indices[count++] = topVertexID + s;
                        if (s == radialSubdivisions - 1) {
                            indices[count++] = topVertexID;
                            indices[count++] = s;
                            indices[count++] = topVertexID;
                            indices[count++] = 0;
                        } else {
                            indices[count++] = topVertexID + s + 1;
                            indices[count++] = s;
                            indices[count++] = topVertexID + s + 1;
                            indices[count++] = s + 1;
                        }
                    }
                }

                return count;
            }
        }
        /* 
                void OnDrawGizmos() {
                    Gizmos.color = Color.blue;
                    foreach (Vector3 attractor in attractorList) {
                        Gizmos.DrawSphere(attractor, 0.05f);
                    }

                    Gizmos.color = Color.red;
                    foreach (Vector3 attractor in killedAttractors) {
                        Gizmos.DrawSphere(attractor, 0.05f);
                    }
                } */
    }
}
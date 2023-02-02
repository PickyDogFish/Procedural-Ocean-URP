using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RandomUtils {
    public static Vector2 RandomVector2(System.Random random) {
        return new Vector2(RandomFloat11(random), RandomFloat11(random));
    }
    public static Vector3 RandomVector3(System.Random random) {
        return new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
    }

    /// <summary>
    /// returns random float between -1 and 1
    /// </summary>
    public static float RandomFloat11(System.Random random){
        return ((float)random.NextDouble() - 0.5f) * 2;
    }

    public static Mesh CopyMesh(Mesh mesh)
    {
        Mesh newmesh = new Mesh();
        newmesh.vertices = mesh.vertices;
        newmesh.triangles = mesh.triangles;
        newmesh.uv = mesh.uv;
        newmesh.normals = mesh.normals;
        newmesh.colors = mesh.colors;
        newmesh.tangents = mesh.tangents;
        return newmesh;
    }
}

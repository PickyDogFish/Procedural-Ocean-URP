using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public static class RandomUtils
{

    const string SAVE_LOCATION = "E:/UserFolders/Pictures/";
    public static Vector2 RandomVector2(System.Random random)
    {
        return new Vector2(RandomFloat11(random), RandomFloat11(random));
    }
    public static Vector3 RandomVector3(System.Random random)
    {
        return new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
    }

    /// <summary>
    /// returns random float between -1 and 1
    /// </summary>
    public static float RandomFloat11(System.Random random)
    {
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

    public static void SaveTexture(Texture2D tex, string fileName)
    {
        byte[] bytes = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(SAVE_LOCATION + fileName, bytes);
        Debug.Log("Saved texture to " + fileName);
    }

    public static void SaveTexture(RenderTexture rTex, string fileName)
    {
        Texture2D tex = ToTexture2D(rTex);
        SaveTexture(tex, fileName);
    }

    public static Texture2D ToTexture2D(RenderTexture rTex, int res=0)
    {
        Texture2D tex;
        if (res != 0){
            tex = new Texture2D(res, res, TextureFormat.RGB24, false);
        } else {
            tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        }
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }

}

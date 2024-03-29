﻿using System.Collections.Generic;
using UnityEngine;

public class Quad // QUAD：square like stuff
{
    public Mesh mesh;

    public Quad(TypeUtility.BlockSide side, Vector3 offset, TypeUtility.BlockType bType, TypeUtility.BlockType hType)
    {
        mesh = new Mesh();
        mesh.name = "ScriptedQuad";

        Vector3[] vertices = new Vector3[4]; // four vertex points
        Vector3[] normals = new Vector3[4];
        Vector2[] uvs = new Vector2[4];
        int[] triangles = new int[] { 3, 1, 0, 3, 2, 1 }; // two triangles

        Vector2 uv00 = TypeUtility.blockUVs[(int)bType, 0]; //(int)bType for dirt is 2 -> accordingly in TypeUtility
        Vector2 uv10 = TypeUtility.blockUVs[(int)bType, 1];
        Vector2 uv01 = TypeUtility.blockUVs[(int)bType, 2];
        Vector2 uv11 = TypeUtility.blockUVs[(int)bType, 3];

        List<Vector2> secondaryUvs = new List<Vector2>();
        secondaryUvs.Add(TypeUtility.blockUVs[(int)hType, 3]);
        secondaryUvs.Add(TypeUtility.blockUVs[(int)hType, 2]);
        secondaryUvs.Add(TypeUtility.blockUVs[(int)hType, 0]);
        secondaryUvs.Add(TypeUtility.blockUVs[(int)hType, 1]);

        // Possible vertices
        Vector3 p0 = new Vector3(-0.5f, -0.5f, 0.5f) + offset;
        Vector3 p1 = new Vector3(0.5f, -0.5f, 0.5f) + offset;
        Vector3 p2 = new Vector3(0.5f, -0.5f, -0.5f) + offset;
        Vector3 p3 = new Vector3(-0.5f, -0.5f, -0.5f) + offset;
        Vector3 p4 = new Vector3(-0.5f, 0.5f, 0.5f) + offset;
        Vector3 p5 = new Vector3(0.5f, 0.5f, 0.5f) + offset;
        Vector3 p6 = new Vector3(0.5f, 0.5f, -0.5f) + offset;
        Vector3 p7 = new Vector3(-0.5f, 0.5f, -0.5f) + offset;

        switch (side)
        {
            case TypeUtility.BlockSide.FRONT:
                vertices = new Vector3[] { p4, p5, p1, p0 };
                normals = new Vector3[] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
                uvs = new Vector2[] { uv11, uv01, uv00, uv10 };
                break;
            case TypeUtility.BlockSide.BACK:
                vertices = new Vector3[] { p6, p7, p3, p2 };
                normals = new Vector3[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
                uvs = new Vector2[] { uv11, uv01, uv00, uv10 };
                break;
            case TypeUtility.BlockSide.TOP:
                vertices = new Vector3[] { p7, p6, p5, p4 };
                normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
                uvs = new Vector2[] { uv11, uv01, uv00, uv10 };
                break;
            case TypeUtility.BlockSide.BOTTOM:
                vertices = new Vector3[] { p0, p1, p2, p3 };
                normals = new Vector3[] { Vector3.down, Vector3.down, Vector3.down, Vector3.down };
                uvs = new Vector2[] { uv11, uv01, uv00, uv10 };
                break;
            case TypeUtility.BlockSide.LEFT:
                vertices = new Vector3[] { p7, p4, p0, p3 };
                normals = new Vector3[] { Vector3.left, Vector3.left, Vector3.left, Vector3.left };
                uvs = new Vector2[] { uv11, uv01, uv00, uv10 };
                break;
            case TypeUtility.BlockSide.RIGHT:
                vertices = new Vector3[] { p5, p6, p2, p1 };
                normals = new Vector3[] { Vector3.right, Vector3.right, Vector3.right, Vector3.right };
                uvs = new Vector2[] { uv11, uv01, uv00, uv10 };
                break;
        }
        
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.SetUVs(1, secondaryUvs); // second channel

        mesh.RecalculateBounds();
    }

}
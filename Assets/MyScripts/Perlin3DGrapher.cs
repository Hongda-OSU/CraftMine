using UnityEngine;

[ExecuteInEditMode]
public class Perlin3DGrapher : MonoBehaviour
{
    public Vector3 dimensions = new Vector3(10, 10, 10);
    public float heightScale, heightOffset;
    public int octaves;
    [Range(0.0f, 1.0f)]
    public float scale;
    [Range(0.0f, 10.0f)]
    public float DrawCutOff;

    public void CreateCubes()
    {
        for (int z = 0; z < dimensions.z; z++)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                for (int x = 0; x < dimensions.x; x++)
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = "Perlin_Cube";
                    cube.transform.parent = this.transform;
                    cube.transform.position = new Vector3(x, y, z);
                }
            }
        }
    }

    public void Graph()
    {
        // destroy existing cubes
        MeshRenderer[] cubes = this.GetComponentsInChildren<MeshRenderer>();
        if (cubes.Length == 0)
            CreateCubes();
        if (cubes.Length == 0) return;

        for (int z = 0; z < dimensions.z; z++)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                for (int x = 0; x < dimensions.x; x++)
                {
                    float p3d = NoiseUtility.FBM(x, y, z, octaves, scale, heightScale, heightOffset);
                    if (p3d < DrawCutOff)
                    {
                        cubes[x + (int)dimensions.x * (y + (int)dimensions.z * z)].enabled = false;
                    }
                    else
                    {
                        cubes[x + (int)dimensions.x * (y + (int)dimensions.z * z)].enabled = true;
                    }
                }
            }
        }
    }

    void OnValidate()
    {
        Graph();
    }
}

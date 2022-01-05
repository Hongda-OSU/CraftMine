using UnityEngine;

[ExecuteInEditMode]
public class PerlinGrapher : MonoBehaviour  // Perlin noise displayer
{ 
    private LineRenderer lr;
    public float heightScale, heightOffset, scale; 
    public int octaves;

    void Start()
    {
        lr = this.GetComponent<LineRenderer>();
        lr.positionCount = 100;
        Graph();
    }

    // Fractal Brownian Motion
    public float FBM(float x, float z)
    {
        float total = 0;
        float frequency = 1;
        for (int i = 0; i < octaves; i++)
        {
            total += Mathf.PerlinNoise(x * scale * frequency, z * scale * frequency) * heightScale;
            frequency *= 2;
        }
        return total;
    }

    public void Graph()
    {
        lr = this.GetComponent<LineRenderer>();
        lr.positionCount = 100;
        int z = 11; // 10 + 1
        Vector3[] positions = new Vector3[lr.positionCount];
        for (int x = 0; x < lr.positionCount; x++)
        {
            float y = FBM(x,z) + heightOffset;
            positions[x] = new Vector3(x, y, z); // height determined by perlin noice
        }
           
        lr.SetPositions(positions);
    }

    void OnValidate()
    {
        Graph();
    }

    void Update()
    {
        
    }
}

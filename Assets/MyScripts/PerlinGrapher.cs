using UnityEngine;

[ExecuteInEditMode]
public class PerlinGrapher : MonoBehaviour  // Perlin noise displayer
{ 
    public LineRenderer lr;
    public float heightScale, heightOffset;

    [Range(0.0f, 1.0f)]
    public float scale;

    [Range(0.0f, 1.0f)]
    public float probability;

    public int octaves;

    void Start()
    {
        lr = this.GetComponent<LineRenderer>();
        lr.positionCount = 100;
        Graph();
    }

    public void Graph()
    {
        lr = this.GetComponent<LineRenderer>();
        lr.positionCount = 100;
        int z = 11; // 10 + 1
        Vector3[] positions = new Vector3[lr.positionCount];
        for (int x = 0; x < lr.positionCount; x++)
        {
            float y = NoiseUtility.FBM(x, z, octaves, scale, heightScale, heightOffset);
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

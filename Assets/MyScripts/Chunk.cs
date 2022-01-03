using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;
public class Chunk : MonoBehaviour
{
    public Material material; 
    public int width, height, depth;
    public Block[,,] blocks;

    void Start()
    {
        MeshFilter mf = this.gameObject.AddComponent<MeshFilter>();
        MeshRenderer mr = this.gameObject.AddComponent<MeshRenderer>();
        mr.material = material;
        blocks = new Block[width, height, depth];

        for (int z = 0; z < depth; z++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    blocks[x, y, z] = new Block(new Vector3(x, y, z), TypeUtility.BlockType.DIRT);
                }
            }
        }
    }

    void Update()
    {
        
    }
}

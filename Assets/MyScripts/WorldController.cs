using System.Collections;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    [SerializeField] public GameObject block;
    [SerializeField] public int worldWidth;
    [SerializeField] public int worldHeight;
    [SerializeField] public int worldDepth;

    void Start()
    {
        StartCoroutine(BuildWorld());
    }

    void Update()
    {
        
    }

    public IEnumerator BuildWorld()
    {
        for (int z = 0; z < worldDepth; z++)
        {
            for (int y = 0; y < worldHeight; y++)
            {
                for (int x = 0; x < worldWidth; x++)
                {
                    if (y >= worldHeight - 2 && Random.Range(0,100) < 50) continue;
                    Vector3 pos = new Vector3(x, y, z);
                    GameObject cube = GameObject.Instantiate(block, pos, Quaternion.identity);
                    cube.name = x + "_" + y + "_" + z;
                    cube.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"));
                }
                // build one row at a time
                yield return null;
            }
        }
    }
}

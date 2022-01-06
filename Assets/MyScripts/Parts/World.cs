using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public struct PerlinSetting
{
    public float heightScale;
    public float scale;
    public int octaves;
    public float heightOffset;
    public float probability;

    public PerlinSetting(float hs, float s, int o, float ho, float p)
    {
        heightScale = hs;
        scale = s;
        octaves = o;
        heightOffset = ho;
        probability = p;
    }
}

public class World : MonoBehaviour
{
    public static Vector3 worldDimensions = new Vector3(3, 3, 3);
    public static Vector3 chunkDimensions = new Vector3(10, 10, 10);
    public GameObject chunkPrefab;
    public GameObject mCamera; // main camera
    public GameObject fpc;
    public Slider loadingBar;

    public static PerlinSetting surfaceSetting;
    public PerlinGrapher surface;

    public static PerlinSetting stoneSetting;
    public PerlinGrapher stone;

    public static PerlinSetting diamondTSetting;
    public PerlinGrapher diamondTop;

    public static PerlinSetting diamondBSetting;
    public PerlinGrapher diamondBottom;

    void Start()
    {
        loadingBar.maxValue = worldDimensions.x * worldDimensions.y * worldDimensions.z;
        surfaceSetting = new PerlinSetting(surface.heightScale, surface.scale, surface.octaves, surface.heightOffset,
            surface.probability);
        stoneSetting = new PerlinSetting(stone.heightScale, stone.scale, stone.octaves, stone.heightOffset,
            stone.probability);
        diamondTSetting = new PerlinSetting(diamondTop.heightScale, diamondTop.scale, diamondTop.octaves, diamondTop.heightOffset,
            diamondTop.probability);
        diamondBSetting = new PerlinSetting(diamondBottom.heightScale, diamondBottom.scale, diamondBottom.octaves, diamondBottom.heightOffset,
            diamondBottom.probability);
        StartCoroutine(BuildWorld());
    }

    IEnumerator BuildWorld()
    {
        for (int z = 0; z < worldDimensions.z; z++)
        {
            for (int y = 0; y < worldDimensions.y; y++)
            {
                for (int x = 0; x < worldDimensions.x; x++)
                {
                    GameObject chunk = Instantiate(chunkPrefab);
                    Vector3 position = new Vector3(x * chunkDimensions.x, y * chunkDimensions.y, z * chunkDimensions.z);
                    chunk.GetComponent<Chunk>().CreateChunk(chunkDimensions, position);
                    loadingBar.value++;
                    yield return null;
                }
            }
        }

        mCamera.SetActive(false);

        // put fpc in the middle of the world
        float xpos = worldDimensions.x * chunkDimensions.x / 2.0f;
        float zpos = worldDimensions.z * chunkDimensions.z / 2.0f;
        Chunk c = chunkPrefab.GetComponent<Chunk>();
        float ypos = NoiseUtility.FBM(xpos, zpos, stoneSetting.octaves, stoneSetting.scale, 
            stoneSetting.heightScale, stoneSetting.heightOffset) + 10;
        fpc.transform.position = new Vector3(xpos, ypos, zpos);
        loadingBar.gameObject.SetActive(false);
        fpc.SetActive(true);
    }

    void Update()
    {
        
    }
}

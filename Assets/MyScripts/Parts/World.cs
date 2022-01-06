using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class World : MonoBehaviour
{
    public static Vector3 worldDimensions = new Vector3(4, 4, 4);
    public static Vector3 chunkDimensions = new Vector3(10, 10, 10);
    public GameObject chunkPrefab;
    public GameObject mCamera; // main camera
    public GameObject fpc;
    public Slider loadingBar;

    public static NoiseUtility.PerlinSetting surfaceSetting;
    public PerlinGrapher surface;

    public static NoiseUtility.PerlinSetting stoneSetting;
    public PerlinGrapher stone;

    public static NoiseUtility.PerlinSetting diamondTSetting;
    public PerlinGrapher diamondTop;

    public static NoiseUtility.PerlinSetting diamondBSetting;
    public PerlinGrapher diamondBottom;

    public static NoiseUtility.PerlinSetting caveSetting;
    public Perlin3DGrapher cave;

    void Start()
    {
        loadingBar.maxValue = worldDimensions.x * worldDimensions.y * worldDimensions.z;
        surfaceSetting = new NoiseUtility.PerlinSetting(surface.heightScale, surface.scale, surface.octaves, surface.heightOffset,
            surface.probability);
        stoneSetting = new NoiseUtility.PerlinSetting(stone.heightScale, stone.scale, stone.octaves, stone.heightOffset,
            stone.probability);
        diamondTSetting = new NoiseUtility.PerlinSetting(diamondTop.heightScale, diamondTop.scale, diamondTop.octaves, diamondTop.heightOffset,
            diamondTop.probability);
        diamondBSetting = new NoiseUtility.PerlinSetting(diamondBottom.heightScale, diamondBottom.scale, diamondBottom.octaves, diamondBottom.heightOffset,
            diamondBottom.probability);
        caveSetting = new NoiseUtility.PerlinSetting(cave.heightScale, cave.scale, cave.octaves, cave.heightOffset,
            cave.DrawCutOff);
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

        mCamera.SetActive(false); // switch camera to fpc

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

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class World : MonoBehaviour
{
    public static Vector3Int worldDimensions = new Vector3Int(4, 4, 4);
    public static Vector3Int chunkDimensions = new Vector3Int(10, 10, 10);
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

    // Build world base on player position
    private HashSet<Vector3Int> chunkChecker = new HashSet<Vector3Int>();
    private HashSet<Vector2Int> chunkColumn = new HashSet<Vector2Int>();
    private Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();
    private Vector3Int lastBuildPosition;
    private Queue<IEnumerator> buildQueue = new Queue<IEnumerator>();
    public int drawRadius;

    void Start()
    {
        loadingBar.maxValue = worldDimensions.x * worldDimensions.z;
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

    IEnumerator BuildCoordinator()
    {
        while (true)
        {
            while (buildQueue.Count > 0)
            {
                yield return StartCoroutine(buildQueue.Dequeue());
            }
            yield return null;
        }
    }

    public void BuildChunkColumn(int x, int z)
    {
        for (int y = 0; y < worldDimensions.y; y++)
        {
            Vector3Int position = new Vector3Int(x, y * chunkDimensions.y, z);
            if (!chunkChecker.Contains(position))
            {
                GameObject chunk = Instantiate(chunkPrefab);
                chunk.name = "Chunk_" + position.x + "_" + position.y + "_" + position.z;
                Chunk c = chunk.GetComponent<Chunk>();
                c.CreateChunk(chunkDimensions, position);
                // save chunk to avoid rebuilding
                chunkChecker.Add(position);
                chunks.Add(position, c);
            }
            else
            {
                chunks[position].meshRender.enabled = true;
            }
            chunkColumn.Add(new Vector2Int(x, z));
        }
    }

    public void HideChunkColumn(int x, int z)
    {
        for (int y = 0; y < worldDimensions.y; y++)
        {
            Vector3Int position = new Vector3Int(x, y * chunkDimensions.y, z);
            if (chunkChecker.Contains(position))
            {
                chunks[position].meshRender.enabled = false;
            }
        }
    }

    IEnumerator HideColumn(int x, int z)
    {
        Vector2Int fpcPos = new Vector2Int(x, z);
        foreach (Vector2Int cc in chunkColumn)
        {
            if ((cc - fpcPos).magnitude >= drawRadius * chunkDimensions.x)
            {
                HideChunkColumn(cc.x, cc.y);
            }
        }
        yield return null;
    }

    IEnumerator BuildWorld()
    {
        for (int z = 0; z < worldDimensions.z; z++)
        {
            for (int x = 0; x < worldDimensions.x; x++)
            {
                BuildChunkColumn(x * chunkDimensions.x, z * chunkDimensions.z);
                loadingBar.value++;
                yield return null;
            }
        }

        // switch camera to fpc
        mCamera.SetActive(false); 

        // put fpc in the middle of the world
        int xpos = worldDimensions.x * chunkDimensions.x / 2;
        int zpos = worldDimensions.z * chunkDimensions.z / 2;
        int ypos = (int)NoiseUtility.FBM(xpos, zpos, stoneSetting.octaves, stoneSetting.scale, 
            stoneSetting.heightScale, stoneSetting.heightOffset) + 20;
        fpc.transform.position = new Vector3Int(xpos, ypos, zpos);
        loadingBar.gameObject.SetActive(false);
        fpc.SetActive(true);

        lastBuildPosition = Vector3Int.CeilToInt(fpc.transform.position);
        StartCoroutine(BuildCoordinator());
        StartCoroutine(UpdateWorld());
    }

    IEnumerator BuildRecursiveWorld(int x, int z, int radius)
    {
        int nextRadius = radius - 1;
        if (radius <= 0) yield break;

        BuildChunkColumn(x, z + chunkDimensions.z);
        buildQueue.Enqueue(BuildRecursiveWorld(x, z + chunkDimensions.z, nextRadius));
        yield return null;

        BuildChunkColumn(x, z - chunkDimensions.z);
        buildQueue.Enqueue(BuildRecursiveWorld(x, z - chunkDimensions.z, nextRadius));
        yield return null;

        BuildChunkColumn(x + chunkDimensions.x, z);
        buildQueue.Enqueue(BuildRecursiveWorld(x + chunkDimensions.x, z, nextRadius));
        yield return null;

        BuildChunkColumn(x - chunkDimensions.x, z);
        buildQueue.Enqueue(BuildRecursiveWorld(x - chunkDimensions.x, z, nextRadius));
        yield return null;
    }

    private WaitForSeconds wfs = new WaitForSeconds(0.5f);
    IEnumerator UpdateWorld()
    {
        while (true)
        {
            if ((lastBuildPosition - fpc.transform.position).magnitude > chunkDimensions.x)
            {
                lastBuildPosition = Vector3Int.CeilToInt(fpc.transform.position);
                int posX = (int)(fpc.transform.position.x / chunkDimensions.x) * chunkDimensions.x; // rounding fpc position
                int posZ = (int)(fpc.transform.position.z / chunkDimensions.z) * chunkDimensions.z;
                buildQueue.Enqueue(BuildRecursiveWorld(posX, posZ, drawRadius));
                buildQueue.Enqueue(HideColumn(posX, posZ));
            }
            yield return wfs;
        }
    }
}

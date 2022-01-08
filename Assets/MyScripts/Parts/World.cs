using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class World : MonoBehaviour
{
    public static Vector3Int worldDimensions = new Vector3Int(5, 5, 5);
    public static Vector3Int extraWorldDimensions = new Vector3Int(5, 5, 5);
    public static Vector3Int chunkDimensions = new Vector3Int(10, 10, 10);

    public bool loadFromFile = false;

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
    public HashSet<Vector3Int> chunkChecker = new HashSet<Vector3Int>(); // saving
    public HashSet<Vector2Int> chunkColumn = new HashSet<Vector2Int>(); // saving
    public Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>(); // saving
    private Vector3Int lastBuildPosition;
    private Queue<IEnumerator> buildQueue = new Queue<IEnumerator>();
    public int drawRadius;

    private WaitForSeconds wfs = new WaitForSeconds(0.5f);
    private WaitForSeconds threeSeconds = new WaitForSeconds(3f);
    public TypeUtility.BlockType buildType = TypeUtility.BlockType.DIRT;

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

        if (loadFromFile)
            StartCoroutine(LoadWorldFromFile());
        else
            StartCoroutine(BuildWorld());
    }

    public void SetBuildType(int type)
    {
        buildType = (TypeUtility.BlockType)type;
    }

    // Dig or Place block
    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) // 0 -> left, 1 ->right
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 10))
            {
                Vector3 hitBlock = Vector3.zero;
                if (Input.GetMouseButtonDown(0))
                    hitBlock = hit.point - hit.normal / 2.0f;
                else
                    hitBlock = hit.point + hit.normal / 2.0f;

                Chunk thisChunk = hit.collider.gameObject.GetComponent<Chunk>();
                int bx = (int)(Mathf.Round(hitBlock.x) - thisChunk.location.x);
                int by = (int)(Mathf.Round(hitBlock.y) - thisChunk.location.y);
                int bz = (int)(Mathf.Round(hitBlock.z) - thisChunk.location.z);

                Vector3Int neighbor;
                if (bx == chunkDimensions.x) // next neighbor chunk
                {
                    neighbor = new Vector3Int((int)thisChunk.location.x + chunkDimensions.x, (int)thisChunk.location.y,
                        (int)thisChunk.location.z);
                    thisChunk = chunks[neighbor];
                    bx = 0;
                }
                else if (bx == -1) // previous neighbor chunk
                {
                    neighbor = new Vector3Int((int)thisChunk.location.x - chunkDimensions.x, (int)thisChunk.location.y,
                        (int)thisChunk.location.z);
                    thisChunk = chunks[neighbor];
                    bx = chunkDimensions.x - 1;
                }
                else if (by == chunkDimensions.y) 
                {
                    neighbor = new Vector3Int((int)thisChunk.location.x, (int)thisChunk.location.y + chunkDimensions.y,
                        (int)thisChunk.location.z);
                    thisChunk = chunks[neighbor];
                    by = 0;
                }
                else if (by == -1)
                {
                    neighbor = new Vector3Int((int)thisChunk.location.x, (int)thisChunk.location.y - chunkDimensions.y,
                        (int)thisChunk.location.z);
                    thisChunk = chunks[neighbor];
                    by = chunkDimensions.y - 1;
                }
                else if (bz == chunkDimensions.z)
                {
                    neighbor = new Vector3Int((int)thisChunk.location.x, (int)thisChunk.location.y,
                        (int)thisChunk.location.z + chunkDimensions.z);
                    thisChunk = chunks[neighbor];
                    bz = 0;
                }
                else if (bz == -1) 
                {
                    neighbor = new Vector3Int((int)thisChunk.location.x, (int)thisChunk.location.y,
                        (int)thisChunk.location.z - chunkDimensions.z);
                    thisChunk = chunks[neighbor];
                    bz = chunkDimensions.z - 1;
                }

                int i = bx + chunkDimensions.x * (by + chunkDimensions.z * bz);
                if (Input.GetMouseButtonDown(0))
                {
                    if (TypeUtility.blockTypeHealth[(int) thisChunk.chunkData[i]] != -1) // breakable
                    {
                        if (thisChunk.healthData[i] == TypeUtility.BlockType.NOCRACK)
                            StartCoroutine(HealBlock(thisChunk, i));
                        thisChunk.healthData[i]++;
                        if (thisChunk.healthData[i] == TypeUtility.BlockType.NOCRACK + TypeUtility.blockTypeHealth[(int)thisChunk.chunkData[i]])
                            thisChunk.chunkData[i] = TypeUtility.BlockType.AIR;
                    }
                }
                else
                {
                    thisChunk.chunkData[i] = buildType;
                    thisChunk.healthData[i] = TypeUtility.BlockType.NOCRACK;
                }
                ReDrawChunk(thisChunk);
            }
        }
    }

    public void ReDrawChunk(Chunk c)
    {
        DestroyImmediate(c.GetComponent<MeshFilter>());
        DestroyImmediate(c.GetComponent<MeshRenderer>());
        DestroyImmediate(c.GetComponent<Collider>());
        c.CreateChunk(chunkDimensions, c.location, false);
    }

    IEnumerator HealBlock(Chunk c, int blockIndex)
    {
        yield return threeSeconds;
        if (c.chunkData[blockIndex] != TypeUtility.BlockType.AIR)
        {
            c.healthData[blockIndex] = TypeUtility.BlockType.NOCRACK;
            ReDrawChunk(c);
        }
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

    public void BuildChunkColumn(int x, int z, bool meshEnabled = true)
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
            chunks[position].meshRender.enabled = meshEnabled;
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
        //StartCoroutine(UpdateWorld());
        StartCoroutine(BuildExtraWorld());
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

    IEnumerator BuildExtraWorld()
    {
        int zEnd = worldDimensions.z + extraWorldDimensions.z;
        int zStart = worldDimensions.z;

        int xEnd = worldDimensions.x + extraWorldDimensions.x;
        int xStart = worldDimensions.x;

        for (int z = zStart; z < zEnd; z++)
        {
            for (int x = 0; x < xEnd; x++)
            {
                BuildChunkColumn(x * chunkDimensions.x, z * chunkDimensions.z, false);
                yield return null;
            }
        }

        for (int z = 0; z < zEnd; z++)
        {
            for (int x = xStart; x <xEnd; x++)
            {
                BuildChunkColumn(x * chunkDimensions.x, z * chunkDimensions.z, false);
                yield return null;
            }
        }
    }

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

    public void SaveWorld()
    {
        FileSaver.Save(this);
    }

    IEnumerator LoadWorldFromFile()
    {
        WorldData wd = FileSaver.Load();
        if (wd == null)
        {
            StartCoroutine(BuildWorld());
            yield break;
        }
        chunkChecker.Clear();
        for (int i = 0; i < wd.chunkCheckerValues.Length; i += 3)
        {
            chunkChecker.Add(new Vector3Int(wd.chunkCheckerValues[i], wd.chunkCheckerValues[i + 1],
                wd.chunkCheckerValues[i + 2]));
        }
        chunkColumn.Clear();
        for (int i = 0; i < wd.chunkColumnValues.Length; i += 2)
        {
            chunkColumn.Add(new Vector2Int(wd.chunkColumnValues[i], wd.chunkColumnValues[i + 1]));
        }

        int index = 0;
        foreach (Vector3Int chunkPos in chunkChecker)
        {
            GameObject chunk = Instantiate(chunkPrefab);
            chunk.name = "Chunk_" + chunkPos.x + "_" + chunkPos.y + "_" + chunkPos.z;
            Chunk c = chunk.GetComponent<Chunk>();
            int blockCount = chunkDimensions.x * chunkDimensions.y * chunkDimensions.z;
            c.chunkData = new TypeUtility.BlockType[blockCount];
            c.healthData = new TypeUtility.BlockType[blockCount];

            for (int i = 0; i < blockCount; i++)
            {
                c.chunkData[i] = (TypeUtility.BlockType) wd.allChunkData[index];
                c.healthData[i] = TypeUtility.BlockType.NOCRACK;
                index++;
            }

            c.CreateChunk(chunkDimensions, chunkPos, false);
            chunks.Add(chunkPos, c);
            ReDrawChunk(c);
            yield return null;
        }

        fpc.transform.position = new Vector3(wd.fpcX, wd.fpcY, wd.fpcZ);
        mCamera.SetActive(false);
        fpc.SetActive(true);
        lastBuildPosition = Vector3Int.CeilToInt(fpc.transform.position);
    }
}

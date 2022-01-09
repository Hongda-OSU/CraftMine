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

    public static NoiseUtility.PerlinSetting treeSetting;
    public Perlin3DGrapher tree;

    public static NoiseUtility.PerlinSetting biomeSetting;
    public Perlin3DGrapher biome;

    // Build world base on player position
    public HashSet<Vector3Int> chunkChecker = new HashSet<Vector3Int>(); // saving
    public HashSet<Vector2Int> chunkColumn = new HashSet<Vector2Int>(); // saving
    public Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>(); // saving
    private Vector3Int lastBuildPosition;
    private Queue<IEnumerator> buildQueue = new Queue<IEnumerator>();
    public int drawRadius;

    private WaitForSeconds wfs = new WaitForSeconds(0.5f);
    private WaitForSeconds threeSeconds = new WaitForSeconds(3f);
    private WaitForSeconds dropDelay = new WaitForSeconds(0.1f);
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
        treeSetting = new NoiseUtility.PerlinSetting(tree.heightScale, tree.scale, tree.octaves, tree.heightOffset,
            tree.DrawCutOff);
        biomeSetting = new NoiseUtility.PerlinSetting(biome.heightScale, biome.scale, biome.octaves, biome.heightOffset,
            biome.DrawCutOff);

        if (loadFromFile)
            StartCoroutine(LoadWorldFromFile());
        else
            StartCoroutine(BuildWorld());
    }

    public void SetBuildType(int type)
    {
        buildType = (TypeUtility.BlockType)type;
    }

    public static Vector3Int FromFlat(int i)
    {
        return new Vector3Int(i % chunkDimensions.x, (i / chunkDimensions.x) % chunkDimensions.y,
            i / (chunkDimensions.x * chunkDimensions.y));
    }

    public static int ToFlat(Vector3Int v)
    {
        return v.x + chunkDimensions.x * (v.y + chunkDimensions.z * v.z);
    }

    public System.Tuple<Vector3Int, Vector3Int> GetWorldNeighbor(Vector3Int blockIndex, Vector3Int chunkIndex)
    {
        Chunk thisChunk = chunks[chunkIndex];
        int bx = blockIndex.x;
        int by = blockIndex.y;
        int bz = blockIndex.z;

        Vector3Int neighbor = chunkIndex;
        if (bx == chunkDimensions.x) // next neighbor chunk
        {
            neighbor = new Vector3Int((int)thisChunk.location.x + chunkDimensions.x, (int)thisChunk.location.y,
                (int)thisChunk.location.z);
            bx = 0;
        }
        else if (bx == -1) // previous neighbor chunk
        {
            neighbor = new Vector3Int((int)thisChunk.location.x - chunkDimensions.x, (int)thisChunk.location.y,
                (int)thisChunk.location.z);
            bx = chunkDimensions.x - 1;
        }
        else if (by == chunkDimensions.y)
        {
            neighbor = new Vector3Int((int)thisChunk.location.x, (int)thisChunk.location.y + chunkDimensions.y,
                (int)thisChunk.location.z);
            by = 0;
        }
        else if (by == -1)
        {
            neighbor = new Vector3Int((int)thisChunk.location.x, (int)thisChunk.location.y - chunkDimensions.y,
                (int)thisChunk.location.z);
            by = chunkDimensions.y - 1;
        }
        else if (bz == chunkDimensions.z)
        {
            neighbor = new Vector3Int((int)thisChunk.location.x, (int)thisChunk.location.y,
                (int)thisChunk.location.z + chunkDimensions.z);
            bz = 0;
        }
        else if (bz == -1)
        {
            neighbor = new Vector3Int((int)thisChunk.location.x, (int)thisChunk.location.y,
                (int)thisChunk.location.z - chunkDimensions.z);
            bz = chunkDimensions.z - 1;
        }

        return new System.Tuple<Vector3Int, Vector3Int>(new Vector3Int(bx, by, bz), neighbor);
    }

    // Dig or Place block
    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) // 0 -> left, 1 ->right
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            int bitMask = ~(1 << 8);
            if (Physics.Raycast(ray, out hit, 10, bitMask))
            {
                Vector3 hitBlock = Vector3.zero;
                if (Input.GetMouseButtonDown(0))
                    hitBlock = hit.point - hit.normal / 2.0f;
                else
                    hitBlock = hit.point + hit.normal / 2.0f;

                Chunk thisChunk = hit.collider.gameObject.transform.parent.GetComponent<Chunk>();
                int bx = (int)(Mathf.Round(hitBlock.x) - thisChunk.location.x);
                int by = (int)(Mathf.Round(hitBlock.y) - thisChunk.location.y);
                int bz = (int)(Mathf.Round(hitBlock.z) - thisChunk.location.z);

                var blockNeighbor = GetWorldNeighbor(new Vector3Int(bx, by, bz), 
                    Vector3Int.CeilToInt(thisChunk.location));
                thisChunk = chunks[blockNeighbor.Item2];
                int i = ToFlat(blockNeighbor.Item1);

                if (Input.GetMouseButtonDown(0))
                {
                    if (TypeUtility.blockTypeHealth[(int) thisChunk.chunkData[i]] != -1) // breakable
                    {
                        if (thisChunk.healthData[i] == TypeUtility.BlockType.NOCRACK)
                            StartCoroutine(HealBlock(thisChunk, i));
                        thisChunk.healthData[i]++;
                        if (thisChunk.healthData[i] == TypeUtility.BlockType.NOCRACK +
                            TypeUtility.blockTypeHealth[(int) thisChunk.chunkData[i]])
                        {
                            thisChunk.chunkData[i] = TypeUtility.BlockType.AIR;
                            Vector3Int nBlock = FromFlat(i);
                            var neighborBlock = GetWorldNeighbor(new Vector3Int(nBlock.x, nBlock.y + 1, nBlock.z),
                                Vector3Int.CeilToInt(thisChunk.location));
                            Vector3Int block = neighborBlock.Item1;
                            int neighborBlockIndex = ToFlat(block);
                            Chunk neighborChunk = chunks[neighborBlock.Item2];
                            StartCoroutine(Drop(neighborChunk, neighborBlockIndex));
                        }
                    }
                }
                else
                {
                    thisChunk.chunkData[i] = buildType;
                    thisChunk.healthData[i] = TypeUtility.BlockType.NOCRACK;
                    StartCoroutine(Drop(thisChunk, i));
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
            chunks[position].meshRenderSolid.enabled = meshEnabled;
            chunks[position].meshRenderFluid.enabled = meshEnabled;
        }
        chunkColumn.Add(new Vector2Int(x, z));
    }

    public void HideChunkColumn(int x, int z)
    {
        for (int y = 0; y < worldDimensions.y; y++)
        {
            Vector3Int position = new Vector3Int(x, y * chunkDimensions.y, z);
            if (chunkChecker.Contains(position))
            {
                chunks[position].meshRenderSolid.enabled = false;
                chunks[position].meshRenderFluid.enabled = false;
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

    IEnumerator Drop(Chunk c, int blockIndex, int strength = 3)
    {
        if (!TypeUtility.canDrop.Contains(c.chunkData[blockIndex])) // sand for now
            yield break;

        yield return dropDelay;
        while (true)
        {
            Vector3Int thisBlock = FromFlat(blockIndex);
            var neighborBlock = GetWorldNeighbor(new Vector3Int(thisBlock.x, thisBlock.y - 1, thisBlock.z),
                Vector3Int.CeilToInt(c.location));
            Vector3Int block = neighborBlock.Item1;
            int neighborBlockIndex = ToFlat(block);
            Chunk neightborChunk = chunks[neighborBlock.Item2];
            if (neightborChunk != null && neightborChunk.chunkData[neighborBlockIndex] == TypeUtility.BlockType.AIR)
            {
                neightborChunk.chunkData[neighborBlockIndex] = c.chunkData[blockIndex];
                neightborChunk.healthData[neighborBlockIndex] = TypeUtility.BlockType.NOCRACK;
                c.chunkData[blockIndex] = TypeUtility.BlockType.AIR;
                var nBlockAbove = GetWorldNeighbor(new Vector3Int(thisBlock.x, thisBlock.y + 1, thisBlock.z),
                    Vector3Int.CeilToInt(c.location));
                Vector3Int blockAbove = nBlockAbove.Item1;
                int nBlockAboveIndex = ToFlat(blockAbove);
                Chunk nChunkAbove = chunks[nBlockAbove.Item2];
                c.chunkData[blockIndex] = TypeUtility.BlockType.AIR;
                c.healthData[blockIndex] = TypeUtility.BlockType.NOCRACK;
                StartCoroutine(Drop(nChunkAbove, nBlockAboveIndex));
                yield return dropDelay;
                ReDrawChunk(c);
                if (neightborChunk != c)
                {
                    ReDrawChunk(neightborChunk);
                }
                c = neightborChunk;
                blockIndex = neighborBlockIndex;
            }
            else if (TypeUtility.canFlow.Contains(c.chunkData[blockIndex]))
            {
                FlowIntoNeighbor(thisBlock, Vector3Int.CeilToInt(c.location), new Vector3Int(1,0,0), strength-1);
                FlowIntoNeighbor(thisBlock, Vector3Int.CeilToInt(c.location), new Vector3Int(-1, 0, 0), strength - 1);
                FlowIntoNeighbor(thisBlock, Vector3Int.CeilToInt(c.location), new Vector3Int(0, 0, 1), strength - 1);
                FlowIntoNeighbor(thisBlock, Vector3Int.CeilToInt(c.location), new Vector3Int(0, 0, -1), strength - 1);
                yield break;
            }
            else
            {
                yield break;
            }
        }
    }

    public void FlowIntoNeighbor(Vector3Int blockPosition, Vector3Int chunkPosition, Vector3Int neighborDirection,
        int strength)
    {
        strength--;
        if (strength <= 0) return;
        Vector3Int neighborPosition = blockPosition + neighborDirection;
        var neighborBlock = GetWorldNeighbor(neighborPosition, chunkPosition);
        Vector3Int block = neighborBlock.Item1;
        int neighborBlockIndex = ToFlat(block);
        Chunk neighborChunk = chunks[neighborBlock.Item2];
        if (neighborChunk == null) return;
        if (neighborChunk.chunkData[neighborBlockIndex] == TypeUtility.BlockType.AIR)
        {
            neighborChunk.chunkData[neighborBlockIndex] = chunks[chunkPosition].chunkData[ToFlat(blockPosition)];
            neighborChunk.healthData[neighborBlockIndex] = TypeUtility.BlockType.NOCRACK;
            ReDrawChunk(neighborChunk);
            StartCoroutine(Drop(neighborChunk, neighborBlockIndex, strength--));
        }
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
            stoneSetting.heightScale, stoneSetting.heightOffset) + 50;
        fpc.transform.position = new Vector3Int(xpos, ypos, zpos);
        loadingBar.gameObject.SetActive(false);
        fpc.SetActive(true);

        lastBuildPosition = Vector3Int.CeilToInt(fpc.transform.position);
        StartCoroutine(BuildCoordinator());
        StartCoroutine(UpdateWorld());
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
        int vIndex = 0;
        loadingBar.maxValue = chunkChecker.Count;
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
            loadingBar.value++;
            ReDrawChunk(c);
            c.meshRenderSolid.enabled = wd.chunkVisibility[vIndex];
            c.meshRenderFluid.enabled = wd.chunkVisibility[vIndex];
            vIndex++;
            yield return null;
        }

        fpc.transform.position = new Vector3(wd.fpcX, wd.fpcY, wd.fpcZ);
        mCamera.SetActive(false);
        fpc.SetActive(true);
        lastBuildPosition = Vector3Int.CeilToInt(fpc.transform.position);
        loadingBar.gameObject.SetActive(false);
        StartCoroutine(BuildCoordinator());
        StartCoroutine(UpdateWorld());
    }
}

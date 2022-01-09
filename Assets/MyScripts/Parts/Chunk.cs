using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine.Rendering;
using Random = Unity.Mathematics.Random;

public class Chunk : MonoBehaviour
{
    public Material material; 
    public int width, height, depth;
    public Block[,,] blocks;
    // (flatten array) Flat[x + WIDTH * (y + DEPTH * z)] = Original[x,y,z] 
    // x = i % WIDTH
    // y = (i / WIDTH) % HEIGHT
    // z = i / (WIDTH * HEIGHT)
    public TypeUtility.BlockType[] chunkData;
    public TypeUtility.BlockType[] healthData;
    public Vector3 location;
    public MeshRenderer meshRender;

    private CalculateBlockTypes calculateBlockTypes;
    private JobHandle jobHandle;
    public NativeArray<Unity.Mathematics.Random> RandomArray { private get; set; }

    struct CalculateBlockTypes : IJobParallelFor
    {
        public NativeArray<TypeUtility.BlockType> cData;
        public NativeArray<TypeUtility.BlockType> hData;
        public int width;
        public int height;
        public Vector3 location;
        public NativeArray<Unity.Mathematics.Random> randoms;

        public void Execute(int i)
        {
            int x = i % width + (int)location.x;
            int y = (i / width) % height + (int)location.y;
            int z = i / (width * height) + (int)location.z;
            var random = randoms[i];

            // using perline noise, lower offset -> more blocks in a chunk
            int surfaceHeight = (int)NoiseUtility.FBM(x, z, World.surfaceSetting.octaves, World.surfaceSetting.scale,
                World.surfaceSetting.heightScale, World.surfaceSetting.heightOffset);

            int stoneHeight = (int)NoiseUtility.FBM(x, z, World.stoneSetting.octaves, World.stoneSetting.scale,
                World.stoneSetting.heightScale, World.stoneSetting.heightOffset);

            int diamondTHeight = (int)NoiseUtility.FBM(x, z, World.diamondTSetting.octaves, World.diamondTSetting.scale,
                World.diamondTSetting.heightScale, World.diamondTSetting.heightOffset);

            int diamondBHeight = (int)NoiseUtility.FBM(x, z, World.diamondBSetting.octaves, World.diamondBSetting.scale,
                World.diamondBSetting.heightScale, World.diamondBSetting.heightOffset);

            int digCave = (int)NoiseUtility.FBM(x, y, z, World.caveSetting.octaves, World.caveSetting.scale,
                World.caveSetting.heightScale, World.caveSetting.heightOffset);

            int plantTree = (int)NoiseUtility.FBM(x, y, z, World.treeSetting.octaves, World.treeSetting.scale,
                World.treeSetting.heightScale, World.treeSetting.heightOffset);

            hData[i] = TypeUtility.BlockType.NOCRACK;

            // bedrock creation
            if (y == 0)
            {
                cData[i] = TypeUtility.BlockType.BEDROCK;
                return;
            }

            // cave creation
            if (digCave < World.caveSetting.probability)
            {
                cData[i] = TypeUtility.BlockType.AIR;
                return;
            }

            if (surfaceHeight == y)
            {
                if (plantTree < World.treeSetting.probability && random.NextFloat(1) <= 0.1f) // tree
                    cData[i] = TypeUtility.BlockType.WOODBASE;
                else
                    cData[i] = TypeUtility.BlockType.GRASSSIDE;
            }
            else if (diamondTHeight > y && diamondBHeight < y && random.NextFloat(1) <= World.diamondTSetting.probability)
            {
                cData[i] = TypeUtility.BlockType.DIAMOND;
            }
            else if (stoneHeight > y && random.NextFloat(1) <= World.stoneSetting.probability)
            {
                cData[i] = TypeUtility.BlockType.STONE;
            }
            else if (surfaceHeight > y)
            {
                cData[i] = TypeUtility.BlockType.DIRT;
            }
            else
            {
                cData[i] = TypeUtility.BlockType.AIR;
            }
        }
    }

    public void BuildChunk()
    {
        int blockCount = width * depth * height;
        chunkData = new TypeUtility.BlockType[blockCount];
        healthData = new TypeUtility.BlockType[blockCount];
        NativeArray<TypeUtility.BlockType> blockTypes = new NativeArray<TypeUtility.BlockType>(chunkData, Allocator.Persistent);
        NativeArray<TypeUtility.BlockType> healthTypes = new NativeArray<TypeUtility.BlockType>(healthData, Allocator.Persistent);

        var randomArray = new Unity.Mathematics.Random[blockCount];
        var seed = new System.Random();
        for (int i = 0; i < blockCount; i++)
            randomArray[i] = new Unity.Mathematics.Random((uint) seed.Next());
        RandomArray = new NativeArray<Random>(randomArray, Allocator.Persistent);

        calculateBlockTypes = new CalculateBlockTypes()
        {
            cData = blockTypes,
            hData = healthTypes,
            width = width,
            height = height,
            location = location,
            randoms = RandomArray
        };
        jobHandle = calculateBlockTypes.Schedule(chunkData.Length, 64);
        jobHandle.Complete();
        calculateBlockTypes.cData.CopyTo(chunkData);
        calculateBlockTypes.hData.CopyTo(healthData);
        blockTypes.Dispose();
        healthTypes.Dispose();
        RandomArray.Dispose();

        BuildTree();
    }

    public (Vector3Int, TypeUtility.BlockType)[] treeDesign = new (Vector3Int, TypeUtility.BlockType)[]
    {
        (new Vector3Int(0,1,-1), TypeUtility.BlockType.LEAVES),
        (new Vector3Int(1,1,-1), TypeUtility.BlockType.LEAVES),
        (new Vector3Int(-1,2,-1), TypeUtility.BlockType.LEAVES),
        (new Vector3Int(0,2,-1), TypeUtility.BlockType.LEAVES),
        (new Vector3Int(0,3,-1), TypeUtility.BlockType.LEAVES),
        (new Vector3Int(0,4,-1), TypeUtility.BlockType.LEAVES),
        (new Vector3Int(1,4,-1), TypeUtility.BlockType.LEAVES),
        (new Vector3Int(0,0,0), TypeUtility.BlockType.WOOD),
        (new Vector3Int(-1,1,0), TypeUtility.BlockType.LEAVES),
        (new Vector3Int(0,1,0), TypeUtility.BlockType.WOOD),
        (new Vector3Int(1,1,0), TypeUtility.BlockType.LEAVES),
        (new Vector3Int(-1,2,0), TypeUtility.BlockType.LEAVES),
        (new Vector3Int(0,2,0), TypeUtility.BlockType.WOOD),
        (new Vector3Int(1,2,0), TypeUtility.BlockType.LEAVES),
        (new Vector3Int(-1,3,0), TypeUtility.BlockType.LEAVES),
        (new Vector3Int(0,3,0), TypeUtility.BlockType.WOOD),
        (new Vector3Int(1,3,0), TypeUtility.BlockType.LEAVES),
        (new Vector3Int(-1,4,0), TypeUtility.BlockType.LEAVES),
        (new Vector3Int(0,4,0), TypeUtility.BlockType.WOOD),
        (new Vector3Int(1,4,0), TypeUtility.BlockType.LEAVES),
        (new Vector3Int(0,5,0), TypeUtility.BlockType.LEAVES),
        (new Vector3Int(0,1,1), TypeUtility.BlockType.LEAVES),
        (new Vector3Int(1,1,1), TypeUtility.BlockType.LEAVES),
        (new Vector3Int(-1,2,1), TypeUtility.BlockType.LEAVES),
        (new Vector3Int(0,2,1), TypeUtility.BlockType.LEAVES),
        (new Vector3Int(0,3,1), TypeUtility.BlockType.LEAVES),
        (new Vector3Int(1,3,1), TypeUtility.BlockType.LEAVES),
        (new Vector3Int(-1,4,1), TypeUtility.BlockType.LEAVES),
        (new Vector3Int(0,4,1), TypeUtility.BlockType.LEAVES),
    };

    public void BuildTree()
    {
        for (int i = 0; i < chunkData.Length; i++)
        {
            if (chunkData[i] == TypeUtility.BlockType.WOODBASE)
            {
                foreach ((Vector3Int, TypeUtility.BlockType) v in treeDesign)
                {
                    Vector3Int blockPos = World.FromFlat(i) + v.Item1;
                    int bIndex = World.ToFlat(blockPos);
                    if (bIndex >= 0 && bIndex < chunkData.Length)
                    {
                        chunkData[bIndex] = v.Item2;
                        healthData[bIndex] = TypeUtility.BlockType.NOCRACK;
                    }
                }
            }
        }
    }

    public void CreateChunk(Vector3 dimension, Vector3 position, bool rebuildBlocks = true)
    {
        location = position;
        width = (int) dimension.x;
        height = (int) dimension.y;
        depth = (int) dimension.z;

        MeshFilter mf = this.gameObject.AddComponent<MeshFilter>();
        MeshRenderer mr = this.gameObject.AddComponent<MeshRenderer>();
        meshRender = mr;
        mr.material = material;
        blocks = new Block[width, height, depth];
        if (rebuildBlocks) 
            BuildChunk();

        List<Mesh> inputMeshes = new List<Mesh>();
        int vertexStart = 0, triStart = 0, counter = 0;
        int meshCount = width * height * depth;
        var jobs = new ProcessMeshDataJob();
        jobs.vertexStart = new NativeArray<int>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        jobs.triStart = new NativeArray<int>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        for (int z = 0; z < depth; z++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    blocks[x, y, z] = new Block(new Vector3(x, y, z) + location, chunkData[x + width * (y + depth * z)],
                        healthData[x + width * (y + depth * z)], this);
                    if (blocks[x,y,z].mesh == null) continue;
                    inputMeshes.Add(blocks[x, y, z].mesh);
                    var vcount = blocks[x, y, z].mesh.vertexCount;
                    var icount = (int)blocks[x, y, z].mesh.GetIndexCount(0);
                    jobs.vertexStart[counter] = vertexStart;
                    jobs.triStart[counter] = triStart;
                    vertexStart += vcount;
                    triStart += icount;
                    counter++;
                }
            }
        }

        jobs.meshData = Mesh.AcquireReadOnlyMeshData(inputMeshes);
        var outputMeshData = Mesh.AllocateWritableMeshData(1);
        jobs.outputMesh = outputMeshData[0];
        jobs.outputMesh.SetIndexBufferParams(triStart, IndexFormat.UInt32);
        jobs.outputMesh.SetVertexBufferParams(vertexStart, 
            new VertexAttributeDescriptor(VertexAttribute.Position, stream: 0), 
            new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 1),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, stream: 2),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord1, stream: 3)); // second uv set

        var handle = jobs.Schedule(inputMeshes.Count, 4);
        var newMesh = new Mesh();
        newMesh.name = "Chunk_" + location.x + "_" + location.y + "_" + location.z; 
        var subMesh = new SubMeshDescriptor(0, triStart, MeshTopology.Triangles);
        subMesh.firstVertex = 0;
        subMesh.vertexCount = vertexStart;

        handle.Complete();

        jobs.outputMesh.subMeshCount = 1;
        jobs.outputMesh.SetSubMesh(0, subMesh);

        Mesh.ApplyAndDisposeWritableMeshData(outputMeshData, new[] {newMesh});
        jobs.meshData.Dispose();
        jobs.vertexStart.Dispose();
        jobs.triStart.Dispose();
        newMesh.RecalculateBounds();

        mf.mesh = newMesh;
        MeshCollider collider = this.gameObject.AddComponent<MeshCollider>();
        collider.sharedMesh = mf.mesh;
    }

    [BurstCompile]
    struct ProcessMeshDataJob : IJobParallelFor
    {
        [ReadOnly] public Mesh.MeshDataArray meshData;
        public Mesh.MeshData outputMesh;
        public NativeArray<int> vertexStart;
        public NativeArray<int> triStart;

        public void Execute(int index)
        {
            Mesh.MeshData data = meshData[index];
            var vCount = data.vertexCount;
            var vStart = vertexStart[index];

            var verts = new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            data.GetVertices(verts.Reinterpret<Vector3>());

            var normals = new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            data.GetNormals(normals.Reinterpret<Vector3>());

            var uvs = new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            data.GetUVs(0, uvs.Reinterpret<Vector3>());

            var uv2s = new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            data.GetUVs(1, uv2s.Reinterpret<Vector3>());

            var outputVerts = outputMesh.GetVertexData<Vector3>(stream: 0);
            var outputNormals = outputMesh.GetVertexData<Vector3>(stream: 1);
            var outputUVs = outputMesh.GetVertexData<Vector3>(stream: 2);
            var outputUV2s = outputMesh.GetVertexData<Vector3>(stream: 3);

            for (int i = 0; i < vCount; i++)
            {
                outputVerts[i + vStart] = verts[i];
                outputNormals[i + vStart] = normals[i];
                outputUVs[i + vStart] = uvs[i];
                outputUV2s[i + vStart] = uv2s[i];
            }

            // Prevent memory leak
            verts.Dispose();
            normals.Dispose();
            uvs.Dispose();
            uv2s.Dispose();

            var tStart = triStart[index];
            var tCount = data.GetSubMesh(0).indexCount;
            var outputTris = outputMesh.GetIndexData<int>();
            if (data.indexFormat == IndexFormat.UInt16)
            {
                var tris = data.GetIndexData<ushort>();
                for (int i = 0; i < tCount; i++)
                {
                    int idx = tris[i];
                    outputTris[i + tStart] = vStart + idx;
                }
            }
            else
            {
                var tris = data.GetIndexData<int>();
                for (int i = 0; i < tCount; i++)
                {
                    int idx = tris[i];
                    outputTris[i + tStart] = vStart + idx;
                }
            }
        }
    }
}

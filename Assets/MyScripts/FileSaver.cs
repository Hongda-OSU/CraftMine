using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WorldData
{
    //private HashSet<Vector3Int> chunkChecker = new HashSet<Vector3Int>(); 
    //private HashSet<Vector2Int> chunkColumn = new HashSet<Vector2Int>(); 
    //private Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>(); 

    public int[] chunkCheckerValues;
    public int[] chunkColumnValues;
    public int[] allChunkData;

    public int fpcX;
    public int fpcY;
    public int fpcZ;

    public WorldData() { }

    public WorldData(HashSet<Vector3Int> cc, HashSet<Vector2Int> cCols, Dictionary<Vector3Int, Chunk> chks, Vector3 fpc)
    {
        chunkCheckerValues = new int[cc.Count * 3];
        int index = 0;
        foreach (Vector3Int v in cc)
        {
            chunkCheckerValues[index] = v.x;
            chunkCheckerValues[index + 1] = v.y;
            chunkCheckerValues[index + 2] = v.z;
            index += 3;
        }

        chunkColumnValues = new int[cCols.Count * 2];
        index = 0;
        foreach (Vector2Int v in cCols)
        {
            chunkColumnValues[index] = v.x;
            chunkColumnValues[index + 1] = v.y;
            index += 2;
        }

        allChunkData = new int[chks.Count * World.chunkDimensions.x * World.chunkDimensions.y * World.chunkDimensions.z];
        index = 0;
        foreach (var ch in chks)
        {
            foreach (TypeUtility.BlockType bt in ch.Value.chunkData)
            {
                allChunkData[index] = (int) bt;
                index++;
            }
        }

        fpcX = (int) fpc.x;
        fpcY = (int) fpc.y;
        fpcZ = (int) fpc.z;
    }
}

public static class FileSaver
{
    private static WorldData wd;

    private static string BuildFileName()
    {
        return Application.persistentDataPath + "/savedata/World_" + 
               World.chunkDimensions.x + "_" +
               World.chunkDimensions.y + "_" + 
               World.chunkDimensions.z + "_" +
               World.worldDimensions.x + "_" + 
               World.worldDimensions.y + "_" + 
               World.worldDimensions.z + ".dat";
    }

    public static void Save(World world)
    {
        string fileName = BuildFileName();
        if (!File.Exists(fileName))
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(fileName, FileMode.OpenOrCreate);
        wd = new WorldData(world.chunkChecker, world.chunkColumn, world.chunks, world.fpc.transform.position);
        bf.Serialize(file, wd);
        file.Close();
        Debug.Log("Saving World to File: " + fileName);
    }

    public static WorldData Load()
    {
        string fileName = BuildFileName();
        if (File.Exists(fileName))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(fileName, FileMode.Open);
            wd = new WorldData();
            wd = (WorldData) bf.Deserialize(file);
            file.Close();
            Debug.Log("Loading World from File: " + fileName);
            return wd;
        }
        return null;
    }


}

using System.Collections.Generic;
using UnityEngine;

public class Block
{
    public Mesh mesh;
    public Chunk parentChunk;

    public Block(Vector3 position, TypeUtility.BlockType bType, Chunk chunk)
    {
        parentChunk = chunk;
        
        if(bType == TypeUtility.BlockType.AIR) return;

        List<Quad> quads = new List<Quad>();
        if (!hasSolidNeighbor((int) position.x, (int) position.y - 1, (int) position.z))
            quads.Add(new Quad(TypeUtility.BlockSide.BOTTOM, position, bType));
        
        if (!hasSolidNeighbor((int) position.x, (int) position.y + 1, (int) position.z))
            quads.Add(new Quad(TypeUtility.BlockSide.TOP, position, bType));
        
        if (!hasSolidNeighbor((int) position.x - 1, (int) position.y, (int) position.z))
            quads.Add(new Quad(TypeUtility.BlockSide.LEFT, position, bType));
        
        if (!hasSolidNeighbor((int)position.x + 1, (int)position.y, (int)position.z))
            quads.Add(new Quad(TypeUtility.BlockSide.RIGHT, position, bType));

        if (!hasSolidNeighbor((int) position.x, (int) position.y, (int) position.z + 1))
            quads.Add(new Quad(TypeUtility.BlockSide.FRONT, position, bType));

        if (!hasSolidNeighbor((int)position.x, (int)position.y, (int)position.z - 1))
            quads.Add(new Quad(TypeUtility.BlockSide.BACK, position, bType));

        if(quads.Count == 0) return;

        int counter = 0;
        Mesh[] sideMeshes = new Mesh[quads.Count];
        foreach (Quad q in quads)
        {
            sideMeshes[counter] = q.mesh;
            counter++;
        }

        mesh = MeshUtility.MergeMeshes(sideMeshes);
        mesh.name = "Cube_0_0_0";
    }

    public bool hasSolidNeighbor(int x, int y, int z)
    {
        if (x < 0 || x >= parentChunk.width ||
            y < 0 || y >= parentChunk.height ||
            z < 0 || z >= parentChunk.depth)
        {
            return false;
        }
       
        if (parentChunk.chunkData[x + parentChunk.width * (y + parentChunk.depth * z)] == TypeUtility.BlockType.AIR ||
            parentChunk.chunkData[x + parentChunk.width * (y + parentChunk.depth * z)] == TypeUtility.BlockType.WATER) // Glass also
        {
            return false;
        }

        return true;
    }
}

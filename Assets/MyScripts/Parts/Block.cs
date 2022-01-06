using System.Collections.Generic;
using UnityEngine;

public class Block
{
    public Mesh mesh;
    public Chunk parentChunk;

    public Block(Vector3 position, TypeUtility.BlockType bType, Chunk chunk)
    {
        parentChunk = chunk;
        Vector3 blockLocalPos = position - chunk.location;
        
        if(bType == TypeUtility.BlockType.AIR) return;

        List<Quad> quads = new List<Quad>();

        // Grass have different top and bottom
        if (!hasSolidNeighbor((int) blockLocalPos.x, (int) blockLocalPos.y - 1, (int) blockLocalPos.z))
        {
            if (bType == TypeUtility.BlockType.GRASSSIDE)
            {
                quads.Add(new Quad(TypeUtility.BlockSide.TOP, position, TypeUtility.BlockType.DIRT));
            }
            else
            {
                quads.Add(new Quad(TypeUtility.BlockSide.BOTTOM, position, bType));
            }
        }

        if (!hasSolidNeighbor((int) blockLocalPos.x, (int) blockLocalPos.y + 1, (int) blockLocalPos.z))
        {
            if (bType == TypeUtility.BlockType.GRASSSIDE) 
            {
                quads.Add(new Quad(TypeUtility.BlockSide.TOP, position, TypeUtility.BlockType.GRASSTOP));
            }
            else
            {
                quads.Add(new Quad(TypeUtility.BlockSide.TOP, position, bType));
            }
        }
           
        
        if (!hasSolidNeighbor((int)blockLocalPos.x - 1, (int)blockLocalPos.y, (int)blockLocalPos.z))
            quads.Add(new Quad(TypeUtility.BlockSide.LEFT, position, bType));
        
        if (!hasSolidNeighbor((int)blockLocalPos.x + 1, (int)blockLocalPos.y, (int)blockLocalPos.z))
            quads.Add(new Quad(TypeUtility.BlockSide.RIGHT, position, bType));

        if (!hasSolidNeighbor((int)blockLocalPos.x, (int)blockLocalPos.y, (int)blockLocalPos.z + 1))
            quads.Add(new Quad(TypeUtility.BlockSide.FRONT, position, bType));

        if (!hasSolidNeighbor((int)blockLocalPos.x, (int)blockLocalPos.y, (int)blockLocalPos.z - 1))
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
        mesh.name = "Cube_" + position.x + "_" + position.y + "_" + position.z;
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

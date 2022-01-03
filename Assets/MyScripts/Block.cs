using UnityEngine;

public class Block
{
    public Mesh mesh;
    public  Block(Vector3 position, TypeUtility.BlockType bType)
    {
        Quad[] quads = new Quad[6]
        {
            new Quad(TypeUtility.BlockSide.TOP, position, bType),
            new Quad(TypeUtility.BlockSide.BOTTOM, position, bType),
            new Quad(TypeUtility.BlockSide.LEFT, position, bType),
            new Quad(TypeUtility.BlockSide.RIGHT, position, bType),
            new Quad(TypeUtility.BlockSide.FRONT, position, bType),
            new Quad(TypeUtility.BlockSide.BACK, position, bType)
        };

        Mesh[] sidMeshes = new Mesh[6]
            {quads[0].mesh, quads[1].mesh, quads[2].mesh, quads[3].mesh, quads[4].mesh, quads[5].mesh};

        mesh = MeshUtility.MergeMeshes(sidMeshes);
        mesh.name = "Cube_0_0_0";
    }
}

using UnityEngine;

public static class TypeUtility
{
    private const float spriteUnit = 0.0625f;
    public enum BlockSide { BOTTOM, TOP, LEFT, RIGHT, FRONT, BACK }
    public enum BlockType
    {
        GRASSTOP, GRASSSIDE, DIRT, WATER, STONE, SAND, AIR
    } //AIR must be the last one

    // (0,0)->left down, (0,1), (1,0), (1,1)->up right
    public static Vector2[,] blockUVs =
    {
        /*GRASSTOP*/ 
        {
            new Vector2(0.125f, 0.375f), new Vector2(0.1875f, 0.375f), 
            new Vector2(0.125f, 0.4375f), new Vector2(0.1875f, 0.4375f)
        }, 
        /*GRASSSIDE*/
        {
            new Vector2(0.1875f, 0.9375f), new Vector2(0.25f, 0.9375f),
            new Vector2(0.1875f, 1.0f), new Vector2(0.25f, 1.0f)
        },
        /*DIRT*/
        {
            new Vector2(0.125f, 0.9375f), new Vector2(0.1875f, 0.9375f),
            new Vector2(0.125f, 1.0f), new Vector2(0.1875f, 1.0f),
        },
        /*WATER*/
        {
            new Vector2(0.875f, 0.125f), new Vector2(0.9375f, 0.125f),
            new Vector2(0.875f, 0.1875f), new Vector2(0.9375f, 0.1875f)
        },
        /*STONE*/
        {
            new Vector2(0, 0.875f), new Vector2(0.0625f, 0.875f),
            new Vector2(0, 0.9375f), new Vector2(0.0625f, 0.9375f)
        },
        /*SAND*/
        {
            new Vector2(0.125f, 0.875f), new Vector2(0.1875f, 0.875f),
            new Vector2(0.125f, 0.9375f), new Vector2(0.1875f, 0.9375f)
        }
        
    };
}

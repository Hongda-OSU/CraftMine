using UnityEngine;

public static class TypeUtility
{
    private const float spriteUnit = 0.0625f;
    public enum BlockSide { BOTTOM, TOP, LEFT, RIGHT, FRONT, BACK }
    public enum BlockType
    {
        GRASSTOP, GRASSSIDE, DIRT, WATER, LAVA, STONE, SAND, GOLD, GOLDBLOCK, BEDROCK, REDSTONE, DIAMOND, DIAMONDBLOCK, NOCRACK,
        CRACK1, CRACK2, CRACK3, CRACK4, AIR
    } //AIR must be the last one

    public static int[] blockTypeHealth =
    {
        2, 2, 1, 1, 1, 3, 1, 3, 4, -1, 3, 3, 4, -1, -1, -1, -1, -1, -1
    }; // -1 -> not breakable

    // (0,0)->left down, (0,1)->right down, (1,0)->up left, (1,1)->up right
    public static Vector2[,] blockUVs =
    {
        /*GRASSTOP*/ 
        {
            new Vector2(2, 6) * spriteUnit, new Vector2(3, 6) * spriteUnit, 
            new Vector2(2, 7) * spriteUnit, new Vector2(3, 7) * spriteUnit
        }, 
        /*GRASSSIDE*/
        {
            new Vector2(3, 15) * spriteUnit, new Vector2(4, 15) * spriteUnit,
            new Vector2(3, 16) * spriteUnit, new Vector2(4, 16) * spriteUnit
        },
        /*DIRT*/
        {
            new Vector2(2, 15) * spriteUnit, new Vector2(3, 15) * spriteUnit,
            new Vector2(2, 16) * spriteUnit, new Vector2(3, 16) * spriteUnit,
        },
        /*WATER*/
        {
            new Vector2(14, 2) * spriteUnit, new Vector2(15, 2) * spriteUnit,
            new Vector2(14, 3) * spriteUnit, new Vector2(15, 3) * spriteUnit
        },
        /*LAVA*/
        {
            new Vector2(13, 1) * spriteUnit, new Vector2(14, 1) * spriteUnit,
            new Vector2(13, 2) * spriteUnit, new Vector2(14, 2) * spriteUnit
        },
        /*STONE*/
        {
            new Vector2(0, 14) * spriteUnit, new Vector2(1, 14) * spriteUnit,
            new Vector2(0, 15) * spriteUnit, new Vector2(1, 15) * spriteUnit
        },
        /*SAND*/
        {
            new Vector2(2, 14) * spriteUnit, new Vector2(3, 14) * spriteUnit,
            new Vector2(2, 15) * spriteUnit, new Vector2(3, 15) * spriteUnit
        },
        /*GOLD*/		
        { 
            new Vector2(0, 13) * spriteUnit,  new Vector2(1, 13) * spriteUnit,
            new Vector2(0, 14) * spriteUnit, new Vector2(1, 14) * spriteUnit
        },
        /*GOLDBLOCK*/		
        {
            new Vector2(7, 14) * spriteUnit,  new Vector2(8, 14) * spriteUnit,
            new Vector2(7, 15) * spriteUnit, new Vector2(8, 15) * spriteUnit
        },
        /*BEDROCK*/		
        {
            new Vector2(1, 14) * spriteUnit, new Vector2(2, 14) * spriteUnit,
            new Vector2(1, 15) * spriteUnit, new Vector2(2, 15) * spriteUnit
        },
        /*REDSTONE*/	
        {
            new Vector2(3, 12) * spriteUnit, new Vector2(4, 12) * spriteUnit,
            new Vector2(3, 13) * spriteUnit, new Vector2(4, 13) * spriteUnit
        },
        /*DIAMOND*/		
        {
            new Vector2(2, 12) * spriteUnit, new Vector2(3, 12) * spriteUnit,
            new Vector2(2, 13) * spriteUnit, new Vector2(3, 13) * spriteUnit
        },
        /*DIAMONDBLOCK*/		
        {
            new Vector2(8, 14) * spriteUnit, new Vector2(9, 14) * spriteUnit,
            new Vector2(8, 15) * spriteUnit, new Vector2(9, 15) * spriteUnit
        },
        /*NOCRACK*/		
        {
            new Vector2(11, 0) * spriteUnit, new Vector2(12, 0) * spriteUnit,
            new Vector2(11, 1 ) * spriteUnit,new Vector2(12, 1) * spriteUnit
        },
        /*CRACK1*/		
        { 
            new Vector2(0, 0) * spriteUnit,  new Vector2(1, 0) * spriteUnit,
            new Vector2(0, 1) * spriteUnit, new Vector2(1, 1) * spriteUnit
        },
        /*CRACK2*/		
        { 
            new Vector2(1, 0) * spriteUnit,  new Vector2(2, 0) * spriteUnit,
            new Vector2(1, 1) * spriteUnit, new Vector2(2, 1) * spriteUnit
        },
        /*CRACK3*/		
        {
            new Vector2(2, 0) * spriteUnit,  new Vector2(3, 0) * spriteUnit,
            new Vector2(2, 1) * spriteUnit, new Vector2(3, 1) * spriteUnit
        },
        /*CRACK4*/		
        { 
            new Vector2(3, 0) * spriteUnit,  new Vector2(4, 0) * spriteUnit,
            new Vector2(3, 1) * spriteUnit, new Vector2(4, 1) * spriteUnit
        }

    };
}

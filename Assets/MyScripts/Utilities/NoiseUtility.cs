using UnityEngine;

public class NoiseUtility
{
    // Fractal Brownian Motion
    // octaves: the amount of perline noise add together
    public static float FBM(float x, float z, int octaves, float scale, float heightScale, float heightOffset) 
    {
        float total = 0;
        float frequency = 1;
        for (int i = 0; i < octaves; i++)
        {
            total += Mathf.PerlinNoise(x * scale * frequency, z * scale * frequency) * heightScale;
            frequency *= 2;
        }
        return total + heightOffset;
    }
}

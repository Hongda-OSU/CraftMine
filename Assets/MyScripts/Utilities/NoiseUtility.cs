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

    public static float FBM(float x, float y, float z, int octaves, float scale, float heightScale, float heightOffset) // 3D version
    {
        float XY = FBM(x, y, octaves, scale, heightScale, heightOffset);
        float XZ = FBM(x, z, octaves, scale, heightScale, heightOffset);

        float YX = FBM(y, x, octaves, scale, heightScale, heightOffset);
        float YZ = FBM(y, z, octaves, scale, heightScale, heightOffset);
        
        float ZX = FBM(z, x, octaves, scale, heightScale, heightOffset);
        float ZY = FBM(z, y, octaves, scale, heightScale, heightOffset);

        return (XY + XZ + YX + YZ + ZX + ZY) / 6.0f;
    }
}

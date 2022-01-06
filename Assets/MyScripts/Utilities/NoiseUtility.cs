using UnityEngine;

public class NoiseUtility
{
    public struct PerlinSetting
    {
        public float heightScale;
        public float scale;
        public int octaves;
        public float heightOffset;
        public float probability; // also DrawCutOff

        public PerlinSetting(float hs, float s, int o, float ho, float p)
        {
            heightScale = hs;
            scale = s;
            octaves = o;
            heightOffset = ho;
            probability = p;
        }
    }

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

    // 3D version overwrite
    public static float FBM(float x, float y, float z, int octaves, float scale, float heightScale, float heightOffset) 
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

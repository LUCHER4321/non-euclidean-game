using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEditor;

public class Bake3DNoise : MonoBehaviour
{
    [Header("Noise Settings")]
    public Vector3 period = new Vector3(10, 10, 10);
    public Vector3 axis = new Vector3(0, 1, 0);
    [Range(0.1f, 360f)]
    public float rotationAngle = 90f;
    public float noiseScale = 10f;
    [Header("Result")]
    public float epsilon = 1f;
    public Texture3D noiseTex;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void BakeTexture()
    {
        int3 resolution = new int3(
            Mathf.Max(1, Mathf.CeilToInt(period.x * noiseScale)),
            Mathf.Max(1, Mathf.CeilToInt(period.y * noiseScale)),
            Mathf.Max(1, Mathf.CeilToInt(period.z * noiseScale))
        );
        noiseTex = new Texture3D(resolution.x, resolution.y, resolution.z, TextureFormat.RGBA32, false);
        Color[] colors = new Color[resolution.x * resolution.y * resolution.z];
        float3 rep = period;
        int idx = 0;
        for (int z = 0; z < resolution.z; z++)
        {
            for (int y = 0; y < resolution.y; y++)
            {
                for (int x = 0; x < resolution.x; x++)
                {
                    float3 pos = new float3(
                        (float)x / resolution.x * period.x,
                        (float)y / resolution.y * period.y,
                        (float)z / resolution.z * period.z
                    );
                    float3 n = new float3(0, 0, 0);
                    for(float f = 0f; f < 360f; f+= rotationAngle)
                    {
                        Unity.Mathematics.quaternion rotation = Unity.Mathematics.quaternion.AxisAngle(normalize(axis), radians(f));
                        pos = mul(rotation, pos);
                        float noiseVal = pnoise(pos, rep);
                        float noiseValX = pnoise(pos + new float3(epsilon, 0, 0), rep);
                        float noiseValY = pnoise(pos + new float3(0, epsilon, 0), rep);
                        float dx = noiseValX - noiseVal;
                        float dy = noiseValY - noiseVal;
                        float3 normal = CalculateGraphNormal(dx, dy);
                        n += normal;
                    }
                    n = normalize(n);
                    float3 mappedNormal = (n * 0.5f) + new float3(0.5f, 0.5f, 0.5f);
                    colors[idx++] = new Color(mappedNormal.x, mappedNormal.y, mappedNormal.z, 1f);
                }
            }
        }
        noiseTex.SetPixels(colors);
        noiseTex.Apply();
        string path = "Assets/Textures/NoiseTex3D.asset";
        AssetDatabase.CreateAsset(noiseTex, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"3D Texture File successfully saved in: {path}");
        Debug.Log($"3D Noise texture generated with resolution: {resolution.x}x{resolution.y}x{resolution.z}");
    }

    float3 mod289(float3 x) { return x - floor(x * (1.0f / 289.0f)) * 289.0f; }
    float4 mod289(float4 x) { return x - floor(x * (1.0f / 289.0f)) * 289.0f; }
    float4 permute(float4 x) { return mod289(((x * 34.0f) + 1.0f) * x); }
    float4 taylorInvSqrt(float4 r) { return 1.79284291400159f - 0.85373472095314f * r; }
    float3 fade(float3 t) { return t * t * t * (t * (t * 6.0f - 15.0f) + 10.0f); }
    float3 customMod(float3 x, float3 y) { return x - y * floor(x / y); }

    float pnoise(float3 P, float3 rep)
    {
        float3 Pi0 = customMod(floor(P), rep);
        float3 Pi1 = customMod(Pi0 + 1.0f, rep);
        Pi0 = mod289(Pi0);
        Pi1 = mod289(Pi1);
        float3 Pf0 = frac(P);
        float3 Pf1 = Pf0 - 1.0f;
        float4 ix = new float4(Pi0.x, Pi1.x, Pi0.x, Pi1.x);
        float4 iy = new float4(Pi0.y, Pi0.y, Pi1.y, Pi1.y);
        float4 iz0 = new float4(Pi0.z);
        float4 iz1 = new float4(Pi1.z);
        float4 ixy = permute(permute(ix) + iy);
        float4 ixy0 = permute(ixy + iz0);
        float4 ixy1 = permute(ixy + iz1);
        float4 gx0 = ixy0 / 7.0f;
        float4 gy0 = frac(floor(gx0) / 7.0f) - 0.5f;
        gx0 = frac(gx0);
        float4 gz0 = 0.5f - abs(gx0) - abs(gy0);
        float4 sz0 = step(gz0, 0.0f);
        gx0 -= sz0 * (step(0.0f, gx0) - 0.5f);
        gy0 -= sz0 * (step(0.0f, gy0) - 0.5f);
        float4 gx1 = ixy1 / 7.0f;
        float4 gy1 = frac(floor(gx1) / 7.0f) - 0.5f;
        gx1 = frac(gx1);
        float4 gz1 = 0.5f - abs(gx1) - abs(gy1);
        float4 sz1 = step(gz1, 0.0f);
        gx1 -= sz1 * (step(0.0f, gx1) - 0.5f);
        gy1 -= sz1 * (step(0.0f, gy1) - 0.5f);
        float3 g000 = new float3(gx0.x, gy0.x, gz0.x);
        float3 g100 = new float3(gx0.y, gy0.y, gz0.y);
        float3 g010 = new float3(gx0.z, gy0.z, gz0.z);
        float3 g110 = new float3(gx0.w, gy0.w, gz0.w);
        float3 g001 = new float3(gx1.x, gy1.x, gz1.x);
        float3 g101 = new float3(gx1.y, gy1.y, gz1.y);
        float3 g011 = new float3(gx1.z, gy1.z, gz1.z);
        float3 g111 = new float3(gx1.w, gy1.w, gz1.w);
        float4 norm0 = taylorInvSqrt(new float4(dot(g000, g000), dot(g010, g010), dot(g100, g100), dot(g110, g110)));
        g000 *= norm0.x;
        g010 *= norm0.y;
        g100 *= norm0.z;
        g110 *= norm0.w;
        float4 norm1 = taylorInvSqrt(new float4(dot(g001, g001), dot(g011, g011), dot(g101, g101), dot(g111, g111)));
        g001 *= norm1.x;
        g011 *= norm1.y;
        g101 *= norm1.z;
        g111 *= norm1.w;
        float n000 = dot(g000, Pf0);
        float n100 = dot(g100, new float3(Pf1.x, Pf0.y, Pf0.z));
        float n010 = dot(g010, new float3(Pf0.x, Pf1.y, Pf0.z));
        float n110 = dot(g110, new float3(Pf1.x, Pf1.y, Pf0.z));
        float n001 = dot(g001, new float3(Pf0.x, Pf0.y, Pf1.z));
        float n101 = dot(g101, new float3(Pf1.x, Pf0.y, Pf1.z));
        float n011 = dot(g011, new float3(Pf0.x, Pf1.y, Pf1.z));
        float n111 = dot(g111, Pf1);
        float3 fade_xyz = fade(Pf0);
        float4 n_z = lerp(new float4(n000, n100, n010, n110), new float4(n001, n101, n011, n111), fade_xyz.z);
        float2 n_yz = lerp(n_z.xy, n_z.zw, fade_xyz.y);
        float n_xyz = lerp(n_yz.x, n_yz.y, fade_xyz.x);
        return 2.2f * n_xyz;
    }

    float3 CalculateGraphNormal(float dx, float dy)
    {
        float dx2 = math.pow(dx, 2.0f);
        float dy2 = math.pow(dy, 2.0f);
        float length = math.sqrt(dx2 + dy2);
        float3 vec3 = new float3(dy, dx, 0.0f);
        float3 dividedVec = length > 0.00001f ? vec3 / length : new float3(0, 0, 0);
        float3 addVec = dividedVec + new float3(0.0f, 0.0f, 1.0f);
        float3 finalNormal = addVec / math.sqrt(2.0f);
        return finalNormal;
    }
}

using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

struct ParticleData{
    public Vector3 position;
    public Vector3 velocity;
    public Vector4 color;
    public float age;
}

struct VertexData
{
    public Vector4 position;
    public Vector4 color;
};
public class KernelExample: MonoBehaviour
{
    const int _numParticleThreads = 160;
    const int _numPatricleGroups  = 1600;
    const int _numParticles =_numParticleThreads * _numPatricleGroups;
    public ComputeShader particleShader;
    public ComputeShader vecFieldShader;
    public Texture colorTexture;

    Material _material;

    int _initKernel;
    int _initTextureKernel;
    int _updateKernel;
    int _geomKernel;

    ComputeBuffer _particleBuffer;
    ComputeBuffer _verticesBuffer;
    ComputeBuffer _velTexBuffer;
    Texture2D _noiseTex;

    void Allocate(){
        _particleBuffer = new ComputeBuffer(_numParticles, Marshal.SizeOf(typeof(ParticleData)));
        _verticesBuffer = new ComputeBuffer(_numParticles*4, Marshal.SizeOf(typeof(VertexData)));
        _velTexBuffer   = new ComputeBuffer(colorTexture.width * colorTexture.height, Marshal.SizeOf(typeof(Vector4)));

        _initKernel   = particleShader.FindKernel("Init");
        _updateKernel = particleShader.FindKernel("Update");
        _geomKernel = particleShader.FindKernel("Geom");
        // _initTextureKernel= particleShader.FindKernel("InitTexture");

        _noiseTex = new Texture2D(colorTexture.width, colorTexture.height);
        GenerateNoiseTex();
    }

    void GenerateNoiseTex(){
        Color[] pix = new Color[_noiseTex.width * _noiseTex.height];
        float y = 0.0F;
        while (y < _noiseTex.height) {
            float x = 0.0F;
            while (x < _noiseTex.width) {
                float xCoord = x / _noiseTex.width;
                float yCoord = y / _noiseTex.height;
                float scale = 4f;
                float sampleR= Mathf.PerlinNoise(scale*(xCoord+0.1f), scale*(yCoord));
                float sampleG= Mathf.PerlinNoise(scale*(xCoord), scale*(yCoord+0.5f));
                pix[(int)(y * _noiseTex.width + x)] = new Color(sampleR, sampleG, 0.0f);
                x++;
            }
            y++;
        }
        _noiseTex.SetPixels(pix);
        _noiseTex.Apply();
    }

    void Start(){
        _material = GetComponent<Renderer>().material;
        Allocate();
        particleShader.SetFloat("_time", Time.time);

        particleShader.SetVector("_colorTextureSize", new Vector2(colorTexture.width, colorTexture.height));
        particleShader.SetTexture(_initKernel, "_colorTexture", colorTexture);

        particleShader.SetBuffer(_initKernel, "_particles", _particleBuffer);
        particleShader.SetTexture(_initKernel, "_velocityTexture", _noiseTex);
        particleShader.Dispatch(_initKernel, _numPatricleGroups, 1, 1);

        // particleShader.SetBuffer(_initTextureKernel, "_velTexBuffer", _velTexBuffer);
        // particleShader.Dispatch(_initTextureKernel, 1, 1, 1);
    }

    void Update(){
        UpdateParticle();
    }

    void UpdateParticle(){
        particleShader.SetFloat("_time", Time.time);
        particleShader.SetFloat("_dt", Time.deltaTime);

        particleShader.SetVector("_colorTextureSize", new Vector2(colorTexture.width, colorTexture.height));
        particleShader.SetTexture(_updateKernel, "_colorTexture", colorTexture);
        particleShader.SetTexture(_updateKernel, "_velocityTexture", _noiseTex);
        particleShader.SetBuffer(_updateKernel, "_particles", _particleBuffer);
        particleShader.Dispatch(_updateKernel, _numParticles /_numParticleThreads, 1, 1);
    }

    void OnRenderObject(){
        particleShader.SetBuffer(_geomKernel, "_particles", _particleBuffer);
        particleShader.SetBuffer(_geomKernel, "_vertices", _verticesBuffer);
        particleShader.Dispatch(_geomKernel, _numParticles /_numParticleThreads, 1, 1);

        _material.SetBuffer("_vertices", _verticesBuffer);
        _material.SetPass(0);
        Graphics.DrawProcedural(MeshTopology.Quads, _numParticles);
    }

    void OnDestroy(){
        _particleBuffer.Release();
        _verticesBuffer.Release();
    }
}

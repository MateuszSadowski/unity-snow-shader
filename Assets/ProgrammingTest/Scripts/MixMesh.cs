using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MixMesh : MonoBehaviour
{
    Mesh oMesh;
    Mesh cMesh;
    MeshFilter oMeshFilter;
    int[] triangles;
    bool animateCloth = false;
    bool animateNoise = false;
    bool animateWaves = false;
    public Vector3[] vertices;
    public int width = 256;
    public int height = 256;
    public float scale = 4.56f;
    public float waveSpeed = 1f;
    public float waveHeight = 10f;

    void Start()
    {
        InitMesh();
    }

    void Update()
    {
        if(animateCloth)
        {
            AnimateCloth();
        }
        if(animateNoise)
        {
            AnimateNoise();
        }
        if(animateWaves)
        {
            AnimateWaves();
        }
    }

    void InitMesh()
    {
        oMeshFilter = GetComponent<MeshFilter>();
        oMesh = oMeshFilter.mesh;

        cMesh = new Mesh();
        cMesh.name = "clone";
        cMesh.vertices = oMesh.vertices;
        cMesh.triangles = oMesh.triangles;
        cMesh.normals = oMesh.normals;
        cMesh.uv = oMesh.uv;
        cMesh.RecalculateNormals();
        oMeshFilter.mesh = cMesh;

        vertices = cMesh.vertices;
        triangles = cMesh.triangles;
    }

    void Reset()
    {
        if (cMesh != null && oMesh != null)
        {
            cMesh.vertices = oMesh.vertices;
            cMesh.triangles = oMesh.triangles;
            cMesh.normals = oMesh.normals;
            cMesh.uv = oMesh.uv;
            oMeshFilter.mesh = cMesh;
            vertices = cMesh.vertices;
            triangles = cMesh.triangles;
        }
    }

    void AnimateCloth()
    {
        Reset();

        for (int i = 0; i < vertices.Length; i++)
        {
            float pX = ( vertices[i].x / width * scale ) + Time.timeSinceLevelLoad;
            float pZ = ( vertices[i].z / height * scale ) + Time.timeSinceLevelLoad;
 
            vertices[i].y += Mathf.PerlinNoise( pX, pZ ) - 0.5f;
        }

        UpdateMesh(vertices);
    }

    void AnimateNoise()
    {
        Reset();

        for (int i = 0; i < vertices.Length; i++)
        {
            float pX = ( vertices[i].x * scale ) + (Time.timeSinceLevelLoad * waveSpeed);
            float pZ = ( vertices[i].z * scale ) + (Time.timeSinceLevelLoad * waveSpeed);
 
            vertices[i].y += Mathf.PerlinNoise( pX, pZ ) - 0.5f;
        }

        UpdateMesh(vertices);
    }

    
    void AnimateWaves()
    {
        Reset();

        for (int i = 0; i < vertices.Length; i++)
        {
            float pX = ( vertices[i].x / width * scale ) + (Time.timeSinceLevelLoad * waveSpeed);
            float pZ = ( vertices[i].z / height * scale ) + (Time.timeSinceLevelLoad * waveSpeed);
 
            vertices[i].y += CalculateWave((int)pX, (int)pZ);
        }

        UpdateMesh(vertices);
    }

    void UpdateMesh(Vector3[] vertices) {
        cMesh.vertices = vertices;
        cMesh.RecalculateBounds();
        cMesh.RecalculateNormals();
    }

    float CalculateWave(int x, int y)
    {
        float xCoord = (float)x / width * scale;
        float yCoord = (float)y / height * scale;

        return (Mathf.PerlinNoise(xCoord, yCoord) - 0.5f) * waveHeight;
    }

    public void toggleReset()
    {
        animateCloth = animateNoise = animateWaves = false;
        Reset();
    }

    public void toggleAnimateCloth()
    {
        animateCloth = !animateCloth;
        animateNoise = animateWaves = false;
    }

    public void toggleAnimateNoise()
    {
        animateNoise = !animateNoise;
        animateCloth = animateWaves = false;

    }
    
    public void toggleAnimateWaves()
    {
        animateWaves = !animateWaves;
        animateNoise = animateCloth = false;

    }

}
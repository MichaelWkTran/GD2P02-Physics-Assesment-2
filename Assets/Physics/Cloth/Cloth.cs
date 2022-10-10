using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.ParticleSystem;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class Cloth : MonoBehaviour
{
    [SerializeField] uint width;
    [SerializeField] uint height;
    [SerializeField] Vector2 cellSize;

    [SerializeField] ClothParticle[] particles;
    [HideInInspector] public Mesh mesh;

    public int GetVertexIndex(int _cellX, int _cellY)
    {
        return (_cellY * ((int)width + 1)) + _cellX;
    }

    public void GenerateGrid()
    {
        Vector3[] verticies = new Vector3[(width + 1) * (height + 1)];
        int[] indices = new int[width * height * 6];
        
        //Destroy Particles
        foreach (ClothParticle particle in particles)
            Destroy(particle.gameObject);
        
        //Reset particles array size
        particles = new ClothParticle[verticies.Length];

        //Set the vertices of the mesh
        for (int cellY = 0; cellY < height + 1; cellY++)
            for (int cellX = 0; cellX < width + 1; cellX++)
            {
                //Set vertex position
                verticies[GetVertexIndex(cellX, cellY)] = new Vector3(cellX * cellSize.x, cellY * cellSize.y);

                //Create a game object to control the position of the vertex
                GameObject vertexGameObject = new GameObject("ClothParticle" + cellX + "-" + cellY);
                vertexGameObject.transform.parent = transform;
                vertexGameObject.transform.localPosition = verticies[GetVertexIndex(cellX, cellY)];

                //Give the created gameObject the ClothParticle script and set its properties
                ClothParticle vertexParticle = vertexGameObject.AddComponent<ClothParticle>();
                particles[GetVertexIndex(cellX, cellY)] = vertexParticle;
                vertexParticle.cloth = this;
                vertexParticle.cellX = (uint)cellX;
                vertexParticle.cellY = (uint)cellY;
            }

        //Set the indices of the mesh
        uint index = 0;
        for (int cellY = 0; cellY < height; cellY++)
            for (int cellX = 0; cellX < width; cellX++)
            {
                indices[index] =     GetVertexIndex(cellX,   cellY);
                indices[index + 1] = GetVertexIndex(cellX,   cellY+1);
                indices[index + 2] = GetVertexIndex(cellX+1, cellY);

                indices[index + 3] = GetVertexIndex(cellX,   cellY+1);
                indices[index + 4] = GetVertexIndex(cellX+1, cellY+1);
                indices[index + 5] = GetVertexIndex(cellX+1, cellY);

                index += 6U;
            }

        //Set to mesh
        mesh.vertices = verticies;
        mesh.triangles = indices;
        mesh.RecalculateNormals();
    }

    void Start()
    {
        mesh = GetComponent<MeshFilter>().sharedMesh = new Mesh();
        mesh.MarkDynamic();

        GenerateGrid();
    }

    void Update()
    {
        Vector3[] vertices = mesh.vertices;

        foreach (ClothParticle particle in particles)
        {
            if (particle == null) return;
            vertices[particle.vertexIndex] = particle.transform.localPosition;
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }
}

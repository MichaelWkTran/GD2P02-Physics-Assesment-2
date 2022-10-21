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

    public void GetCellPos(int _vertexIndex, out int _cellX, out int _cellY)
    {
        _cellY = (int)(_vertexIndex/(width + 1.0f));
        _cellX = _vertexIndex - _cellY;
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
                Vector3 vertexPosition = new Vector3(cellX*cellSize.x, -cellY*cellSize.y);
                vertexPosition += new Vector3(-width*cellSize.x, height*cellSize.y) / 2.0f;
                verticies[GetVertexIndex(cellX, cellY)] = vertexPosition;

                //Create a game object to control the position of the vertex
                GameObject vertexGameObject = new GameObject("ClothParticle" + cellX + "-" + cellY);
                vertexGameObject.transform.parent = transform;
                vertexGameObject.transform.localPosition = vertexPosition;

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
                indices[index + 1] = GetVertexIndex(cellX + 1, cellY);
                indices[index + 2] = GetVertexIndex(cellX,   cellY+1);

                indices[index + 3] = GetVertexIndex(cellX,   cellY+1);
                indices[index + 4] = GetVertexIndex(cellX + 1, cellY);
                indices[index + 5] = GetVertexIndex(cellX+1, cellY+1);

                index += 6U;
            }

        //Set the joints of the particles
        foreach (ClothParticle particle in particles)
        {
            ClothParticle GetSurroundingParticle(int _cellOffsetX, int _cellOffsetY)
            {
                if (particle.cellX + _cellOffsetX < 0) return null;
                if (particle.cellX + _cellOffsetX > width) return null;
                if (particle.cellY + _cellOffsetY < 0) return null;
                if (particle.cellY + _cellOffsetY > height) return null;

                return particles[GetVertexIndex((int)particle.cellX + _cellOffsetX, (int)particle.cellY + _cellOffsetY)];
            }

            particle.top         = GetSurroundingParticle( 0,-1);
            particle.topRight    = GetSurroundingParticle( 1,-1);
            particle.right       = GetSurroundingParticle( 1, 0);
            particle.bottomRight = GetSurroundingParticle( 1, 1);
            particle.bottom      = GetSurroundingParticle( 0, 1);
            particle.bottomLeft  = GetSurroundingParticle(-1, 1);
            particle.left        = GetSurroundingParticle(-1, 0);
            particle.leftTop     = GetSurroundingParticle(-1, 1);
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

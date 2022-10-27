using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class Cloth : MonoBehaviour
{
    [SerializeField] uint width;
    [SerializeField] uint height;
    [SerializeField] Vector2 cellSize;

    [Header("Physics")]
    public float mass = 1.0f;
    [Range(0.0f, 1.0f)] public float damping = 0.1f;
    public float gravityScale = 1.0f;
    public float spring = 1.0f;

    public ClothParticle[] particles;
    [HideInInspector] public Mesh mesh;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().sharedMesh = new Mesh();
        mesh.MarkDynamic();
        GenerateMesh();
    }

    void Update()
    {
        Vector3[] vertices = mesh.vertices;

        foreach (ClothParticle particle in particles)
        {
            if (particle == null) continue;
            vertices[particle.vertexIndex] = particle.transform.localPosition;
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    public int GetVertexIndex(int _cellX, int _cellY)
    {
        return (_cellY * ((int)width + 1)) + _cellX;
    }

    public void GetCellPos(int _vertexIndex, out int _cellX, out int _cellY)
    {
        _cellY = (int)(_vertexIndex / (width + 1.0f));
        _cellX = _vertexIndex - _cellY;
    }

    public Vector2 GetCellSize()
    {
        return cellSize;
    }

    public void GenerateMesh()
    {
        Vector3[] verticies = new Vector3[(width + 1) * (height + 1)];
        Vector2[] uvs = new Vector2[(width + 1) * (height + 1)];
        int[] indices = new int[width * height * 6];

        //Destroy Particles
        //{
        //    List<Transform> children = new List<Transform>();
        //    foreach(Transform child in transform) children.Add(child);
        //
        //    foreach (Transform child in children)
        //        Destroy(child.gameObject);
        //}

        foreach (ClothParticle particle in particles)
            Destroy(particle.gameObject);

        //Reset particles array size
        particles = new ClothParticle[verticies.Length];

        //Set the vertices of the mesh
        for (int cellY = 0; cellY < height + 1; cellY++)
            for (int cellX = 0; cellX < width + 1; cellX++)
            {
                int vertexIndex = GetVertexIndex(cellX, cellY);

                //Set vertex position
                Vector3 vertexPosition = new Vector3(cellX * cellSize.x, -cellY * cellSize.y);
                vertexPosition += new Vector3(-width * cellSize.x, height * cellSize.y) / 2.0f;
                verticies[vertexIndex] = vertexPosition;
                uvs[vertexIndex] = new Vector2(cellX / (float)width, 1.0f - (cellY / (float)height));

                //Create a game object to control the position of the vertex
                GameObject vertexGameObject = new GameObject("ClothParticle" + cellX + "-" + cellY);
                vertexGameObject.transform.parent = transform;
                vertexGameObject.transform.localPosition = vertexPosition;

                //Give the created gameObject the ClothParticle script and set its properties
                ClothParticle vertexParticle = vertexGameObject.AddComponent<ClothParticle>();
                particles[vertexIndex] = vertexParticle;

                vertexParticle.cloth = this;
                vertexParticle.cellX = (uint)cellX;
                vertexParticle.cellY = (uint)cellY;
                vertexParticle.mass = mass;
                vertexParticle.damping = damping;
                vertexParticle.gravityScale = gravityScale;
                vertexParticle.spring = spring;
            }

        //Set the indices of the mesh
        uint index = 0;
        for (int cellY = 0; cellY < height; cellY++)
            for (int cellX = 0; cellX < width; cellX++)
            {
                indices[index] = GetVertexIndex(cellX, cellY);
                indices[index + 1] = GetVertexIndex(cellX + 1, cellY);
                indices[index + 2] = GetVertexIndex(cellX, cellY + 1);

                indices[index + 3] = GetVertexIndex(cellX, cellY + 1);
                indices[index + 4] = GetVertexIndex(cellX + 1, cellY);
                indices[index + 5] = GetVertexIndex(cellX + 1, cellY + 1);

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

            particle.top = GetSurroundingParticle(0, -1);
            particle.topRight = GetSurroundingParticle(1, -1);
            particle.right = GetSurroundingParticle(1, 0);
            particle.bottomRight = GetSurroundingParticle(1, 1);
            particle.bottom = GetSurroundingParticle(0, 1);
            particle.bottomLeft = GetSurroundingParticle(-1, 1);
            particle.left = GetSurroundingParticle(-1, 0);
            particle.leftTop = GetSurroundingParticle(-1, -1);
        }

        //Set the pinned Joints
        for (int i = 0; i < width + 1; i++)
        {
            particles[i].pinned = true;
        }

        //Set to mesh
        mesh.vertices = verticies;
        mesh.uv = uvs;
        mesh.triangles = indices;
    }
}

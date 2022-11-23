// Bachelor of Software Engineering
// Media Design School
// Auckland
// New Zealand
// (c) 2022 Media Design School
//
// File Name: Cloth.cs
// Description: Cloth implementation file
// Authors: Michael Wai Kit Tran

using UnityEngine;
using static UnityEngine.ParticleSystem;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
public class Cloth : MonoBehaviour
{
    public uint m_width;
    public uint m_height;
    public Vector2 m_cellSize;
    public float m_tearForce;

    [Header("Physics")]
    public uint m_timeStep = 1;
    public float m_mass = 1.0f;
    [Range(0.0f, 1.0f)] public float m_damping = 0.1f;
    public float m_gravityScale = 1.0f;
    public float m_spring = 1.0f;
    public float m_collisionDistance = 0.1f;

    [Header("Miscellaneous")]
    public ParticleSystem m_fireParticleSystem;
    public float m_fireGrowthRate;

    public ClothParticle[] m_particles;
    [HideInInspector] public Mesh m_mesh { get; private set; }

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: Start()
    //	 Purpose: Initalise cloth mesh
    void Start()
    {
        m_mesh = GetComponent<MeshFilter>().sharedMesh = new Mesh();
        m_mesh.MarkDynamic();
        GenerateMesh();
    }

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: FixedUpdate()
    //	 Purpose: Update the cloth and its particles every time step
    void FixedUpdate()
    {
        //Update cloth particle physics
        for (uint i = 0; i < m_timeStep; i++)
            foreach (ClothParticle particle in m_particles)
            {
                if (particle == null) continue;
                particle.ParticleUpdate();
            }        

        //Update cloth mesh
        Vector3[] vertices = m_mesh.vertices;
        foreach (ClothParticle particle in m_particles)
        {
            if (particle == null) continue;
            vertices[particle.vertexIndex] = particle.transform.localPosition;
        }

        m_mesh.vertices = vertices;
        m_mesh.RecalculateNormals();
        m_mesh.RecalculateBounds();
        GetComponent<MeshCollider>().sharedMesh = m_mesh;
    }

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: GenerateMesh()
    //	 Purpose: Generates the mesh for the cloth
    public void GenerateMesh()
    {
        //Destroy Particles
        foreach (Transform child in transform) Destroy(child.gameObject);

        Vector3[] verticies = new Vector3[(m_width + 1) * (m_height + 1)];
        Vector2[] uvs = new Vector2[(m_width + 1) * (m_height + 1)];
        int[] indices = new int[m_width * m_height * 6];
        
        //Reset particles array size
        m_particles = new ClothParticle[verticies.Length];

        //Set the vertices of the mesh
        for (int cellY = 0; cellY < m_height+1; cellY++)
            for (int cellX = 0; cellX < m_width+1; cellX++)
            {
                int vertexIndex = GetVertexIndex(cellX, cellY);

                //Set vertex position
                Vector3 vertexPosition = new Vector3(cellX * m_cellSize.x, -cellY * m_cellSize.y);
                vertexPosition += new Vector3(-m_width * m_cellSize.x, m_height * m_cellSize.y) / 2.0f;
                verticies[vertexIndex] = vertexPosition;
                uvs[vertexIndex] = new Vector2(cellX / (float)m_width, 1.0f - (cellY / (float)m_height));

                //Create a game object to control the position of the vertex
                GameObject vertexGameObject = new GameObject("ClothParticle" + cellX + "-" + cellY);
                vertexGameObject.transform.parent = transform;
                vertexGameObject.transform.localPosition = vertexPosition;

                //Give the created gameObject the ClothParticle script and set its properties
                ClothParticle vertexParticle = vertexGameObject.AddComponent<ClothParticle>();
                m_particles[vertexIndex] = vertexParticle;

                vertexParticle.cloth = this;
                vertexParticle.cellX = (uint)cellX;
                vertexParticle.cellY = (uint)cellY;
            }

        //Set the indices of the mesh
        uint index = 0;
        for (int cellY = 0; cellY < m_height; cellY++)
            for (int cellX = 0; cellX < m_width; cellX++)
            {
                indices[index]   = GetVertexIndex(cellX,   cellY);
                indices[index+1] = GetVertexIndex(cellX+1, cellY);
                indices[index+2] = GetVertexIndex(cellX,   cellY+1);

                indices[index+3] = GetVertexIndex(cellX,   cellY+1);
                indices[index+4] = GetVertexIndex(cellX+1, cellY);
                indices[index+5] = GetVertexIndex(cellX+1, cellY+1);

                index += 6U;
            }

        //Set the joints of the particles
        foreach (ClothParticle particle in m_particles)
        {
            ClothParticle GetSurroundingParticle(int _cellOffsetX, int _cellOffsetY)
            {
                if (particle.cellX + _cellOffsetX < 0)      return null;
                if (particle.cellX + _cellOffsetX > m_width)  return null;
                if (particle.cellY + _cellOffsetY < 0)      return null;
                if (particle.cellY + _cellOffsetY > m_height) return null;

                return m_particles[GetVertexIndex((int)particle.cellX + _cellOffsetX, (int)particle.cellY + _cellOffsetY)];
            }

            particle.top =         GetSurroundingParticle(0, -1);
            particle.topRight =    GetSurroundingParticle(1, -1);
            particle.right =       GetSurroundingParticle(1, 0);
            particle.bottomRight = GetSurroundingParticle(1, 1);
            particle.bottom =      GetSurroundingParticle(0, 1);
            particle.bottomLeft =  GetSurroundingParticle(-1, 1);
            particle.left =        GetSurroundingParticle(-1, 0);
            particle.leftTop =     GetSurroundingParticle(-1, -1);
        }

        //Set the pinned Joints
        for (int i = 0; i < m_width + 1; i++)
        {
            m_particles[i].pinned = true;
        }

        //Set to mesh
        m_mesh.vertices = verticies;
        m_mesh.uv = uvs;
        m_mesh.triangles = indices;
    }

    //Get Set methods
    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: GetVertexIndex()
    //	 Purpose: Get the index of a particle to be used by the variable, m_particles. _cellX and _cellY are the positions of the vertex in the cloth
    //	 Returns: Get the index of a particle to be used by the variable, m_particles. 
    public int GetVertexIndex(int _cellX, int _cellY)
    {
        return (_cellY * ((int)m_width + 1)) + _cellX;
    }

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: GetVertexIndex()
    //	 Purpose: Get the positions of the vertex in the cloth, _cellX and _cellY, by using its particle index
    public void GetCellPos(int _vertexIndex, out int _cellX, out int _cellY)
    {
        _cellY = (int)(_vertexIndex / (m_width + 1.0f));
        _cellX = _vertexIndex - _cellY;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClothParticle : MonoBehaviour
{
    public Cloth cloth;
    public uint cellX;
    public uint cellY;
    public int vertexIndex { get; private set; }

    public ClothParticle top;
    public ClothParticle topRight;
    public ClothParticle right;
    public ClothParticle bottomRight;
    public ClothParticle bottom;
    public ClothParticle bottomLeft;
    public ClothParticle left;
    public ClothParticle leftTop;

    void OnDestroy()
    {
        ////If there are particles this particle is joined to. Then ignore checking the mesh triangles
        //if (!top && !topRight && !right && !bottomRight && !bottom && !bottomLeft && !left && !leftTop)
        //    return;

        //Find the indicies to delete
        List<int> oldIndices = cloth.mesh.triangles.ToList();
        for (int index = 0; index < oldIndices.Count; index += 3)
        {
            //Check whether the triangle contains this vertex
            bool destroyedIndexFound = false;
            for (int triangleIndex = 0; triangleIndex < 3; triangleIndex++)
            {
                if (vertexIndex != oldIndices[index + triangleIndex])
                    continue;
            
                destroyedIndexFound = true;
                break;
            }
            
            //If the triangle does not contain the vertex of this cloth particle,
            //then skip this triangle
            if (!destroyedIndexFound) continue;

            //Set the index to -1 to indicate that it is to be destroyed
            for (int triangleIndex = 0; triangleIndex < 3; triangleIndex++)
                oldIndices[index + triangleIndex] = -1;
        }

        //Create a new list in indices that include all indices from the previous list
        //excluding the ones marked for deletion.
        List<int> indices = new List<int>();
        for (int index = 0; index < oldIndices.Count; index++)
        {
            if (oldIndices[index] == -1) continue;
            indices.Add(oldIndices[index]);
        }
        
        //Set the new indices to the mesh
        cloth.mesh.triangles = indices.ToArray();
    }

    void Start()
    {
        vertexIndex = cloth.GetVertexIndex((int)cellX, (int)cellY);
    }

    void Update()
    {
        //Check whether there are no particles this particle is joined to. If so delete it.
        //if (!top && !topRight && !right && !bottomRight && !bottom && !bottomLeft && !left && !leftTop)
        //{
        //    Destroy(gameObject);
        //    return;
        //}
    }
}

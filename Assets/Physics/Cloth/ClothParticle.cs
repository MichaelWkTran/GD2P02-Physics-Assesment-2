using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting;

public class ClothParticle : MonoBehaviour
{
    public Cloth cloth;
    public uint cellX;
    public uint cellY;
    public int vertexIndex { get; private set; }

    [Header("Physics")]
    Vector3 prevPosition;
    Vector3 acceleration;
    Vector3 impulse;
    public float mass = 1.0f;
    [Range(0.0f, 1.0f)] public float damping = 0.1f;
    public float gravityScale = 1.0f;
    public float spring = 0.6f;
    public float restDistance = 0.3f;
    public float maxDistance = 0.3f;

    [Header("Constraints")]
    public bool pinned = false;
    public ClothParticle top;
    public ClothParticle topRight;
    public ClothParticle right;
    public ClothParticle bottomRight;
    public ClothParticle bottom;
    public ClothParticle bottomLeft;
    public ClothParticle left;
    public ClothParticle leftTop;

    void Start()
    {
        prevPosition = transform.position;
        vertexIndex = cloth.GetVertexIndex((int)cellX, (int)cellY);
    }

    void FixedUpdate()
    {
        //Check whether there are no particles this particle is joined to. If so delete it.
        if (!top && !topRight && !right && !bottomRight && !bottom && !bottomLeft && !left && !leftTop)
        {
            Destroy(gameObject);
            return;
        }

        //Apply constraints
        void asdsd(ClothParticle _otherParticle, bool sss = false)
        {
            if (_otherParticle == null) return;

            //Get the vector from this particle to the other particle
            Vector3 delta = transform.position - _otherParticle.transform.position;
            
            //Force the particle to be a fixed max distance from each other
            if (delta.magnitude > maxDistance && !pinned && sss)
            {
                transform.position = new Vector3(transform.position.x, (_otherParticle.transform.position + (delta.normalized * maxDistance)).y, transform.position.z);
            }


            //Get how far this particle is from rest position
            float difference = restDistance - delta.magnitude;

            float inverseMass = 1.0f / mass;
            float inverseOtherMass = 1.0f / _otherParticle.mass;

            ApplyForce(delta.normalized * spring * (restDistance - delta.magnitude));
            
            
            //_otherParticle.ApplyForce(-delta.normalized * spring * (restDistance - delta.magnitude));
            //ApplyForce(delta.normalized * (1/(inverseMass + inverseOtherMass)) * spring * difference);
            //_otherParticle.ApplyForce(-delta.normalized * (1/(inverseMass + inverseOtherMass)) * spring * difference);
        }

        asdsd(top, true);
        asdsd(right);
        asdsd(bottom);
        asdsd(left);

        //Ignore physics if pinned
        if (pinned) return;

        //Apply self collision
        foreach(ClothParticle particle in cloth.particles)
        {
            if (particle == this) continue;
            if (particle == null) continue;
        
            Vector3 particleDifference = particle.transform.position - transform.position;
            if (particleDifference.magnitude > 0.1f) continue;
            
            ApplyImpulse(particleDifference/2.0f);
            particle.ApplyImpulse(-particleDifference/2.0f);
        }

        //Collision With ground
        if (transform.position.y < 0) transform.position = new Vector3(transform.position.x, 0.0f, transform.position.z);

        //Apply gravity
        ApplyForce(Physics.gravity * gravityScale * mass);
        //ApplyForce(Vector3.forward * 2.0f);

        //Apply verlet integration
        Vector3 velocity = transform.position - prevPosition;
        prevPosition = transform.position;
        transform.position += ((1.0f - damping) * velocity) + impulse + (acceleration * Time.fixedDeltaTime);
        acceleration = Vector3.zero;
        impulse = Vector3.zero;
    }

    void OnDestroy()
    {
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

    public void ApplyForce(Vector3 _force)
    {
        if (pinned) return;
        acceleration += _force / mass;
    }

    public void ApplyImpulse(Vector3 _impulse)
    {
        if (pinned) return;
        impulse += _impulse / mass;
    }
}

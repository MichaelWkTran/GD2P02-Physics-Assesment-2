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

    [Header("Physics")]
    Vector3 prevPosition;
    Vector3 acceleration;
    public float mass = 1.0f;
    [Range(0.0f, 1.0f)] public float damping = 0.1f;
    public float gravityScale = 1.0f;
    public float spring = 1.0f;
    float shearRestDistance;

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
        shearRestDistance = Mathf.Sqrt(Mathf.Pow(cloth.GetCellSize().x, 2.0f) + Mathf.Pow(cloth.GetCellSize().y, 2.0f));
    }

    void FixedUpdate()
    {
        //Check whether there are no particles this particle is joined to. If so delete it.
        {
            int constraintCount = 0;
            constraintCount += top ? 1 : 0;
            constraintCount += topRight ? 1 : 0;
            constraintCount += right ? 1 : 0;
            constraintCount += bottomRight ? 1 : 0;
            constraintCount += bottom ? 1 : 0;
            constraintCount += bottomLeft ? 1 : 0;
            constraintCount += left ? 1 : 0;
            constraintCount += leftTop ? 1 : 0;

            if (constraintCount < 2)
            {
                Destroy(gameObject);
                return;
            }
        }

        //Apply constraints
        void ApplyParticleConstraint(ClothParticle _a, ClothParticle _b, float _restDistance)
        {
            if (_b == null || _a == null || _a == _b) return;

            Vector3 delta = _b.transform.position - _a.transform.position;
            Vector3 springForce = delta * spring * (1.0f - (_restDistance / delta.magnitude));
            
            _a.ApplyForce( (springForce/2.0f) * (_b.pinned ? 2.0f : 1.0f) * mass);
            _b.ApplyForce(-(springForce/2.0f) * (_a.pinned ? 2.0f : 1.0f) * mass);
        }

        //Apply Stretch Constraint
        ApplyParticleConstraint(this, right,  cloth.GetCellSize().x);
        ApplyParticleConstraint(this, bottom, cloth.GetCellSize().y);
        
        //Apply Shear Constraint
        ApplyParticleConstraint(this, bottomRight, shearRestDistance);
        ApplyParticleConstraint(this, bottomLeft,  shearRestDistance);

        //Apply Bend Constraint
        ApplyParticleConstraint(left, right,  cloth.GetCellSize().x * 2.0f);
        ApplyParticleConstraint(top,  bottom, cloth.GetCellSize().y * 2.0f);
        ApplyParticleConstraint(leftTop,  bottomRight, shearRestDistance * 2.0f);
        ApplyParticleConstraint(topRight, bottomLeft,  shearRestDistance * 2.0f);

        //Ignore physics if pinned
        if (pinned) return;

        //Apply self collision
        //foreach(ClothParticle particle in cloth.particles)
        //{
        //    if (particle == this) continue;
        //    if (particle == null) continue;
        //
        //    Vector3 particleDifference = particle.transform.position - transform.position;
        //    if (particleDifference.magnitude > 0.1f) continue;
        //
        //    transform.position += particleDifference/2.0f;
        //    particle.transform.position += -particleDifference/2.0f;
        //}

        //Collision with ground
        float distanceToGround = 0.01f;
        if (transform.position.y < distanceToGround) transform.position = new Vector3(transform.position.x, distanceToGround, transform.position.z);

        //Collision with sphere
        SphereCollision[] spheres = FindObjectsOfType<SphereCollision>();
        foreach (SphereCollision sphere in spheres)
        {
            float distanceToSphere = 0.1f;

            //Get the vector from the sphere to the particle
            Vector3 delta = transform.position - sphere.transform.position;
            
            //If the particle is outside of the sphere, move to the next partcile
            if (delta.magnitude > sphere.radius+distanceToSphere) continue;
            transform.position += delta.normalized * (sphere.radius + distanceToSphere - delta.magnitude);
        }

        //Apply gravity
        ApplyForce(Physics.gravity * gravityScale * mass);

        //Tear cloth if too much force is applied to it
        if (acceleration.magnitude > 100.0f)
        {
            Destroy(gameObject);
            return;
        }

        //Apply verlet integration
        transform.position += ((1.0f - damping) * (transform.position - prevPosition)) + (acceleration * Time.fixedDeltaTime);
        prevPosition = transform.position;
        acceleration = Vector3.zero;
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
}

// Bachelor of Software Engineering
// Media Design School
// Auckland
// New Zealand
// (c) 2022 Media Design School
//
// File Name: ClothParticle.cs
// Description: ClothParticle implementation file
// Authors: Michael Wai Kit Tran

using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class ClothParticle : MonoBehaviour
{
    public Cloth cloth;
    public uint cellX;
    public uint cellY;
    public int vertexIndex { get; private set; }

    [Header("Physics")]
    Vector3 prevPosition;
    Vector3 acceleration;
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

    [Header("Miscellaneous")]
    public float fireFactor;

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: Start()
    //	 Purpose: Sets up velvet intergration, get particle index, and sets shear rest distance for physics constraints
    void Start()
    {
        prevPosition = transform.position;
        vertexIndex = cloth.GetVertexIndex((int)cellX, (int)cellY);
        shearRestDistance = Mathf.Sqrt(Mathf.Pow(cloth.m_cellSize.x, 2.0f) + Mathf.Pow(cloth.m_cellSize.y, 2.0f));
    }

    void Update()
    {
        if (fireFactor > 1) { Destroy(); return; }

        float GetFireGrowth(ClothParticle _particle)
        {
            if (_particle == null) return 0;
            return cloth.m_fireGrowthRate * (_particle.fireFactor / 1.0f);
        }

        fireFactor += GetFireGrowth(this) * Time.deltaTime;
        fireFactor += GetFireGrowth(top) * Time.deltaTime;
        fireFactor += GetFireGrowth(topRight) * Time.deltaTime;
        fireFactor += GetFireGrowth(right) * Time.deltaTime;
        fireFactor += GetFireGrowth(bottomRight) * Time.deltaTime;
        fireFactor += GetFireGrowth(bottom) * Time.deltaTime;
        fireFactor += GetFireGrowth(bottomLeft) * Time.deltaTime;
        fireFactor += GetFireGrowth(left) * Time.deltaTime;
        fireFactor += GetFireGrowth(leftTop) * Time.deltaTime;

        if (fireFactor <= 0.5f) return;
        EmitParams emitParams = new EmitParams()
        {
            position = transform.position
        };


        cloth.m_fireParticleSystem.Emit(emitParams, (int)(cloth.m_fireParticleSystem.emission.rateOverTime.Evaluate(0) * Time.deltaTime));
    }

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: Destroy()
    //	 Purpose: Destroys the particle, the triangle it is apart of, and disconnects constraints
    public void Destroy()
    {
        List<int> verticiesToCheck = new List<int>();

        //Find the indicies to delete
        List<int> indices = cloth.m_mesh.triangles.ToList();
        for (int index = 0; index < indices.Count; index += 3)
        {
            //Check whether the triangle contains this vertex
            bool destroyedIndexFound = false;
            for (int triangleIndex = 0; triangleIndex < 3; triangleIndex++)
            {
                //Skip checking triangle if it does not contain this vertex
                if (vertexIndex != indices[index + triangleIndex]) continue;

                //Mark the indices of the triangle to be deleted if it contains this vertex
                //and skip checking the triangle
                destroyedIndexFound = true;
                break;
            }

            //If the triangle does not contain the vertex of this cloth particle,
            //then skip this triangle
            if (!destroyedIndexFound) continue;

            //Remove triangle
            verticiesToCheck.Add(indices[index]);
            verticiesToCheck.Add(indices[index + 1]);
            verticiesToCheck.Add(indices[index + 2]);
            indices.RemoveAt(index);
            indices.RemoveAt(index);
            indices.RemoveAt(index);
            index -= 3;
        }

        //Set the final traingles of the mesh
        cloth.m_mesh.triangles = indices.ToArray();

        //Loop through all the verticies of deleted triangles
        //and check whether they should be deleted or not
        foreach (int vertex in verticiesToCheck)
        {
            ClothParticle particle = cloth.m_particles[vertex];
            if (particle == this || particle == null) continue;
            particle.CheckToBeDeleted();
        }

        //Destroy this particles
        Destroy(gameObject);
    }

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: ParticleUpdate()
    //	 Purpose: Update the physics of the particles
    public void ParticleUpdate()
    {
        //Apply constraints
        void ApplyParticleConstraint(ClothParticle _a, ClothParticle _b, float _restDistance)
        {
            if (_b == null || _a == null || _a == _b) return;

            Vector3 delta = _b.transform.position - _a.transform.position;
            Vector3 springForce = delta * cloth.m_spring * (1.0f - (_restDistance / delta.magnitude));

            _a.ApplyForce((springForce/2.0f) * (_b.pinned ? 2.0f : 1.0f) * cloth.m_mass);
            _b.ApplyForce(-(springForce/2.0f) * (_a.pinned ? 2.0f : 1.0f) * cloth.m_mass);
        }

        //Apply Stretch Constraint
        ApplyParticleConstraint(this, right, cloth.m_cellSize.x);
        ApplyParticleConstraint(this, bottom, cloth.m_cellSize.y);

        //Apply Shear Constraint
        ApplyParticleConstraint(this, bottomRight, shearRestDistance);
        ApplyParticleConstraint(this, bottomLeft, shearRestDistance);

        //Apply Bend Constraint
        ApplyParticleConstraint(left, right, cloth.m_cellSize.x * 2.0f);
        ApplyParticleConstraint(top, bottom, cloth.m_cellSize.y * 2.0f);
        ApplyParticleConstraint(leftTop, bottomRight, shearRestDistance * 2.0f);
        ApplyParticleConstraint(topRight, bottomLeft, shearRestDistance * 2.0f);

        //Ignore physics if pinned
        if (pinned) return;

        ////Apply self collision
        //foreach(ClothParticle particle in cloth.particles)
        //{
        //    if (particle == this || particle == null) continue;
        
        //    Vector3 particleDifference = particle.transform.position - transform.position;
        //    Debug.Log(particleDifference);
        
        //    if (particleDifference.magnitude >= 0.01f) continue;
            
        //    //transform.position += particleDifference/2.0f;
        //    //particle.transform.position -= particleDifference/2.0f;
        //}

        //Collision with ground
        float distanceToGround = 0.01f;
        if (transform.position.y < distanceToGround) transform.position = new Vector3(transform.position.x, distanceToGround, transform.position.z);

        //Collision with sphere
        SphereCollision[] spheres = FindObjectsOfType<SphereCollision>();
        foreach (SphereCollision sphere in spheres)
        {
            //Get the vector from the sphere to the particle
            Vector3 delta = transform.position - sphere.transform.position;

            //If the particle is outside of the sphere, move to the next partcile
            if (delta.magnitude >= sphere.radius + cloth.m_collisionDistance) continue;
            transform.position += delta.normalized * (sphere.radius + cloth.m_collisionDistance - delta.magnitude);
        }

        //Collision with capsule
        CapsuleCollision[] capsules = FindObjectsOfType<CapsuleCollision>();
        foreach (CapsuleCollision capsule in capsules)
        {
            //Get capsule start and end points
            Vector3 capsuleStart = capsule.GetGlobalStart();
            Vector3 capsuleEnd = capsule.GetGlobalEnd();

            //Get vector from capsule start to end
            Vector3 capsuleVector = capsuleEnd - capsuleStart;

            //Project the particle onto vector and calculate
            float capsuleProjectionFactor = Vector3.Dot(transform.position - capsuleStart, capsuleVector);
            capsuleProjectionFactor /= capsuleVector.magnitude;
            capsuleProjectionFactor = Mathf.Clamp01(capsuleProjectionFactor);

            //Calculate the position of the sphere in which the particle would collide with
            Vector3 sphere = Vector3.Lerp(capsuleStart, capsuleEnd, capsuleProjectionFactor);
            
            //Get the vector from the sphere to the particle
            Vector3 delta = transform.position - sphere;

            //If the particle is outside of the sphere, move to the next partcile
            if (delta.magnitude >= capsule.GetGlobalRadius() + cloth.m_collisionDistance) continue;
            transform.position += delta.normalized * (capsule.GetGlobalRadius() + cloth.m_collisionDistance - delta.magnitude);
        }

        //Apply gravity
        ApplyForce(Physics.gravity * cloth.m_gravityScale * cloth.m_mass);

        //Tear cloth if too much force is applied to it
        if (acceleration.magnitude > cloth.m_tearForce) Destroy();
        
        //Apply verlet integration
        transform.position += (((1.0f - cloth.m_damping) * (transform.position - prevPosition)) + (acceleration * Time.fixedDeltaTime))/cloth.m_timeStep;
        prevPosition = transform.position;
        acceleration = Vector3.zero;
    }

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: CheckToBeDeleted()
    //	 Purpose: Checks whether constraints attached to particle still exists are destroys the particle if it has no constriants
    public void CheckToBeDeleted()
    {
        bool topFound = false;
        bool topRightFound = false;
        bool rightFound = false;
        bool bottomRightFound = false;
        bool bottomFound = false;
        bool bottomLeftFound = false;
        bool leftFound = false;
        bool leftTopFound = false;

        //Check whether a constraint still exists in the cloth mesh
        void CheckConstraintExists(ref ClothParticle _constraint, ref bool _constraintFound, int _triangleIndex)
        {
            if (_constraint == null || _constraintFound == true) return;
            
            bool thisParticleFound = false;
            bool otherParticleFound = false;

            //Loop through all verticies of the triangle
            for (int i = 0; i < 3; i++)
            {
                //If a vertex of a possible edge has not been found after checking the second triangle vertex,
                //no constraints are found
                if (i == 2 && !thisParticleFound && !otherParticleFound) return;
                
                //Check whether the checked vertex is this particle or the particle it is constrained to
                if (!thisParticleFound && cloth.m_mesh.triangles[_triangleIndex + i] == vertexIndex) thisParticleFound = true;
                else if (cloth.m_mesh.triangles[_triangleIndex + i] == _constraint.vertexIndex) otherParticleFound = true;

                //End function if the constaint exists
                if (thisParticleFound && otherParticleFound) { _constraintFound = true; return; }
            }
        }

        //Loop through mesh to find constraints in mesh
        for (int triangleIndex = 0; triangleIndex < cloth.m_mesh.triangles.Length; triangleIndex += 3)
        {
            CheckConstraintExists(ref top,         ref topFound,         triangleIndex);
            CheckConstraintExists(ref topRight,    ref topRightFound,    triangleIndex);
            CheckConstraintExists(ref right,       ref rightFound,       triangleIndex);
            CheckConstraintExists(ref bottomRight, ref bottomRightFound, triangleIndex);
            CheckConstraintExists(ref bottom,      ref bottomFound,      triangleIndex);
            CheckConstraintExists(ref bottomLeft,  ref bottomLeftFound,  triangleIndex);
            CheckConstraintExists(ref left,        ref leftFound,        triangleIndex);
            CheckConstraintExists(ref leftTop,     ref leftTopFound,     triangleIndex);
        }

        //Remove constraints that do not exist in the mesh
        if (!topFound) top = null;
        if (!topRightFound) topRight = null;
        if (!rightFound) right = null;
        if (!bottomRightFound) bottomRight = null;
        if (!bottomFound) bottom = null;
        if (!bottomLeftFound) bottomLeft = null;
        if (!leftFound) left = null;
        if (!leftTopFound) leftTop = null;

        //Check whether there are any triangles in the mesh that uses this vertex
        foreach (int index in cloth.m_mesh.triangles)
            if (index == vertexIndex) return;

        //If there are no triangles that uses this vertex, destroy it
        Destroy(gameObject);
    }

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: ApplyForce()
    //	 Purpose: Apply forces to the particles
    public void ApplyForce(Vector3 _force)
    {
        if (pinned) return;
        acceleration += _force / cloth.m_mass;
    }
}

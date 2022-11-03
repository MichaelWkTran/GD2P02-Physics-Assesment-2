using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Unity.VisualScripting.Dependencies.NCalc;
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

    void OnDestroy()
    {
        if (cloth.clearing) return;

        //Find the indicies to delete
        List<int> oldIndices = cloth.mesh.triangles.ToList();
        for (int index = 0; index < oldIndices.Count; index += 3)
        {
            //Check whether the triangle contains this vertex
            bool destroyedIndexFound = false;
            for (int triangleIndex = 0; triangleIndex < 3; triangleIndex++)
            {
                //Skip checking triangle if it does not contain this vertex
                if (vertexIndex != oldIndices[index + triangleIndex]) continue;

                //Disconnect all constraints in this triangle


                //Mark the indices of the triangle to be deleted if it contains this vertex
                //and skip checking the triangle
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

        foreach(ClothParticle particle in cloth.particles)
        {
            if (particle == this || particle == null) continue;
            particle.CheckToBeDeleted();
        }
    }

    public void ParticleUpdate()
    {
        //Apply constraints
        void ApplyParticleConstraint(ClothParticle _a, ClothParticle _b, float _restDistance)
        {
            if (_b == null || _a == null || _a == _b) return;

            Vector3 delta = _b.transform.position - _a.transform.position;
            Vector3 springForce = delta * cloth.spring * (1.0f - (_restDistance / delta.magnitude));

            _a.ApplyForce((springForce/2.0f) * (_b.pinned ? 2.0f : 1.0f) * cloth.mass);
            _b.ApplyForce(-(springForce/2.0f) * (_a.pinned ? 2.0f : 1.0f) * cloth.mass);
        }

        //Apply Stretch Constraint
        ApplyParticleConstraint(this, right, cloth.GetCellSize().x);
        ApplyParticleConstraint(this, bottom, cloth.GetCellSize().y);

        //Apply Shear Constraint
        ApplyParticleConstraint(this, bottomRight, shearRestDistance);
        ApplyParticleConstraint(this, bottomLeft, shearRestDistance);

        //Apply Bend Constraint
        ApplyParticleConstraint(left, right, cloth.GetCellSize().x * 2.0f);
        ApplyParticleConstraint(top, bottom, cloth.GetCellSize().y * 2.0f);
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
            if (delta.magnitude >= sphere.radius + cloth.collisionDistance) continue;
            transform.position += delta.normalized * (sphere.radius + cloth.collisionDistance - delta.magnitude);
        }

        //Apply gravity
        ApplyForce(Physics.gravity * cloth.gravityScale * cloth.mass);

        //Tear cloth if too much force is applied to it
        //if (acceleration.magnitude > cloth.tearForce) Destroy(gameObject);
        
        //Apply verlet integration
        transform.position += (((1.0f - cloth.damping) * (transform.position - prevPosition)) + (acceleration * Time.fixedDeltaTime))/cloth.timeStep;
        prevPosition = transform.position;
        acceleration = Vector3.zero;
    }

    public class Pair<T, U>
    {
        public Pair(T first, U second) { First = first; Second = second; }

        public T First { get; set; }
        public U Second { get; set; }
    };

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

        void sssss(ref ClothParticle _constraint, ref bool _constraintFound, int _triangleIndex)
        {
            if (_constraint == null || _constraintFound == true) return;

            bool a = false;
            bool s = false;

            for (int i = 0; i < 3; i++)
            {
                if (!a && cloth.mesh.triangles[_triangleIndex + i] == vertexIndex) a = true;
                else if (cloth.mesh.triangles[_triangleIndex + i] == _constraint.vertexIndex) s = true;

                if (a && s) { _constraintFound = true; return; }
            }
        }

        for (int triangleIndex = 0; triangleIndex < cloth.mesh.triangles.Length; triangleIndex += 3)
        {
            sssss(ref top,         ref topFound,         triangleIndex);
            sssss(ref topRight,    ref topRightFound,    triangleIndex);
            sssss(ref right,       ref rightFound,       triangleIndex);
            sssss(ref bottomRight, ref bottomRightFound, triangleIndex);
            sssss(ref bottom,      ref bottomFound,      triangleIndex);
            sssss(ref bottomLeft,  ref bottomLeftFound,  triangleIndex);
            sssss(ref left,        ref leftFound,        triangleIndex);
            sssss(ref leftTop,     ref leftTopFound,     triangleIndex);
        }

        if (!topFound) top = null;
        if (!topRightFound) topRight = null;
        if (!rightFound) right = null;
        if (!bottomRightFound) bottomRight = null;
        if (!bottomFound) bottom = null;
        if (!bottomLeftFound) bottomLeft = null;
        if (!leftFound) left = null;
        if (!leftTopFound) leftTop = null;

        //Check whether there are any triangles in the mesh that uses this vertex
        foreach (int index in cloth.mesh.triangles)
            if (index == vertexIndex) return;

        //If there are no triangles that uses this vertex, destroy it
        Destroy(gameObject);
    }

    public void ApplyForce(Vector3 _force)
    {
        if (pinned) return;
        acceleration += _force / cloth.mass;
    }
}

// Bachelor of Software Engineering
// Media Design School
// Auckland
// New Zealand
// (c) 2022 Media Design School
//
// File Name: SphereCollision.cs
// Description: Sphere collision implementation file
// Authors: Michael Wai Kit Tran

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SphereCollision : MonoBehaviour
{
    public float localRadius = 0.5f;
    public float radius { get; private set; }

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: Update()
    //	 Purpose: Set sphere radius
    void Update()
    {
        radius = localRadius * Mathf.Max(transform.lossyScale.x, Mathf.Max(transform.lossyScale.y, transform.lossyScale.z));
    }

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: OnDrawGizmosSelected()
    //	 Purpose: Draw sphere collider
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}

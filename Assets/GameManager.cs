using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] ClothParticle heldParticle = null;
    float particleZCoord = 0.0f;
    Vector3 offset = Vector3.zero;

    void Start()
    {
        
    }

    void Update()
    {
        MouseGrab();
    }

    void MouseGrab()
    {
        Vector3 GetMouseWorldPos()
        {
            Vector3 mousePoint = Input.mousePosition;
            mousePoint.z = particleZCoord;
            return Camera.main.ScreenToWorldPoint(mousePoint);
        }

        float ssss = 0.1f;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            Cloth cloth = FindObjectOfType<Cloth>();
            if (cloth == null) return;

            float closestParticleDistance = ssss;
            foreach (ClothParticle particle in cloth.m_particles)
            {
                if (particle == null) continue;
                float particleDistance = Vector3.Cross(ray.direction, particle.transform.position - ray.origin).magnitude;
                if (particleDistance < closestParticleDistance)
                {
                    closestParticleDistance = particleDistance;
                    heldParticle = particle;
                }
            }

            if (heldParticle)
            {
                particleZCoord = Camera.main.WorldToScreenPoint(heldParticle.transform.position).z;
                offset = heldParticle.transform.position - GetMouseWorldPos();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            heldParticle = null;
        }

        
        if (heldParticle != null)
        {
            heldParticle.transform.position = GetMouseWorldPos() + offset;
        }
    }

    void MouseClickTear()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        float ssss = 0.1f;

        if (Input.GetMouseButton(0))
        {
            Cloth cloth = FindObjectOfType<Cloth>();
            if (cloth == null) return;

            ClothParticle closestParticle = null;
            float closestParticleDistance = ssss;
            foreach (ClothParticle particle in cloth.m_particles)
            {
                if (particle == null) continue;
                float particleDistance = Vector3.Cross(ray.direction, particle.transform.position - ray.origin).magnitude;
                if (particleDistance < closestParticleDistance)
                {
                    closestParticleDistance = particleDistance;
                    closestParticle = particle;
                }
            }

            if (closestParticle != null) closestParticle.Destroy();
        }
    }
}

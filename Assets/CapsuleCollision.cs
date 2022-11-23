using UnityEngine;

public class CapsuleCollision : MonoBehaviour
{
    public float m_radius = 0.5f;
    public Vector3 m_start;
    public Vector3 m_end;

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: GetGlobalStart()
    //	 Purpose: Get the start point of the capsule in global position
    //	 Returns: The start point of the capsule in global position
    public Vector3 GetGlobalStart()
    {
        Vector3 globalStart = m_start;
        globalStart.Scale(transform.lossyScale);
        return (transform.rotation * globalStart) + transform.position;
    }

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: GetGlobalEnd()
    //	 Purpose: Get the end point of the capsule in global position
    //	 Returns: The end point of the capsule in global position
    public Vector3 GetGlobalEnd()
    {
        Vector3 globalEnd = m_end;
        globalEnd.Scale(transform.lossyScale);
        return (transform.rotation * globalEnd) + transform.position;
    }

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: GetGlobalRadius()
    //	 Purpose: Get the radius of the capsule scaled based on the global transform scale
    //	 Returns: The radius of the capsule scaled based on the global transform scale
    public float GetGlobalRadius()
    {
        return m_radius * Mathf.Max(transform.lossyScale.x, Mathf.Max(transform.lossyScale.y, transform.lossyScale.z));
    }

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: OnDrawGizmosSelected()
    //	 Purpose: Draw capsule collider
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        float globalRadius = GetGlobalRadius();
        Vector3 globalStart = GetGlobalStart();
        Vector3 globalEnd = GetGlobalEnd();

        Gizmos.DrawWireSphere(globalStart, globalRadius);
        Gizmos.DrawWireSphere(globalEnd, globalRadius);

        Gizmos.DrawLine(globalStart + (Vector3.up * m_radius), globalEnd + (Vector3.up * GetGlobalRadius()));
        Gizmos.DrawLine(globalStart + (Vector3.right * m_radius), globalEnd + (Vector3.right * GetGlobalRadius()));
        Gizmos.DrawLine(globalStart + (Vector3.down * m_radius), globalEnd + (Vector3.down * GetGlobalRadius()));
        Gizmos.DrawLine(globalStart - (Vector3.up * m_radius), globalEnd - (Vector3.up * GetGlobalRadius()));
        Gizmos.DrawLine(globalStart - (Vector3.right * m_radius), globalEnd - (Vector3.right * GetGlobalRadius()));
        Gizmos.DrawLine(globalStart - (Vector3.down * m_radius), globalEnd - (Vector3.down * GetGlobalRadius()));
    }
}

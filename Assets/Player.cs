using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] float m_moveSpeed;
    [SerializeField] Vector2 m_rotationSensitivity;
    [SerializeField] float m_minRotationY;
    [SerializeField] float m_maxRotationY; 
    Vector2 m_rotation;
    
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        //Show or hide cursor
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }

        //Disable movement if the cursor is not locked
        if (Cursor.lockState != CursorLockMode.Locked) return;

        //Move Camera
        transform.position += Input.GetAxisRaw("Horizontal") * m_moveSpeed * transform.right * Time.deltaTime;
        transform.position += Input.GetAxisRaw("Vertical") * m_moveSpeed * transform.forward * Time.deltaTime;
        transform.position += Input.GetAxisRaw("Depth") * m_moveSpeed * Vector3.up * Time.deltaTime;

        //Rotate Camera
        m_rotation.x += Input.GetAxisRaw("Mouse X") * m_rotationSensitivity.x;
        m_rotation.x = Mathf.Repeat(m_rotation.x, 360.0f);

        m_rotation.y += Input.GetAxisRaw("Mouse Y") * m_rotationSensitivity.y;
        Mathf.Clamp(m_rotation.y, m_minRotationY, m_maxRotationY);

        transform.rotation = Quaternion.Euler(m_rotation.y, m_rotation.x, 0.0f);
    }
}

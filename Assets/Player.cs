using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] Vector2 rotationSensitivity;
    [SerializeField] float minRotationY;
    [SerializeField] float maxRotationY; 
    Vector2 rotation;
    
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
        transform.position += Input.GetAxisRaw("Horizontal") * moveSpeed * transform.right * Time.deltaTime;
        transform.position += Input.GetAxisRaw("Vertical") * moveSpeed * transform.forward * Time.deltaTime;
        transform.position += Input.GetAxisRaw("Depth") * moveSpeed * Vector3.up * Time.deltaTime;

        //Rotate Camera
        rotation.x += Input.GetAxisRaw("Mouse X") * rotationSensitivity.x;
        rotation.x = Mathf.Repeat(rotation.x, 360.0f);

        rotation.y += Input.GetAxisRaw("Mouse Y") * rotationSensitivity.y;
        Mathf.Clamp(rotation.y, minRotationY, maxRotationY);

        transform.rotation = Quaternion.Euler(rotation.y, rotation.x, 0.0f);
    }
}

// Bachelor of Software Engineering
// Media Design School
// Auckland
// New Zealand
// (c) 2022 Media Design School
//
// File Name: GameManager.cs
// Description: Manages the how the user is able to interact with the program
// Authors: Michael Wai Kit Tran

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum MouseInteraction{ None, Grab, Tear, Pin, Unpin };
    [SerializeField] Cloth m_cloth;

    [Header("UI")]
    [SerializeField] TMP_InputField m_clothCellWidth;
    [SerializeField] TMP_InputField m_clothCellHeight;
    [SerializeField] TMP_InputField m_numberOfHorizontalCells;
    [SerializeField] TMP_InputField m_numberOfVerticalCells;

    [Header("Mouse interaction")]
    [SerializeField] MouseInteraction m_mouseInteraction;
    [SerializeField] ClothParticle m_heldParticle = null;
    float m_maxParticleDistance = 0.1f;
    float m_particleZCoord = 0.0f;
    Vector3 m_offset = Vector3.zero;

    [Header("Colldiers")]
    [SerializeField] SphereCollision m_sphereCollision;
    [SerializeField] CapsuleCollision m_capsuleCollision;

    [Header("Wind")]
    [SerializeField] float m_windSpeed = 0;
    [SerializeField] Vector3 m_windDirection;

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: Start()
    //	 Purpose: Set up UI
    void Start()
    {
        //Set the place holder text values in the input fields
        m_clothCellWidth.placeholder.GetComponent<TMP_Text>().text = m_cloth.m_cellSize.x.ToString();
        m_clothCellHeight.placeholder.GetComponent<TMP_Text>().text = m_cloth.m_cellSize.y.ToString();
        m_numberOfHorizontalCells.placeholder.GetComponent<TMP_Text>().text = m_cloth.m_width.ToString();
        m_numberOfVerticalCells.placeholder.GetComponent<TMP_Text>().text = m_cloth.m_height.ToString();
    }

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: Update()
    //	 Purpose: Update interactions with cloth
    void Update()
    {
        //Mouse Interactions
        if (Cursor.lockState != CursorLockMode.Locked)
            switch (m_mouseInteraction)
            {
                case MouseInteraction.Grab:
                    MouseGrab();
                    break;
                case MouseInteraction.Tear:
                    MouseTear();
                    break;
                case MouseInteraction.Pin:
                    MousePin();
                    break;
                case MouseInteraction.Unpin:
                    MouseUnpin();
                    break;
            }

        //Add Wind Force
        if (m_windSpeed > 0)
        {
            foreach (ClothParticle particle in FindObjectsOfType<ClothParticle>())
            {
                particle.ApplyForce
                (
                    (Quaternion.Euler(m_windDirection.x, m_windDirection.y, m_windDirection.z) * Vector3.forward) *
                    m_windSpeed
                );
            }
        }
    }

#region Mouse Interactions

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: GetClosestParticleToMouse()
    //	 Purpose: Gets the closest particle to the ray that shoots from the camera to the mouse
    //	 Returns: The closest particle to the ray that shoots from the camera to the mouse
    ClothParticle GetClosestParticleToMouse()
    {
        ClothParticle selectedParticle = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float closestParticleDistance = m_maxParticleDistance;

        //Loop through all particles in the cloth
        foreach (ClothParticle particle in m_cloth.m_particles)
        {
            //If the particles does not exist, ignore it
            if (particle == null) continue;

            //Select the particle if it is closer to the mouse ray than the previous selected particle
            float particleDistance = Vector3.Cross(ray.direction, particle.transform.position - ray.origin).magnitude;
            if (particleDistance < closestParticleDistance)
            {
                closestParticleDistance = particleDistance;
                selectedParticle = particle;
            }
        }

        return selectedParticle;
    }

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: MouseGrab()
    //	 Purpose: Grabs a cloth particle using the mouse
    void MouseGrab()
    {
        Vector3 GetMouseWorldPos()
        {
            Vector3 mousePoint = Input.mousePosition;
            mousePoint.z = m_particleZCoord;
            return Camera.main.ScreenToWorldPoint(mousePoint);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (m_heldParticle = GetClosestParticleToMouse())
            {
                m_particleZCoord = Camera.main.WorldToScreenPoint(m_heldParticle.transform.position).z;
                m_offset = m_heldParticle.transform.position - GetMouseWorldPos();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            m_heldParticle = null;
        }

        if (m_heldParticle != null)
        {
            m_heldParticle.transform.position = GetMouseWorldPos() + m_offset;
        }
    }

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: MouseTear()
    //	 Purpose: Destroys a cloth particle using the mouse
    void MouseTear()
    {
        if (Input.GetMouseButton(0))
        {
            ClothParticle selectedParticle = GetClosestParticleToMouse();
            if (selectedParticle) selectedParticle.Destroy();
        }
    }

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: MousePin()
    //	 Purpose: Pins a cloth particle using the mouse
    void MousePin()
    {
        if (Input.GetMouseButton(0))
        {
            ClothParticle selectedParticle = GetClosestParticleToMouse();
            if (selectedParticle) selectedParticle.pinned = true;
        }
    }

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: MousePin()
    //	 Purpose: Unpin a cloth particle using the mouse
    void MouseUnpin()
    {
        if (Input.GetMouseButton(0))
        {
            ClothParticle selectedParticle = GetClosestParticleToMouse();
            if (selectedParticle) selectedParticle.pinned = false;
        }
    }

#endregion

#region Set values from  UI

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: SetStringFromInputField()
    //	 Purpose: Get the text from the placeholder of the input field, _inputField, if the input field text is empty. If it is not empty then get it from the _inputField
    //	 Returns: Returns the text from the placeholder of the input field, _inputField, if the input field text is empty. If it is not empty then return it from the _inputField
    string SetStringFromInputField(TMP_InputField _inputField)
    {
        //Get the text from the placeholder of the input field if the input field text is empty
        return _inputField.text != "" ? _inputField.text : _inputField.placeholder.GetComponent<TMP_Text>().text;
    }

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: OnGenerateMeshPressed()
    //	 Purpose: Called when the Generate button is pressed 
    public void OnGenerateMeshPressed()
    {
        //Set the cell size variable to the m_cloth
        {
            Vector2 newCellSize = Vector2.zero;
            newCellSize.x = float.Parse(SetStringFromInputField(m_clothCellWidth));
            newCellSize.y = float.Parse(SetStringFromInputField(m_clothCellHeight));

            if (newCellSize.x < 0) newCellSize.x = 0;
            if (newCellSize.y < 0) newCellSize.y = 0;
            m_cloth.m_cellSize = newCellSize;
        }

        //Set the cell number to the m_cloth
        {
            int newWidth = int.Parse(SetStringFromInputField(m_numberOfHorizontalCells));
            int newHeight = int.Parse(SetStringFromInputField(m_numberOfVerticalCells));

            m_cloth.m_width = (uint)newWidth;
            m_cloth.m_height = (uint)newHeight;
        }

        //Generate mesh
        m_cloth.GenerateMesh();
    }

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: OnMouseClickDropdownChange()
    //	 Purpose: Changes m_mouseInteraction when the mouse interaction dropdown, _dropdown, is changed
    public void OnMouseClickDropdownChange(TMP_Dropdown _dropdown)
    {
        m_mouseInteraction = (MouseInteraction)_dropdown.value;
    }

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: OnColliderDropdownChange()
    //	 Purpose: Changes the active animated collider when its corresponding dropdown , _dropdown, is changed
    public void OnColliderDropdownChange(TMP_Dropdown _dropdown)
    {
        //Disable all animated colliders
        m_sphereCollision.gameObject.SetActive(false);
        m_capsuleCollision.gameObject.SetActive(false);

        //Activate the requested collider
        switch (_dropdown.value)
        {
            case 1:
                m_sphereCollision.gameObject.SetActive(true);
                break;
            case 2:
                m_capsuleCollision.gameObject.SetActive(true);
                break;
        }
    }

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: OnWindSpeedChange()
    //	 Purpose: Changes m_windSpeed when its coresponding input field, _inputField, is changed
    public void OnWindSpeedChange(TMP_InputField _inputField)
    {
        m_windSpeed = float.Parse(_inputField.text);
    }

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: OnWindDirectionChangeX()
    //	 Purpose: Changes m_windDirection.x when corresonding slider is changed
    public void OnWindDirectionChangeX(Slider _slider)
    {
        m_windDirection.x = _slider.value;
    }

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: OnWindDirectionChangeY()
    //	 Purpose: Changes m_windDirection.y when corresonding slider is changed
    public void OnWindDirectionChangeY(Slider _slider)
    {
        m_windDirection.y = _slider.value;
    }

    //------------------------------------------------------------------------------------------------------------------------
    // Procedure: OnWindDirectionChangeZ()
    //	 Purpose: Changes m_windDirection.z when its corresonding slider is changed
    public void OnWindDirectionChangeZ(Slider _slider)
    {
        m_windDirection.z = _slider.value;
    }

#endregion
}
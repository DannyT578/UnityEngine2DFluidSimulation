using UnityEngine;

public class SimulationControls : MonoBehaviour
{
    // The radius value that will change based on scroll wheel input
    public float radius = 1.0f;
    // The rate at which the radius increases or decreases when scrolling
    public float scrollSpeed = 0.1f;
    
    // Push and Pull actions to be implemented
    public void Push(Vector3 position)
    {
        // Implement push logic here, e.g., apply force or effect on particles at the position
        Debug.Log("Push at position: " + position);
    }

    public void Pull(Vector3 position)
    {
        // Implement pull logic here, e.g., attract particles to the position
        Debug.Log("Pull at position: " + position);
    }

    public void controls()
    {
        // Check for mouse input and trigger push or pull actions
        if (Input.GetMouseButton(0)) // Left-click to push
        {
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPosition.z = 0; // Ensure it's on the 2D plane
            Push(mouseWorldPosition);
        }
        else if (Input.GetMouseButton(1)) // Right-click to pull
        {
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPosition.z = 0; // Ensure it's on the 2D plane
            Pull(mouseWorldPosition);
        }

        // Scroll wheel to change the radius value
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            radius += scrollInput * scrollSpeed;
            radius = Mathf.Max(radius, 0.1f); // Prevent radius from going negative or too small
            Debug.Log("Radius: " + radius);
        }
    }
}

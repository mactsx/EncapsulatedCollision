using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float moveSpeed = 5f; // Movement speed, adjustable in the Inspector

    void Update()
    {
        // Get horizontal and vertical input from WASD or Arrow Keys
        float horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right Arrow
        float vertical = Input.GetAxis("Vertical"); // W/S or Up/Down Arrow

        // Calculate the movement vector
        Vector3 movement = new Vector3(horizontal, 0f, vertical) * moveSpeed * Time.deltaTime;

        // Apply the movement to the object's position
        transform.Translate(movement);
    }
}

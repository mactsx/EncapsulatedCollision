using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderToggle : MonoBehaviour
{
    private BoxCollider boxCollider;
    private MeshCollider meshCollider;
    public bool isInside = false;
    public float resetTime = 2f;

    void Start()
    {
        // Get references to the colliders on the object
        boxCollider = GetComponent<BoxCollider>();
        meshCollider = GetComponent<MeshCollider>();
        

        // Ensure both colliders are available
        if (boxCollider == null || meshCollider == null)
        {
            Debug.LogError("Both BoxCollider and MeshCollider are required on this object.");
            enabled = false; // Disable the script if colliders are missing
            return;
        }

        // Initially set the MeshCollider as inactive
        meshCollider.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Toggle Collider");
        
        if (!isInside)
        {
            boxCollider.enabled = false;
            meshCollider.enabled = true;
            isInside = true;

            // Start the reset coroutine
            StartCoroutine(ResetCollider());
        }
    }

    IEnumerator ResetCollider()
    {
        // Wait for the resetTime to pass
        yield return new WaitForSeconds(resetTime);

        boxCollider.enabled = true;
        meshCollider.enabled = false;
        isInside = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class DynamicCollider : MonoBehaviour
{
    public bool expandedView = false;

    public bool isDynamicEnabled;
    private BoxCollider boxCol;
    private MeshCollider meshCol;
    public bool isInside = false;
    public float resetTime = 2f;

    void Start()
    {
        // Check if dynamic collision is enabled
        if (isDynamicEnabled)
        {
            // Get references to the colliders on the object
            boxCol = GetComponent<BoxCollider>();
            meshCol = GetComponent<MeshCollider>();

            // Initially set the MeshCollider as inactive
            meshCol.enabled = false;

            // Ensure box collider is set to trigger
            boxCol.isTrigger = true;
        }
        else
        {
            Debug.Log("Dynamic Collision is not enabled");
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        

        if (isDynamicEnabled && !isInside)
        {
            Debug.Log("Hit Outside Collider!");
            //Debug.Log("Miss!");

            boxCol.enabled = false;
            meshCol.enabled = true;
            isInside = true;

            // Start the reset coroutine
            StartCoroutine(ResetCollider());
        }
        if (meshCol != null && meshCol.enabled && other is MeshCollider)
        {
            Debug.Log("Hit Mesh to Mesh!");
        }

    }


    IEnumerator ResetCollider()
    {
        // Wait for the resetTime to pass
        yield return new WaitForSeconds(resetTime);

        boxCol.enabled = true;
        meshCol.enabled = false;
        isInside = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        
    }

}

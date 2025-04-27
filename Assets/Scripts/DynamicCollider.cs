using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class DynamicCollider : MonoBehaviour
{
    public bool expandedView = false;

    public bool isDynamicEnabled;
    public Collider triggerCollider;
    private Collider otherCol;
    private MeshCollider meshCol;
    public bool isInside = false;
    public float resetTime = 1f;
    

    void Start()
    {
        // Check if dynamic collision is enabled
        if (isDynamicEnabled)
        {
            // Get references to the colliders on the object
            otherCol = triggerCollider;
            meshCol = GetComponent<MeshCollider>();

            if (meshCol == null) 
            {
                meshCol = gameObject.AddComponent<MeshCollider>();
            }
            // Initially set the MeshCollider as inactive
            meshCol.enabled = false;

            if (otherCol != null)
            {
                // Ensure other collider is set to trigger
                otherCol.isTrigger = true;
            }
        }
        else
        {
            Debug.Log("Dynamic Collision is not enabled");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDynamicEnabled && !isInside && otherCol != null)
        {
            Debug.Log("Hit Outside Collider!");
            //Debug.Log("Miss!");

            otherCol.enabled = false;
            meshCol.enabled = true;
            isInside = true;

            // Start the reset coroutine
            StartCoroutine(ResetCollider());

            if (meshCol != null && meshCol.enabled && other is MeshCollider)
            {
                Debug.Log("Hit Mesh to Mesh!");
            }
        }

    }

    public void SetTrigger(int i, Collider[] allCols)
    {
        if (allCols[i] is MeshCollider)
        { }
        else
        {
            allCols[i].isTrigger = true;
        }
    }

    public void UndoisTrigger(int i, Collider[] allCols)
    {
        if (allCols[i] is MeshCollider)
        { }
        else
        {
            if (allCols[i] == null)
            {
                return;
            }
            allCols[i].isTrigger = false;
        }
    }


    IEnumerator ResetCollider()
    {
        // Wait for the resetTime to pass
        yield return new WaitForSeconds(resetTime);

        otherCol.enabled = true;
        meshCol.enabled = false;
        isInside = false;
    }

}

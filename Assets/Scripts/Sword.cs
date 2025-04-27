using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    private Collider col;
    public float duration = 1.1f;

    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartSwing()
    {
        col.enabled = true;
        Invoke(nameof(EndSwing), duration); // Duration of the hit window
    }

    void EndSwing()
    {
        col.enabled = false;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (other.GetComponent<Damageable>() != null)
            {
                Damageable hit = other.GetComponent<Damageable>();
                Vector3 hitPoint = other.ClosestPoint(transform.position);
                hit.TakeDamage(8, hitPoint);
            }
        }
    }
}

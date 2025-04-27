using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    public GameObject damageNumberPrefab;

    public void TakeDamage(float amount, Vector3 hitPoint)
    {
        // Apply health logic here

        var dmgObj = Instantiate(damageNumberPrefab, hitPoint, Quaternion.identity);
        dmgObj.GetComponent<DamageNumber>().SetDamage(amount);
    }
}

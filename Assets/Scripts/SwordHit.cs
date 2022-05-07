using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordHit : MonoBehaviour
{
    public bool attacking = false;
    public bool struck = false;

    private void OnTriggerEnter(Collider other)
    {
        if(attacking && other.transform.tag == "Crab" && !struck)
        {
            Debug.Log("Hit crab");
            //deal damage
            other.GetComponent<EnemyController>().health -= 5;
            struck = true;
        }
    }
}

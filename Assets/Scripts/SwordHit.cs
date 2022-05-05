using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordHit : MonoBehaviour
{
    public bool attacking = false;

    private void OnTriggerEnter(Collider other)
    {
        if(attacking && other.transform.tag == "Crab")
        {
            //deal damage
        }
    }
}

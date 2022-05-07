using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    public float speed = 25f;
    public float liveTime = 10f;

    // Start is called before the first frame update
    void Start()
    {
        FindObjectOfType<AudioManager>().PlayUninterrupted("Ame Screech");
    }

    void Update()
    {
        //move relative to up orientation
        transform.position += transform.up * speed * Time.deltaTime;
        transform.RotateAround(transform.position, transform.up, 120f * Time.deltaTime);
        liveTime -= Time.deltaTime;
        if(liveTime <= 0f)
        {
            transform.gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<PlayerMovement>().TakeDamage();
        }
        transform.gameObject.SetActive(false);
    }
}

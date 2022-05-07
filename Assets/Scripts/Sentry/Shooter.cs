using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    public GameObject projectile;
    public Transform projectileSpawner;
    EnemyController sentryScript;
    PointAt pointScript;
    public float shootTime = 5f;
    bool canShoot = true;

    // Start is called before the first frame update
    void Start()
    {
        pointScript = GetComponent<PointAt>();

        Transform xform = gameObject.transform;
        do
        {
            if (GetComponentInParent<EnemyController>() != null)
            {
                sentryScript = GetComponentInParent<EnemyController>();
                break;
            }
            xform = xform.parent;
        } while (xform != null);
    }

    void Update()
    {
        if (pointScript.target)
        {
            float distToPlayer = Vector3.SqrMagnitude(pointScript.target.position - transform.position);

            //time ticks if < 30 units away
            if (distToPlayer <= 900f)
            {
                shootTime -= Time.deltaTime;
                /*if (shootTime <= 0f)
                {
                    //shoot
                    if (canShoot) Shoot();
                    canShoot = false;

                    //reset time (moved to EnemyController)
                    //shootTime += 5f;
                }
                else canShoot = true;*/
            }
        }
    }

    public void Shoot()
    {
        Instantiate(projectile, projectileSpawner.position, projectileSpawner.rotation);
    }
}

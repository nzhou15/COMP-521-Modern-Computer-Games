using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletEmitter : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    GameObject prevBullet;

    public float bulletSpeed = 3f;

    void Update()
    {   
        // only one projectile may exist at a time
        // checks if the previous bullet still exists
        if (prevBullet == null && Input.GetButtonDown("Fire1"))
        {
            Fire();
        }
    }

    private void Fire()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.transform.position, bulletSpawn.transform.rotation);
        bullet.GetComponent<Rigidbody>().AddForce(bulletSpawn.forward * bulletSpeed, ForceMode.Impulse);

        // ignores collision between player and bullet
        Physics.IgnoreCollision(bullet.GetComponent<Collider>(), bulletSpawn.parent.GetComponent<Collider>());

        Destroy(bullet, 3f);    // destorys bullet after its life time

        prevBullet = bullet;
    }
}

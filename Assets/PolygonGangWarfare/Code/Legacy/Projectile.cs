using System;
using UnityEngine;
using System.Collections;
using InfimaGames.LowPolyShooterPack;
using Random = UnityEngine.Random;

public class Projectile : MonoBehaviour
{

    [Header("Damage Settings")]
    [Tooltip("How much damage this projectile deals.")]
    public float damage = 25f;

    [Range(5, 100)]
    [Tooltip("After how long time should the bullet prefab be destroyed?")]
    public float destroyAfter;
    [Tooltip("If enabled the bullet destroys on impact")]
    public bool destroyOnImpact = false;
    [Tooltip("Minimum time after impact that the bullet is destroyed")]
    public float minDestroyTime;
    [Tooltip("Maximum time after impact that the bullet is destroyed")]
    public float maxDestroyTime;

    [Header("Impact Effect Prefabs")]
    public Transform[] bloodImpactPrefabs;
    public Transform[] metalImpactPrefabs;
    public Transform[] dirtImpactPrefabs;
    public Transform[] concreteImpactPrefabs;

    private void Start()
    {
        StartCoroutine(DestroyAfter());
    }
    public void Setup(Transform shooterRoot)
    {
        Collider myCollider = GetComponent<Collider>();

        Collider[] shooterColliders = shooterRoot.GetComponentsInChildren<Collider>();

        foreach (Collider col in shooterColliders)
        {
            Physics.IgnoreCollision(myCollider, col);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        Hitbox hitbox = collision.collider.GetComponent<Hitbox>();
        if (hitbox != null)
        {
            hitbox.ApplyDamage(damage);
        }
        else
        {
            Health targetHealth = collision.collider.GetComponentInParent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage); // Звичайна шкода без множника
            }
        }
        //Ignore collisions with other projectiles.
        if (collision.gameObject.GetComponent<Projectile>() != null)
            return;

       

        //If destroy on impact is false, start 
        //coroutine with random destroy timer
        if (!destroyOnImpact)
        {
            StartCoroutine(DestroyTimer());
        }
        //Otherwise, destroy bullet on impact
        else
        {
            Destroy(gameObject);
        }

        //If bullet collides with "Blood" tag
        if (collision.transform.CompareTag("Blood"))
        {
            //Instantiate random impact prefab from array
            Instantiate(bloodImpactPrefabs[Random.Range
                (0, bloodImpactPrefabs.Length)], transform.position,
                Quaternion.LookRotation(collision.contacts[0].normal));
            //Destroy bullet object
            Destroy(gameObject);
        }

        //If bullet collides with "Metal" tag
        if (collision.transform.CompareTag("Metal"))
        {
            //Instantiate random impact prefab from array
            Instantiate(metalImpactPrefabs[Random.Range
                (0, metalImpactPrefabs.Length)], transform.position,
                Quaternion.LookRotation(collision.contacts[0].normal));
            //Destroy bullet object
            Destroy(gameObject);
        }

        //If bullet collides with "Dirt" tag
        if (collision.transform.CompareTag("Dirt"))
        {
            //Instantiate random impact prefab from array
            Instantiate(dirtImpactPrefabs[Random.Range
                (0, dirtImpactPrefabs.Length)], transform.position,
                Quaternion.LookRotation(collision.contacts[0].normal));
            //Destroy bullet object
            Destroy(gameObject);
        }

        //If bullet collides with "Concrete" tag
        if (collision.transform.CompareTag("Concrete"))
        {
            //Instantiate random impact prefab from array
            Instantiate(concreteImpactPrefabs[Random.Range
                (0, concreteImpactPrefabs.Length)], transform.position,
                Quaternion.LookRotation(collision.contacts[0].normal));
            //Destroy bullet object
            Destroy(gameObject);
        }

        //If bullet collides with "Target" tag
        if (collision.transform.CompareTag("Target"))
        {
            //Toggle "isHit" on target object
            if (collision.transform.TryGetComponent<TargetScript>(out var target))
                target.isHit = true;
            //Destroy bullet object
            Destroy(gameObject);
        }

        //If bullet collides with "ExplosiveBarrel" tag
        if (collision.transform.CompareTag("ExplosiveBarrel"))
        {
            //Toggle "explode" on explosive barrel object
            if (collision.transform.TryGetComponent<ExplosiveBarrelScript>(out var barrel))
                barrel.explode = true;
            //Destroy bullet object
            Destroy(gameObject);
        }

        //If bullet collides with "GasTank" tag
        if (collision.transform.CompareTag("GasTank"))
        {
            //Toggle "isHit" on gas tank object
            if (collision.transform.TryGetComponent<GasTankScript>(out var tank))
                tank.isHit = true;
            //Destroy bullet object
            Destroy(gameObject);
        }
    }

    private IEnumerator DestroyTimer()
    {
        //Wait random time based on min and max values
        yield return new WaitForSeconds
            (Random.Range(minDestroyTime, maxDestroyTime));
        //Destroy bullet object
        Destroy(gameObject);
    }

    private IEnumerator DestroyAfter()
    {
        //Wait for set amount of time
        yield return new WaitForSeconds(destroyAfter);
        //Destroy bullet object
        Destroy(gameObject);
    }
}
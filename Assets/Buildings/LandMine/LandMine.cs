using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandMine : MonoBehaviour
{
    public float radius = 3f;
    public string tagToDetect = "Enemy";
    public ParticleSystem explosionEffect;
    public float damage = 1f;
    public MeshRenderer MeshRenderer;
    private bool mineEnabled = false;

    public void PlaceMine()
    {
        mineEnabled = true;
        MeshRenderer.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagToDetect))
        {
            Explode();
        }
    }
    private void DamageEnemiesInRadius()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
        List<GameObject> objectsWithTag = new List<GameObject>();

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(tagToDetect))
            {
                objectsWithTag.Add(hitCollider.gameObject);
            }
        }
        foreach (var obj in objectsWithTag)
        {
            obj.GetComponent<Enemy>().TakeDamage(damage);
        }
    }
    public void Explode()
    {
        DamageEnemiesInRadius();
        if (explosionEffect != null)
        {
            explosionEffect.transform.SetParent(null);

            explosionEffect.transform.position = transform.position;

            explosionEffect.Play();

            Destroy(explosionEffect.gameObject, 2f);
        }
        Destroy(gameObject);
    }
}

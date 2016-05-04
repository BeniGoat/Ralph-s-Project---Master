using UnityEngine;
using System.Collections;

public class MissileGuidance : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 targetPos;
    private SteeringBehaviours steeringBehaviours;

    public GameObject fuselage, currentTarget, explosionSmall, explosionLarge;
    public ParticleSystem smokeTrail;
    public float missileSpeed, missileLifetime, turnRate;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        steeringBehaviours = GetComponent<SteeringBehaviours>();
        StartCoroutine(MissileSelfDestruct());
    }

    void FixedUpdate()
    {
        if (steeringBehaviours.FindClosestShip(gameObject, GameManager.instance.enemyList) == null)

            rb.velocity *= missileSpeed;

        else
        {
            currentTarget = steeringBehaviours.FindClosestShip(gameObject, GameManager.instance.enemyList);
            targetPos = currentTarget.transform.position;
            rb.AddForce(steeringBehaviours.Seek(targetPos, turnRate) * missileSpeed);
        }
    }

    void OnCollisionEnter(Collision other)
    {
        StopAllCoroutines();

        ParticleSystem.EmissionModule emission = smokeTrail.emission;
        ParticleSystem.MinMaxCurve rate = new ParticleSystem.MinMaxCurve();
        rate.constantMax = 0.0f;
        emission.rate = rate;
        explosionLarge = Instantiate(explosionLarge, transform.position, transform.rotation) as GameObject;
        if (other.gameObject.name != "Mothership")
        {
            explosionLarge = Instantiate(explosionLarge, other.transform.position, other.transform.rotation) as GameObject;
            Destroy(other.gameObject);
        }
        Destroy(fuselage);
        Destroy(explosionLarge, 3f);
        Destroy(gameObject, 3f);
    }

    // Coroutine to destroy a missile if it misses and runs out of fuel
    IEnumerator MissileSelfDestruct()
    {
        yield return new WaitForSeconds(missileLifetime);

        ParticleSystem.EmissionModule emission = smokeTrail.emission;
        ParticleSystem.MinMaxCurve rate = new ParticleSystem.MinMaxCurve();
        rate.constantMax = 0.0f;
        emission.rate = rate;
        explosionSmall = Instantiate(explosionSmall, transform.position, transform.rotation) as GameObject;

        Destroy(fuselage);
        Destroy(explosionSmall, 3f);
        Destroy(gameObject, 5f);
    }
}

using UnityEngine;
using System.Collections.Generic;

public class Flocking : MonoBehaviour
{
    private Rigidbody rb;
    private GameObject explosion;
    private Vector3 targetPos;
    private SteeringBehaviours steeringBehaviours;
    private bool outOfBounds = false;

    public GameObject explosionLarge, explosionHuge;
    [HideInInspector]
    public GameObject targetObject;
    public float seperationWeight, cohesionWeight, alignWeight, wanderWeight, initialSpeedFactor,
                 maxForceMag = 10f,
                 maxSpeed = 50f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        steeringBehaviours = GetComponent<SteeringBehaviours>();
        // Give the enemy an initial random velocity in a random vector
        initialSpeedFactor = Random.Range(0.1f, 1);
        rb.velocity = Random.insideUnitCircle * maxSpeed * initialSpeedFactor;
        // Add enemy to list of enemies 
        GameManager.instance.enemyList.Add(gameObject);
    }

    void FixedUpdate()
    {
        targetObject = steeringBehaviours.FindClosestShip(gameObject, GameManager.instance.friendlyList);
        if (targetObject != null)
        {
            targetPos = targetObject.transform.position;
            rb.AddForce(SetCourse());
        }

        transform.position = new Vector3
            (
            Mathf.Clamp(transform.position.x, -2048, 2048),
            Mathf.Clamp(transform.position.y, -2048, 2048),
            0.0f);
    }

    public Vector3 Seperation()
    {
        Vector3 steeringForce = Vector3.zero;
        // iterate through the game objects
        for (int i = 0; i < GameManager.instance.enemyList.Count; i++)
        {
            // store entity locally
            GameObject entity = GameManager.instance.enemyList[i];
            if (entity != null)
            {
                // get vector between objects
                Vector3 toEntity = transform.position - entity.transform.position;
                // adjust the force based on distance
                steeringForce += toEntity.normalized / toEntity.magnitude;
            }
        }
        return steeringForce;
    }

    public Vector3 Cohesion()
    {
        Vector3 steeringForce = Vector3.zero;
        Vector3 centreOfMass = Vector3.zero;
        int taggedCount = 0;
        // iterate through the entities
        foreach (GameObject entity in GameManager.instance.enemyList)
        {
            // check if another enemy
            if (entity != gameObject)
            {
                // add to centre of mass
                centreOfMass += entity.transform.position;
                // count how many enemies checked
                taggedCount++;
            }
        }
        // if there is at least one boid
        if (taggedCount > 0)
        {
            // find average centre of mass
            centreOfMass /= taggedCount;
            // if we are already at the centre of mass
            if (centreOfMass.sqrMagnitude == 0)
            {
                // just return nothing
                return Vector3.zero;
            }
            else
            {
                // if we aren't at the centre of mass, seek towards it
                steeringForce = Vector3.Normalize(steeringBehaviours.Seek(centreOfMass));
            }
        }
        return steeringForce;
    }

    public Vector3 Alignment()
    {
        Vector3 steeringForce = Vector3.zero;
        int taggedCount = 0;

        foreach (GameObject entity in GameManager.instance.enemyList)
        {
            if (entity != gameObject)
            {
                // add the facing direction of the entity to the steering force
                steeringForce += entity.transform.up;
                // count the enemy
                taggedCount++;
            }
        }
        // if there is at least one enemy
        if (taggedCount > 0)
        {
            // find average centre of mass
            steeringForce /= taggedCount;
            // adjust for the direction of the gameObject we're working on
            steeringForce -= transform.up;
        }
        return steeringForce;
    }

    // Accumulate the flocking forces
    // Returns true if force was accumulated
    // Running total will be updated by reference
    private bool AccumulateForce(ref Vector3 runningTotal, Vector3 force)
    {
        // how much force has been accumulated
        float soFar = runningTotal.magnitude;
        // how much force budget do we have left?
        float remaining = maxForceMag - soFar;
        // if we are at or above maximum force, exit
        if (remaining <= 0)
        {
            return false;
        }
        // calculate the magnitude of the force we want to add
        float toAdd = force.magnitude;
        // check if we have room to add force
        if (toAdd < remaining)
        {
            // if so, add the new force
            runningTotal += force;
        }
        else
        {
            // if not, just return the maximum (add truncated force)
            runningTotal += force.normalized * remaining;
        }
        return true;
    }

    private Vector3 SetCourse()
    {
        // initialise the force
        Vector3 force = Vector3.zero;
        Vector3 steeringForce = Vector3.zero;
        // if we found some game objects within the radius
        if (GameManager.instance.enemyList.Count > 0)
        {
            // seperation adjusted by weight
            force = Seperation() * seperationWeight;
            // check if this uses up our force budget
            if (!AccumulateForce(ref steeringForce, force))
            {
                return steeringForce;
            }
            // cohesion adjusted by weight
            force = Cohesion() * cohesionWeight;
            // check if this uses up our force budget
            if (!AccumulateForce(ref steeringForce, force))
            {
                return steeringForce;
            }
            // alignment adjusted by weight
            force = Alignment() * alignWeight;
            // check if this uses up our force budget
            if (!AccumulateForce(ref steeringForce, force))
            {
                return steeringForce;
            }
            force = steeringBehaviours.Seek(targetPos) * wanderWeight;
            if (!AccumulateForce(ref steeringForce, force))
            {
                return steeringForce;
            }
        }
        return steeringForce;
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Friendly")
        {
            if (other.gameObject.name == "Mothership" && GameManager.instance.mothershipHealth == 1)
            {
                explosion = Instantiate(explosionHuge, other.transform.position, other.transform.rotation) as GameObject;
                Destroy(other.gameObject);
            }
            else if (other.gameObject.name == "Mothership" && GameManager.instance.mothershipHealth > 1)
            {
                explosion = Instantiate(explosionLarge, transform.position, transform.rotation) as GameObject;
                GameManager.instance.mothershipHealth--;
            }
            else
            {
                explosion = Instantiate(explosionLarge, other.transform.position, other.transform.rotation) as GameObject;
            }
            Destroy(gameObject);
            Destroy(explosion, 3f);
        }
    }
}

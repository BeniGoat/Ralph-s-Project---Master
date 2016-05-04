using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SteeringBehaviours : MonoBehaviour
{
    private Rigidbody rb;
    public float maxPrediction = 1f;
    public float maxAcceleration = 50f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public Vector3 Seek(Vector3 seekTarget, float turnRate)
    {
        Vector3 targetPos = seekTarget - transform.position;
        Vector3 currentDir = Vector3.RotateTowards(transform.up, targetPos, Mathf.Deg2Rad * turnRate * Time.deltaTime, 0.0f);
        currentDir.z = 0f;
        currentDir.Normalize();
        transform.up = currentDir;
        return currentDir;
    }

    public Vector3 Seek(Vector3 targetPosition)
    {
        return Seek(targetPosition, maxAcceleration);
    }

    public Vector3 Intercept(GameObject interceptTarget, float turnRate)
    {
        // Calculate the distance to the target
        Vector3 targetPos = interceptTarget.transform.position - transform.position;
        float distance = targetPos.magnitude;

        // Get the character's speed
        float speed = rb.velocity.magnitude;

        // Calculate the prediction time
        float prediction;
        if (speed <= distance / maxPrediction)
        {
            prediction = maxPrediction;
        }
        else
        {
            prediction = distance / speed;
        }
        Rigidbody targetRB = interceptTarget.GetComponent<Rigidbody>();
        Vector3 targetVel = new Vector3();
        if (targetRB != null)
            targetVel = targetRB.velocity;
        // Put the target together based on where we think the target will be
        Vector3 explicitTarget = interceptTarget.transform.position + targetVel * prediction;

        return Seek(explicitTarget, turnRate);
    }

    public Vector3 Flee(Vector3 fleeTarget, float turnRate)
    {
        Vector3 targetPos = transform.position - fleeTarget;
        Vector3 currentDir = Vector3.RotateTowards(transform.up, targetPos, Mathf.Deg2Rad * turnRate * Time.deltaTime, 0.0f);
        currentDir.z = 0f;
        currentDir.Normalize();
        transform.up = currentDir;
        return currentDir;
    }

    public Vector3 Evade(GameObject evadeTarget, float turnRate)
    {
        // Calculate the distance to the target
        Vector3 targetPos = evadeTarget.transform.position - transform.position;
        float distance = targetPos.magnitude;

        // Get the targets's speed
        Rigidbody targetRB = evadeTarget.GetComponent<Rigidbody>();
        Vector3 targetVel = new Vector3();
        if (targetRB != null)
            targetVel = targetRB.velocity;
        float speed = targetVel.magnitude;

        // Calculate the prediction time
        float prediction;
        if (speed <= distance / maxPrediction)
        {
            prediction = maxPrediction;
        }
        else
        {
            prediction = distance / speed;
            //Place the predicted position a little before the target reaches the character
            prediction *= 0.9f;
        }

        // Put the target together based on where we think the target will be
        Vector3 explicitTarget = evadeTarget.transform.position + targetVel * prediction;

        return Flee(explicitTarget, turnRate);
    }

    public Vector3 ObjectAvoidance(RaycastHit hit, float distanceToAvoid)
    {
        Vector3 avoidTarget = hit.point + hit.normal * distanceToAvoid;
        return Seek(avoidTarget);
    }

    public GameObject FindClosestShip(GameObject obj, List<GameObject> objList)
    {
        GameObject[] gos = objList.ToArray();
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = obj.transform.position;
        for (int i = 0; i < gos.Length; i++)
        {
            if (gos[i] == null)
                return null;
            Vector3 diff = gos[i].transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = gos[i];
                distance = curDistance;
            }
        }
        return closest;
    }
}

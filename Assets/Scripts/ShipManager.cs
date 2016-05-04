using UnityEngine;

public class ShipManager : MonoBehaviour
{
    private Rigidbody rb;
    private GameObject explosion;
    private RaycastHit hit;
    private Vector3 targetPos;
    private SteeringBehaviours steeringBehaviours;
    private bool collisionDetection = false;

    [HideInInspector]
    public GameObject currentTarget;
    public GameObject explosionLarge;
    public bool isFiring = false;
    public int ammoCount = 10;
    public float avoidDistance = 100f;
    public float distanceToAvoid = 50f;
    public float maxSpeed = 100f;
    public float maxAngle = 50f;
    public float turnRate = 30f;

    public enum AgentState { Evade, Flee, Intercept, ObjectAvoidance, Seek };
    public AgentState aiState;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        steeringBehaviours = GetComponent<SteeringBehaviours>();
        // Add this gameobject to the friendly ships list
        GameManager.instance.friendlyList.Add(gameObject);
    }

    void FixedUpdate()
    {
        ShipAI();
    }
    // Ship will examine its surroundings and evaluate its current state
    void ShipAI()
    {
        collisionDetection = Physics.Raycast(transform.position, rb.velocity.normalized * avoidDistance, out hit, avoidDistance);
        // Check if ammo is depleted or if there are no active enemies in the vicinity
        if (ammoCount < 1 || steeringBehaviours.FindClosestShip(gameObject, GameManager.instance.enemyList) == null)
        {
            // Rearm at mothership
            targetPos = GameManager.instance.mothership.transform.position;
            aiState = AgentState.Seek;
        }
        else
        {
            currentTarget = steeringBehaviours.FindClosestShip(gameObject, GameManager.instance.enemyList);
            targetPos = currentTarget.transform.position;

            if (InsideShootingAngle(currentTarget))
                isFiring = true;
            else
                isFiring = false;

            //Establish the states the AI can be in
            switch (aiState)
            {
                case AgentState.Seek:
                    rb.velocity = steeringBehaviours.Seek(targetPos, turnRate) * maxSpeed;
                    break;
                case AgentState.Intercept:
                    rb.velocity = steeringBehaviours.Intercept(currentTarget, turnRate) * maxSpeed;
                    break;
                case AgentState.Flee:
                    rb.velocity = steeringBehaviours.Flee(targetPos, turnRate) * maxSpeed;
                    break;
                case AgentState.Evade:
                    rb.velocity = steeringBehaviours.Evade(currentTarget, turnRate) * maxSpeed;
                    break;
                case AgentState.ObjectAvoidance:
                    rb.velocity = steeringBehaviours.ObjectAvoidance(hit, distanceToAvoid) * maxSpeed;
                    break;
            }
            // If in danger of collision, alter course to avoid obstacle
            if (collisionDetection)
                
                aiState = AgentState.ObjectAvoidance;
            else
            {            
                // Set conditions which cause each state to activate
                if (Vector3.Distance(targetPos, transform.position) > GameManager.engagementRange)
                    aiState = AgentState.Seek;
                if (Vector3.Distance(targetPos, transform.position) > GameManager.engagementRange * 0.25f)
                    aiState = AgentState.Intercept;
                else
                    aiState = AgentState.Evade;
            }
        }
    }

    // Returns true if current target is within a certain angle in front of the ship
    public bool InsideShootingAngle(GameObject targetObject)
    {
        Vector3 targetPos = targetObject.transform.position - transform.position;
        float left = Vector3.Dot(transform.up, targetPos.normalized) * 90f;
        float right = Vector3.Dot(-transform.up, targetPos.normalized) * 90f;
        return left > maxAngle && right < -maxAngle;
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Friendly")
        {
            if (other.gameObject.name != "Mothership")
            {
                explosion = Instantiate(explosionLarge, transform.position, transform.rotation) as GameObject;
                Destroy(gameObject);
            }
            else
                Destroy(gameObject);
        }
        else
        {
            explosion = Instantiate(explosionLarge, transform.position, transform.rotation) as GameObject;
            Destroy(gameObject);
        }
        Destroy(explosion, 3f);
    }
}

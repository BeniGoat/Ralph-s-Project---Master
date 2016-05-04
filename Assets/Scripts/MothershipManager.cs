using UnityEngine;

public class MothershipManager : MonoBehaviour
{
    private Rigidbody rb;
    private GameObject currentTarget;
    private Vector3 targetPos;

    public SteeringBehaviours steeringBehaviours;
    public GameObject fighterPrefab;
    public Transform[] fighterSpawnPoints;
    public float spawnTime = 5f;
    public float maxSpeed = 25f;
    public float turnRate = 10f;

    public enum AgentState { Evade, Flee, Intercept, Seek };
    public AgentState aiState;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        steeringBehaviours = GetComponent<SteeringBehaviours>();
        // Add this gameobject to the friendly ships list
        GameManager.instance.friendlyList.Add(gameObject);
        // Repeating method to launch fighters
        InvokeRepeating("SpawnFighter", spawnTime, spawnTime);
    }

    void FixedUpdate()
    {
        ShipAI();
    }

    // Ship will examine its surroundings and evaluate its current state
    void ShipAI()
    {
        if (steeringBehaviours.FindClosestShip(gameObject, GameManager.instance.enemyList) == null)
       
            rb.velocity = steeringBehaviours.Seek(Vector3.zero, turnRate) * maxSpeed;
        
        else
        {
            currentTarget = steeringBehaviours.FindClosestShip(gameObject, GameManager.instance.enemyList);
            targetPos = currentTarget.transform.position;
        }

        // Establish the states the AI can be in
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
        }
        // Set conditions which cause each state to activate
        if (Vector3.Distance(targetPos, transform.position) > GameManager.engagementRange)
            aiState = AgentState.Seek;
        if (Vector3.Distance(targetPos, transform.position) > GameManager.engagementRange * 0.25f)
            aiState = AgentState.Intercept;
        else
            aiState = AgentState.Evade;
    }

    void SpawnFighter()
    {
        // Check if there is enemy activity within sensor range
        currentTarget = steeringBehaviours.FindClosestShip(gameObject, GameManager.instance.enemyList);
        targetPos = currentTarget.transform.position;
        if (Vector3.Distance(targetPos, transform.position) < GameManager.engagementRange)
        {
            for (int i = 0; i < fighterSpawnPoints.Length; i++)
            {
                // If so, launch Vipers!
                GameObject fighterClone = Instantiate(fighterPrefab, fighterSpawnPoints[i].position, fighterSpawnPoints[i].rotation) as GameObject;
            }
        }
        else
            return;
    }
}

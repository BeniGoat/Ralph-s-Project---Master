using UnityEngine;
using System.Collections;

public class MissileLauncher : MonoBehaviour
{
    private GameObject target;
    private ShipManager shipManager;
    private Vector3 targetPos, launchSpeed;
    private float fireTimer;

    public GameObject missilePrefab;
    public float weaponsRange, fireRate;

    void Start()
    {
        shipManager = GetComponentInParent<ShipManager>();
        // Establishing weapons range to be half of sensor range
        weaponsRange = GameManager.engagementRange * 0.5f;
        // Setting weapon cooldown time
        fireTimer = fireRate;
    }

    void Update()
    {
        // Get this launcher's nearest target
        target = shipManager.currentTarget;

        if (target != null)
        {
            targetPos = target.transform.position - transform.position;

            // check if target is in range, within weapons arc, ship still has ammo, and that the missile is ready
            if (targetPos.magnitude < weaponsRange
                && shipManager.isFiring == true
                && shipManager.ammoCount > 0
                && Time.time > fireTimer)
            {
                // And if so fire missile
                fireTimer = Time.time + fireRate;
                // Get the firing ship's velocity at launch
                launchSpeed = GetComponentInParent<Rigidbody>().velocity;
                GameObject missileClone = Instantiate(missilePrefab, transform.position, transform.rotation) as GameObject;
                // Impart the missile with initial velocity of ship
                missileClone.GetComponent<Rigidbody>().velocity = launchSpeed;
                // Reduce ammo count
                shipManager.ammoCount--;
            }
        }
    }
}

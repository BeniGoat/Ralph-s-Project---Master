using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class GameManager : MonoBehaviour
{
    private bool isPaused = false;
    private float enemiesRemaining;

    public List<GameObject> enemyList, friendlyList;
    public static GameManager instance = null;
    public GameObject mothership, enemyPrefab;
    public Transform[] enemySpawnPoints;

    public int mothershipHealth = 10, spawnTime = 3;
    public static float engagementRange = 2000f;

    void Awake()
    {
        // Check if instance already exists
        if (instance == null)

            // If not, set instance to this
            instance = this;

        // If instance already exists and it's not this
        else if (instance != this)

            // Then destroy this
            Destroy(gameObject);
    }

    void Start()
    {
        enemyList = new List<GameObject>();
        friendlyList = new List<GameObject>();
        
        mothership = GameObject.Find("Mothership");
        // Spawns 3 new enemies every 3 seconds
        InvokeRepeating("SpawnEnemy", 0f, spawnTime);
    }

    void Update()
    {
        // Ceases spawning new enemies after 30 seconds
        if(Time.time > 30f)
            CancelInvoke();

        // Remove empty entries in enemy list
        enemyList.RemoveAll(item => item == null);
        // Remove empty entries in frendly list
        friendlyList.RemoveAll(item => item == null);

        enemiesRemaining = enemyList.Count;

        InputManager();

        // Exits Play Mode if winning conditions are achieved
        if (IsBattleOver())
            EditorApplication.isPlaying = false;
    }

    void InputManager()
    {
        //pausng the game
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isPaused)
            {
                Time.timeScale = 0;
                isPaused = true;
            }
            else
            {
                Time.timeScale = 1;
                isPaused = false;
            }
        }
    }
    // Checks if winning conditions are met
    bool IsBattleOver()
    {
        // Have all the enemies been destroyed, or has the mothership been destroyed??
        if (enemiesRemaining == 0 || !mothership.activeInHierarchy)
            return true;
        else
            return false;
    }
    // Cycles through spawnpoints and spawns an enemy at each
    void SpawnEnemy()
    {
        for (int i = 0; i < enemySpawnPoints.Length; i++)
        {
            GameObject enemyClone = Instantiate(enemyPrefab, enemySpawnPoints[i].position, enemySpawnPoints[i].rotation) as GameObject;
        }
    }
}

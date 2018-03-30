using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestWaveSpawner : MonoBehaviour {

    public List<GameObject> Enemies = new List<GameObject>();
    List<GameObject> EnemiesLeft = new List<GameObject>();
    List<GameObject> AllowedEnemies = new List<GameObject>();

    public GameObject FodderEnemy;
    public GameObject RangedEnemy;

    float TimeBetweenSpawn = 0;
    float CurrentTime = 0;

    bool CurrentlySpawning = false;
    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update()
    {
        if (EnemiesLeft.Count > 0)
        {
            CurrentTime -= Time.deltaTime;
            if (CurrentTime <= 0)
            {
                SpawnEnemy();
                CurrentTime = TimeBetweenSpawn;
            }
        }
        else
        {
            CurrentlySpawning = false;
        }
    }

    void SpawnEnemy ()
    {
        // find a random spawn
        GameObject[] enemySpawns = GameObject.FindGameObjectsWithTag("EnemySpawn");
        int spawnNumber = Random.Range(0, enemySpawns.Length);

        // spawn in our enemy
        Instantiate(EnemiesLeft[EnemiesLeft.Count - 1], enemySpawns[spawnNumber].transform.position, enemySpawns[spawnNumber].transform.rotation);

        EnemiesLeft.Remove(EnemiesLeft[EnemiesLeft.Count - 1]);
    }

    // Creates a wave based on the amount of points allowed, the difficulty of the enemies, and the total amount of time in seconds it takes to spawn all enemies
    public void CreateWave(int points, int difficulty, int length)
    {
        // safety check, make sure that if we are already spawning in a wave you can't create a new one
        if (CurrentlySpawning)
        {
            return;
        }
        // Clear the lists of enemies we have
        AllowedEnemies.Clear();
        Enemies.Clear();
        // based on the difficulty of the wave allow certain enemies to spawn
        switch (difficulty)
        {
            case 1:
                AllowedEnemies.Add(FodderEnemy);
                AllowedEnemies.Add(RangedEnemy);
                break;
            case 2:
                break;
            case 3:
                break;
            default:
                break;
        }

        // if we still have points left we will still spawn enemies
        while (points > 0)
        {
            // find the index of the allowed enemy we will spawn
            int enemy = (int)Random.Range(0, AllowedEnemies.Count);

            // make sure we have enough points left to spawn the enemy we want to spawn
            // if not find a new enemy
            while (true)
            {
                if (AllowedEnemies[enemy].GetComponent<CharacterStats>().ScorePointValue > points)
                {
                    int newEnemy = (int)Random.Range(0, AllowedEnemies.Count);
                    enemy = newEnemy;
                }
                else
                    break;
            }

            // add the enemy to the list of enemies
            Enemies.Add(AllowedEnemies[enemy]);

            // subtract the enemies point value from the total points allowed
            points -= (int)AllowedEnemies[enemy].GetComponent<CharacterStats>().ScorePointValue;

            // make sure we have enough total points left to spawn an allowed enemy
            // if not leave this loop
            int counter = 0;
            for (int i = 0; i < AllowedEnemies.Count; i++)
            {
                if (AllowedEnemies[i].GetComponent<CharacterStats>().ScorePointValue > points)
                    counter++;
            }
            if (counter == AllowedEnemies.Count)
                break;
        }

        // EnemiesLeft is used when actually spawning the wave
        EnemiesLeft = Enemies;

        // find the time between enemy spawns
        TimeBetweenSpawn = (float)(length) / (float)Enemies.Count;
        CurrentTime = TimeBetweenSpawn;

        CurrentlySpawning = true;
    }
}

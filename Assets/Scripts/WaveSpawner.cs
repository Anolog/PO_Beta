using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyList
{
    FodderEnemy,
    RockThrowEnemy,
    SupportEnemy,
    Boss,
    NumEnemies,
}


public class WaveSpawner : MonoBehaviour
{
    Game m_Game;

    public bool LogData = false;

    public static float TIME_BEFORE_FIRST_WAVE = 1f;
    public static float TIME_BETWEEN_WAVES = 5f;
    public static int MINIMUM_POINTS_LEFT = 20;

    public int TimeScaleFactor = 5;

    public List<GameObject> Enemies = new List<GameObject>();
    private List<GameObject> m_EnemiesLeft = new List<GameObject>();

    public void OverrideEnemiesLeft() { m_EnemiesLeft = new List<GameObject>(); }

    private List<GameObject> m_AllowedEnemies = new List<GameObject>();

    //public GameObject FodderEnemy;
    //public GameObject RangedEnemy;

    private float m_TimeBetweenSpawn = 0;
    private float m_CurrentTime = 0;
    float m_WaveTime;

    public int PointsInScene = 0;

    DesiredPointsFunction m_DesiredPoints;

    private bool m_CurrentlySpawning = false;
    private bool m_IsStopped = false;
    private bool m_SpawnInvincible = false;

    public void SetSpawnInvincible(bool spawnInvincible) { m_SpawnInvincible = spawnInvincible; }

    public void OverrideCurrentWave() { m_CurrentlySpawning = false; }

    private void Awake()
    {
        m_Game = GetComponent<Game>();
    }

    public void StopSpawning()
    {
        m_IsStopped = true;
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Command.KillEnemy(enemy);
        }
        foreach (GameObject boss in GameObject.FindGameObjectsWithTag("Boss"))
        {
            if (boss != null)
            {
                Command.KillBoss(boss);
            }
        }
    }

    public void StartSpawning()
    {
        m_IsStopped = false;
    }
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!m_IsStopped)
        {
            if (m_EnemiesLeft.Count > 0)
            {
                int desiredPoints = m_DesiredPoints(m_WaveTime);
                int currentPoints = PointsInScene;

                int pointDifference = desiredPoints - currentPoints;
                pointDifference = Mathf.Clamp(pointDifference, -10 * TimeScaleFactor, 10 * TimeScaleFactor);
                //pointDifference /= TimeScaleFactor;
                //pointDifference = Mathf.Abs(pointDifference);

                float scaledFrameTime = Time.deltaTime;
                //don't change if Scale factor is between 1 and -1, it means you're pretty close and it messes up the formula
                if (pointDifference > 1)
                    scaledFrameTime *= pointDifference;
                else if (pointDifference < -1)
                    scaledFrameTime /= -pointDifference;

                m_WaveTime += scaledFrameTime;
                m_CurrentTime -= scaledFrameTime;

                if (m_CurrentTime <= 0)
                {
                    SpawnEnemy();
                    m_CurrentTime += m_TimeBetweenSpawn;
                }

                UnityEngine.UI.Text info = GetComponentInChildren<UnityEngine.UI.Text>();

                if (LogData)
                {
                    info.text =
                        "Desired Points: " + desiredPoints.ToString() +
                        "\nCurrent Points: " + currentPoints.ToString() +
                        "\nAdjusted Point Difference: " + pointDifference.ToString() +
                        "\nRelative Time: " + scaledFrameTime.ToString();
                }
                else
                {
                    info.text = string.Empty;
                }
            }
            else
            {
                m_CurrentlySpawning = false;
            }
        }
    }

    public void SpawnBoss(GameObject boss)
    {
        if (m_SpawnInvincible)
        {
            boss.GetComponent<CharacterStats>().SetInvincible(true);
        }
        //GameObject BossSpawn = GameObject.FindGameObjectWithTag("BossSpawn");
        Instantiate(boss, Vector3.zero - new Vector3(0,0.75f,3), Quaternion.identity);
        Enemies.Add(boss);

        //GameObject.FindGameObjectWithTag("Boss").GetComponent<Animator>().SetTrigger("Spawn");

        m_Game.StartCutscene();
    }

    public int NumberEnemiesSpawned()
    {
        int totalEnemies = Enemies.Count;
        int numLeft = m_EnemiesLeft.Count;
        return totalEnemies - numLeft;
    }

    public void SpawnEnemy(GameObject enemy, int amount)
    {
        if (m_SpawnInvincible)
        {
            enemy.GetComponent<CharacterStats>().SetInvincible(true);
        }

        StartCoroutine(SpawnEnemiesOverTimeCoroutine(enemy, amount));
    }

    IEnumerator SpawnEnemiesOverTimeCoroutine(GameObject enemy, float amount)
    {
        for (int i = 0; i < amount; ++i)
        {
            GameObject[] enemySpawns = GameObject.FindGameObjectsWithTag("EnemySpawn");
            int spawnNumber = Random.Range(0, enemySpawns.Length);

            Instantiate(enemy, enemySpawns[spawnNumber].transform.position, enemySpawns[spawnNumber].transform.rotation);

            yield return new WaitForSeconds(0.1f);
        }

        yield return null;
    }

    void SpawnEnemy()
    {
        // if we are spawning invincible enemies - set that up here
        if (m_SpawnInvincible)
        {
            foreach (GameObject enemy in m_EnemiesLeft)
            {
                enemy.GetComponent<CharacterStats>().SetInvincible(true);
            }
        }

        // find a random spawn
        GameObject[] enemySpawns = GameObject.FindGameObjectsWithTag("EnemySpawn");
        int spawnNumber = Random.Range(0, enemySpawns.Length);

        // spawn in our enemy
        Instantiate(m_EnemiesLeft[m_EnemiesLeft.Count - 1], enemySpawns[spawnNumber].transform.position, enemySpawns[spawnNumber].transform.rotation);

        PointsInScene += m_EnemiesLeft[m_EnemiesLeft.Count - 1].GetComponent<CharacterStats>().ScorePointValue;

        m_EnemiesLeft.RemoveAt(m_EnemiesLeft.Count - 1);
    }

    public delegate int DesiredPointsFunction(float a);

    // pass in an array of enemy weightings as ints - see enum for order
    // pass in a list of enemylist values - default argument as numEnemies
    // need a variable time scale
    public void CreateWave(int points, int totalLengthInSeconds, DesiredPointsFunction desiredPoints, int[] enemyWeighting, List<EnemyList> guaranteedEnemies)
    {
        int totalWeight = 0;
        Dictionary<int, GameObject> enemyWeightValues = new Dictionary<int, GameObject>();

        GameObject[] enemyList = m_Game.GetEnemies();

        for (int i = 0; i < enemyWeighting.Length; i++)
        {
            if (enemyWeighting[i] != 0)
            {
                totalWeight += enemyWeighting[i];
                enemyWeightValues.Add(totalWeight, enemyList[i]);
            }
        }

        m_AllowedEnemies.Clear();
        Enemies.Clear();

        for (int i = 0; i < guaranteedEnemies.Count; i++)
        {
            Enemies.Add(enemyList[(int)guaranteedEnemies[i]]);
        }


        while (points > 0)
        {
            //the last enemy spawned may make the wave go slightly over it's point value

            int randVal = Random.Range(1, totalWeight + 1);

            while (!enemyWeightValues.ContainsKey(randVal))
            {
                randVal++;
            }

            Enemies.Add(enemyWeightValues[randVal]);
            points -= enemyWeightValues[randVal].GetComponent<CharacterStats>().ScorePointValue;
        }

        //randomize the order of the wave
        while (Enemies.Count > 0)
        {
            int rand = Random.Range(0, Enemies.Count - 1);
            m_EnemiesLeft.Add(Enemies[rand]);
            Enemies.RemoveAt(rand);
        }

        Enemies = new List<GameObject>(m_EnemiesLeft);

        foreach(GameObject enemy in Enemies)
        {
            m_Game.SetPointsLeft(m_Game.GetPointsLeft() + enemy.GetComponent<CharacterStats>().ScorePointValue);
        }

        m_TimeBetweenSpawn = (float)(totalLengthInSeconds) / (float)Enemies.Count;
        m_CurrentTime = m_TimeBetweenSpawn;
        m_CurrentlySpawning = true;
        m_DesiredPoints = desiredPoints;
        m_WaveTime = 0;
    }


    // Creates a wave based on the amount of points allowed, the difficulty of the enemies, and the total amount of time in seconds it takes to spawn all enemies
    public void CreateWave(int points, KeyValuePair<GameObject, int>[] enemies, int totalLengthInSeconds)
    {
        // this variable is used when finding what enemy we want to create
        int randMax = 0;

        // safety check, make sure that if we are already spawning in a wave you can't create a new one
        if (m_CurrentlySpawning)
        {
            return;
        }
        // Clear the lists of enemies we have
        m_AllowedEnemies.Clear();
        Enemies.Clear();

        for (int i = 0; i < enemies.Length; i++)
        {
            m_AllowedEnemies.Add(enemies[i].Key);
            randMax += enemies[i].Value;
        }

        // Find the enemy in this wave with the least amount of points
        int minPoints = m_AllowedEnemies[0].GetComponent<CharacterStats>().ScorePointValue;
        for (int i = 1; i < m_AllowedEnemies.Count; i++)
        {
            int enemyPoints = m_AllowedEnemies[0].GetComponent<CharacterStats>().ScorePointValue;
            if (enemyPoints < minPoints)
            {
                minPoints = enemyPoints;
            }
        }

        // if we still have points left we will still spawn enemies
        while (points >= minPoints)
        {
            // find the allowed enemy we will spawn based on the enemy weightings
            int enemy = Random.Range(0, randMax);
            int weighting = 0;
            for (int i = 0; i < enemies.Length; i++)
            {
                weighting += enemies[i].Value;
                if (enemy < weighting)
                {
                    enemy = i;
                    break;
                }
            }

            // make sure we have enough points left to spawn the enemy we want to spawn
            // if not find a new enemy
            while (m_AllowedEnemies[enemy].GetComponent<CharacterStats>().ScorePointValue > points)
            {
                enemy = Random.Range(0, m_AllowedEnemies.Count);
            }

            // add the enemy to the list of enemies
            Enemies.Add(m_AllowedEnemies[enemy]);

            // subtract the enemies point value from the total points allowed
            points -= m_AllowedEnemies[enemy].GetComponent<CharacterStats>().ScorePointValue;
        }

        // EnemiesLeft is used when actually spawning the wave
        m_EnemiesLeft = new List<GameObject>(Enemies);

        // find the time between enemy spawns
        m_TimeBetweenSpawn = (float)(totalLengthInSeconds) / (float)Enemies.Count;
        m_CurrentTime = m_TimeBetweenSpawn;

        m_CurrentlySpawning = true;
    }
}


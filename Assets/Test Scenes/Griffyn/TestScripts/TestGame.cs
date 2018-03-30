using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGame : MonoBehaviour
{
    static float TIME_BETWEEN_WAVES = 5f;
    static uint MINIMUM_POINTS_LEFT = 200;

    public GameObject Player;

    GameObject Player1;

    // Timer between waves
    int pointsLeft = 0;
    float waveTime = TIME_BETWEEN_WAVES;
    bool inWave = false;

    // Use this for initialization
    void Start()
    {
        // Initialize the Command and Player Input objects
        //pInput.ActuallyStart(); // Start isn't called when you use AddComponent so we use this function to initalize pInputs variables

        // find our player spawn point
        Vector3 spawnPos = GameObject.FindGameObjectWithTag("PlayerSpawn").transform.position;
        Quaternion spawnRot = GameObject.FindGameObjectWithTag("PlayerSpawn").transform.rotation;

        // Create our test Player
        // For now he has locked rotation on x and y
        Player1 = Instantiate(Player, spawnPos, spawnRot);
        Player1.tag = "Player"; // tag as player so that enemies can find all of the players
        //pInput.AddPlayer(0, Player1, Controllers.KEYBOARD_MOUSE); // Changed for keyboard and mouse testing
    }

    // Update is called once per frame
    void Update()
    {
        if (pointsLeft < MINIMUM_POINTS_LEFT)
        {
            waveTime -= Time.deltaTime;
        }
        if (waveTime <= 0 && !inWave)
        {
            gameObject.GetComponent<TestWaveSpawner>().CreateWave(1000, 1, 20);
            waveTime = TIME_BETWEEN_WAVES;
            inWave = true;
        }
    }
}

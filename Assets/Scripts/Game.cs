using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class Game : MonoBehaviour
{
    #region Stats
    public static float[] FODDER_HEALTH = { 16, 22, 25, 33 };
    public static int[] FODDER_DAMAGE = { 7, 9, 12, 14};
    public static float[] FODDER_ATTSPD = { 3, 2.5f, 2, 1.5f };
    public static float[] FODDER_MVMNTSPD = { 4.0f, 4.5f, 4.5f, 5.0f };
    public static int FODDER_POINT_WORTH = 10;

    public static float[] RANGED_HEALTH = { 23, 30, 38, 45 };
    public static int[] RANGED_DAMAGE = { 17, 20, 23, 25 };
    public static float[] RANGED_ATTSPD = { 2.5f, 2.5f, 2, 1.5f };
    public static float[] RANGED_MVMNTSPD = { 3, 3.5f, 3.5f, 4 };
    public static int RANGED_POINT_WORTH = 15;

    public static float[] SUPPORT_HEALTH = { 35, 42, 50, 60 };
    public static float[] SUPPORT_HEALSPD = { 3, 4, 5, 6 };
    public static float[] SUPPORT_ATTSPD = { 3, 3, 2, 1.5f };
    public static float[] SUPPORT_MVMNTSPD = { 3, 3, 3, 3 };
    public static int SUPPORT_POINT_WORTH = 25;

    public static float[] BOSS_HEALTH = { 400, 500, 600, 700 };
    public static int[] BOSS_DAMAGE = { 0, 15, 25, 35 };
    public static float[] BOSS_ATTSPD = { 3.5f, 3, 2.5f, 2 };
    public static float[] BOSS_MVMNTSPD = { 3.5f, 3.5f, 3.5f, 4 };
    public static int BOSS_POINT_WORTH = 100;
    #endregion

    //Post game
    private Canvas m_PostGameUI;

    //Pause menu
    public Canvas PauseMenuUI;

    //Players
    public GameObject Player;

    const int MAX_PLAYER_AMOUNT = 4;

    public List<GameObject> Players;
    //PlayerInput pInput;
    public WaveSpawner Spawner;

    // Enemies
    public GameObject[] Enemies;
    public GameObject Boss;

    public GameObject[] GetEnemies() { return Enemies; }

    public bool IsPaused = false;

    //Switching Scene bool
    private bool m_HasSwitchedScene = false;

    private PlayerInput m_PlayerInput;
    private Dictionary<Controllers, GameObject> m_Players;
    [SerializeField]
    GameObject m_GameManagerPrefab;

    // Wave variables
    int pointsLeft = 0;
    int debugPoints = 0;
    float m_WaveTime = WaveSpawner.TIME_BEFORE_FIRST_WAVE;

    bool m_InWave = false;

    //int WavePoints = 100;
    private int numberOfWaves = 1;

    //Keeping track for other scripts
    int AmountOfPlayersInGame;
    public int GetAmountOfPlayersInGame() { return AmountOfPlayersInGame; }
    public GameObject GetPlayerByNumber(int playerNum) { return Players[playerNum]; }

    public int GetPointsLeft() { return pointsLeft; }
    public void SetPointsLeft(int points) { pointsLeft = points; }

    //End game functionality
    private bool[] m_PlayerDeathStatus = new bool[MAX_PLAYER_AMOUNT];
    private bool m_IsAllDown = false;
    public bool IsBossDead = false; // do a better job of fixing this later Stephen>:(
    public float DeadTimer = 5.0f; // do a better job of fixing this later Stephen>:(
    bool m_BossSpawned = false;
    private int allPlayersDownedTrack = 0;
    //True for won, false for lost
    //private bool m_GameWonOrLost;
    //public bool GameWonOrLost() { return m_GameWonOrLost; }

    public PlayerSpawnCollider PlayerSpawnCollider;

    //Have this for having the boss itself tell the game, rather than the game get from the boss
    public void SetBossDead(bool status) { status = IsBossDead; }

    public bool inCutscene = false;
    private float CutsceneTime = 35f;
    private float CutsceneTimer = 0;

    public Camera CutSceneCam;

    public GameObject CutSceneAnimator;

    public GameObject[] CamHelpers;

    public GameObject RoofPanel;

    private float m_RoofPanelFazeOutTime = 5;
    //private float m_RoofPanelFazeOutTimer = 0;

    public List<GameObject> PlayerCinematicPositions;

    public void OverrideWave(int waveNum)
    {
        m_WaveTime = 0.0f;
        m_InWave = false;
        numberOfWaves = waveNum;

        Debug.Log("This cheat is currently out of order");

        //WavePoints = 100;

        //for (int i = 0; i < waveNum; i++)
        //{
        //    WavePoints = (int)(WavePoints + (WavePoints * i * 0.25f));
        //}
    }

    // Use this for initialization
    void Start()
    {
        //if (GameObject.FindGameObjectWithTag("GameManager") == null)
        //{
        //    Instantiate(m_GameManagerPrefab);

        //    GameManager.playerManager.CreateGenericCharacter(Controllers.KEYBOARD_MOUSE);
        //}
        //GameManager.playerManager.CreatePlayers();

        m_Players = GameManager.playerManager.Players;

        // Change difficulty based on number of players
        int numPlayers = m_Players.Count - 1; // for the sake of indexing an array we will use player count - 1
        SetPlayerDifficulty(FODDER_HEALTH[numPlayers], FODDER_DAMAGE[numPlayers], FODDER_ATTSPD[numPlayers], FODDER_MVMNTSPD[numPlayers],
                            RANGED_HEALTH[numPlayers], RANGED_DAMAGE[numPlayers], RANGED_ATTSPD[numPlayers], RANGED_MVMNTSPD[numPlayers],
                            SUPPORT_HEALTH[numPlayers], SUPPORT_HEALSPD[numPlayers], SUPPORT_ATTSPD[numPlayers], SUPPORT_MVMNTSPD[numPlayers],
                            BOSS_HEALTH[numPlayers], BOSS_DAMAGE[numPlayers], BOSS_ATTSPD[numPlayers], BOSS_MVMNTSPD[numPlayers]);

        GameManager.cameraManager.SetupCameras(GameManager.playerManager.PlayerList());


        GameManager.playerManager.SetPlayerData();
        GameManager.playerManager.EnablePlayers();

        m_PlayerInput = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().playerInput;

        for (int i = 0; i < m_Players.Count; i++)
        {
            //Set the players stats cause I have no idea where to do this or if we have a better idea
            GameObject playerRef = GameManager.playerManager.PlayerList()[i];
            GameObject SwordAndShield = playerRef.transform.Find("SwordAndShield").gameObject;
            GameObject Bow = playerRef.transform.Find("Bow").gameObject;

            //Change stats
            if (SwordAndShield.activeInHierarchy)
            {
                playerRef.GetComponent<PlayerStats>().MaxShield = 50;
                playerRef.GetComponent<PlayerStats>().DamageReduction = 0.1f;
            }

            //Change stats
            else if (Bow.activeInHierarchy)
            {
                playerRef.GetComponent<PlayerStats>().MaxShield = 25;
                //playerRef.GetComponent<PlayerStats>().MaxHealth = 100;
            }
        }

            


        PauseMenuUI.gameObject.SetActive(false);
        PauseMenuUI.enabled = true;

        // Initialize the Command and Player Input objects
        //pInput = gameObject.AddComponent<PlayerInput>();
        Spawner = gameObject.GetComponent<WaveSpawner>();


        if (GameManager.playerManager != null)
        {
            Players = GameManager.playerManager.PlayerList();
            AmountOfPlayersInGame = GameManager.playerManager.numberOfPlayers;
            //Setting vars to the players currently in the game.
            for (int i = 0; i < AmountOfPlayersInGame; i++)
            {
                m_PlayerDeathStatus[i] = false;
                Players[i].GetComponentInChildren<Animator>().SetBool("InGame", true);
            }
        }

        foreach(GameObject enemy in Enemies)
        {
            enemy.GetComponent<CharacterStats>().Game = gameObject;
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        SetUpInput();

        //Boss.SetActive(true);
        //Boss.GetComponent<Animator>().SetTrigger("Spawn");

    }

    // health, damage, ability values
    private void SetPlayerDifficulty(float fodderHealth, int fodderDamage, float fodderAttSpd, float fodderMvmntSpd,
                                        float rangedHealth, int rangedDamage, float rangedAttSpd, float rangedMvmntSpd,
                                        float supportHealth, float supportHealSpd, float SupportAttSpd, float SupportMvmntSpd,
                                        float bossHealth, int bossDamage, float bossAttSpd, float BossMvmntSpd)
    {
        CharacterStats fodderStats = Enemies[0].GetComponent<CharacterStats>();
        CharacterStats rangedStats = Enemies[1].GetComponent<CharacterStats>();
        CharacterStats supportStats = Enemies[2].GetComponent<CharacterStats>();
        SupportEnemyAI supportAI = Enemies[2].GetComponent<SupportEnemyAI>();
        CharacterStats bossStats = Boss.GetComponent<CharacterStats>();

        fodderStats.Health = fodderHealth;            // regular 25
        fodderStats.Damage = fodderDamage;            // regular 5
        fodderStats.AttSpd = fodderAttSpd;            // regular 2
        fodderStats.MovementSpeed = fodderMvmntSpd;   // regular 5

        rangedStats.Health = rangedHealth;            // regular 40
        rangedStats.Damage = rangedDamage;            // regular 20
        rangedStats.AttSpd = rangedAttSpd;            // regular 2
        rangedStats.MovementSpeed = rangedMvmntSpd;   // regular 3.5

        supportStats.Health = supportHealth;          // regular 50
        supportAI.HealSpeed = supportHealSpd;         // regular 5
        supportStats.AttSpd = SupportAttSpd;          // regular 2
        supportStats.MovementSpeed = SupportMvmntSpd; // regular 3.5

        bossStats.Health = bossHealth;                // regular 600
        bossStats.Damage = bossDamage;                // regular 20 // this is a number added to the damage of his abilities - basic melee does 40 base, ground shock does 20 base and energy mine does 10 base
        bossStats.AttSpd = bossAttSpd;                // regular 3
        bossStats.MovementSpeed = BossMvmntSpd;       // regular 3
    }

    public void SetUpInput()
    {
        m_PlayerInput.HandleAButton = Command.Jump;
        m_PlayerInput.HandleXButton = Command.UseAbility1;
        m_PlayerInput.HandleYButton = Command.UseAbility2;
        m_PlayerInput.HandleBButton = Command.UseAbility3;
        m_PlayerInput.HandleLeftBumper = Command.UseRevive;
        m_PlayerInput.HandleRightBumper = Command.UseRevive;
        m_PlayerInput.HandleStart = Command.HandlePause;
        m_PlayerInput.HandleLeftTrigger = Command.UseAbility4;
        m_PlayerInput.HandleRightTrigger = Command.UseAttackAbility;
        m_PlayerInput.HandleLeftStick = Command.Move;
        m_PlayerInput.HandleRightStick = Command.RotateController;
        m_PlayerInput.HandleLeftMouseClick = Command.UseAttackAbility;
        m_PlayerInput.HandleRightMouseClick = Command.UseAbility4;
        m_PlayerInput.HandleMouseMove = Command.RotateMouse;
    }

    public void SetUpCutSceneInput()
    {
        m_PlayerInput.HandleAButton = Command.SkipCutScene;
    }

    public void RemoveCutSceneInput()
    {
        m_PlayerInput.HandleAButton = null;
    }

    public void RemoveInput()
    {
        m_PlayerInput.HandleAButton = null;
        m_PlayerInput.HandleXButton = null;
        m_PlayerInput.HandleYButton = null;
        m_PlayerInput.HandleBButton = null;
        m_PlayerInput.HandleLeftBumper = null;
        m_PlayerInput.HandleRightBumper = null;
        m_PlayerInput.HandleStart = null;
        m_PlayerInput.HandleLeftTrigger = null;
        m_PlayerInput.HandleRightTrigger = null;
        m_PlayerInput.HandleLeftStick = null;
        m_PlayerInput.HandleRightStick = null;
        m_PlayerInput.HandleLeftMouseClick = null;
        m_PlayerInput.HandleRightMouseClick = null;
        m_PlayerInput.HandleMouseMove = null;
    }

    // Update is called once per frame
    void Update()
    {

        if (PauseMenuUI.gameObject.activeSelf == true)
        {
            Time.timeScale = 0;
            IsPaused = true;
        }
        else
        {
            IsPaused = false;
        }

        if (PlayerSpawnCollider.AllPlayersOut)
        {
            //Reset the character status
            for (int i = 0; i < AmountOfPlayersInGame; i++)
            {

                if (Players[i].GetComponent<CharacterStats>().isDowned != m_PlayerDeathStatus[i])
                {
                    m_PlayerDeathStatus[i] = Players[i].GetComponent<CharacterStats>().isDowned;
                    //allPlayersDownedTrack++;

                    if (m_PlayerDeathStatus[i] == true)
                    {
                        //Debug.Log("Inside Changing Death Status");

                        allPlayersDownedTrack++;
                    }

                    else if (m_PlayerDeathStatus[i] == false)
                    {
                        //Debug.Log("Inside Changing Status to not Downed");
                        allPlayersDownedTrack--;
                    }
                }

                else if (allPlayersDownedTrack == AmountOfPlayersInGame)
                {
                    //Debug.Log("Inside tracking if all players == amount of players");

                    if (!m_IsAllDown)
                    {
                        m_IsAllDown = true;
                        DeadTimer += Time.time;
                    }

                }
            }

            if (m_IsAllDown == true || IsBossDead == true)
            {
                //Debug.Log("Everybody is down");
                if (DeadTimer <= Time.time)
                {
                    DeadTimer = 5.0f;

                    //Disable the players
                    for (int i = 0; i < Players.Count; i++)
                    {
                        if (Players[i] != null)
                        {
                            Players[i].SetActive(false);
                        }
                    }

                    //Check if players won or lost 
                    if (m_IsAllDown)
                    {
                        Players[0].GetComponent<PlayerStats>().WinOrLoseGame = false;
                    }

                    if (IsBossDead)
                    {
                        Players[0].GetComponent<PlayerStats>().WinOrLoseGame = true;
                    }

                    //GameManager.musicManager.PlayMusic(GameManager.musicManager.MusicTracks[3], true, false);

                    if (UnityEngine.SceneManagement.SceneManager.GetActiveScene() == UnityEngine.SceneManagement.SceneManager.GetSceneByName("Main_Scene") && m_HasSwitchedScene == false)
                    {
                        //DeDebug.Log("Switching Scenes");
                        GameManager.sceneManager.SwitchScenes("PostGame_Scene");
                        m_HasSwitchedScene = true;
                    }
                }

                return;
            }

            if (!inCutscene)
            {
                if (pointsLeft < WaveSpawner.MINIMUM_POINTS_LEFT)
                {
                    m_WaveTime -= Time.deltaTime;
                    m_InWave = false;
                }
                if (m_WaveTime <= 0 && !m_InWave && numberOfWaves <= 5)
                {
                    foreach (GameObject player in m_Players.Values)
                    {
                        string currentWave = numberOfWaves.ToString() + " / 5";
                        PlayerUI UI = player.transform.Find("Player_UI_Prefab").GetComponent<PlayerUI>();
                        UI.UpdateText(UI.CurrentWaveText, currentWave);
                    }

                    List<EnemyList> guaranteedEnemies = new List<EnemyList>();
                    int[] enemyWeights = new int[3];
                    int wavePoints = 0;
                    int totalLengthInSeconds = 0;
                    WaveSpawner.DesiredPointsFunction desiredPoints = x => 0;
                    if (numberOfWaves == 1)
                    {
                        enemyWeights[(int)EnemyList.FodderEnemy] = 5;
                        enemyWeights[(int)EnemyList.RockThrowEnemy] = 0;
                        enemyWeights[(int)EnemyList.SupportEnemy] = 0;

                        guaranteedEnemies.Add(EnemyList.RockThrowEnemy);
                        guaranteedEnemies.Add(EnemyList.RockThrowEnemy);

                        wavePoints = 165;

                        totalLengthInSeconds = 30;

                        desiredPoints = x => 70;
                    }
                    else if (numberOfWaves == 2)
                    {
                        enemyWeights[(int)EnemyList.FodderEnemy] = 4;
                        enemyWeights[(int)EnemyList.RockThrowEnemy] = 3;
                        enemyWeights[(int)EnemyList.SupportEnemy] = 0;

                        wavePoints = 220;

                        totalLengthInSeconds = 40;

                        desiredPoints = x => 70 + (int)x;
                    }
                    else if (numberOfWaves == 3)
                    {
                        enemyWeights[(int)EnemyList.FodderEnemy] = 5;
                        enemyWeights[(int)EnemyList.RockThrowEnemy] = 3;
                        enemyWeights[(int)EnemyList.SupportEnemy] = 0;

                        guaranteedEnemies.Add(EnemyList.SupportEnemy);
                        guaranteedEnemies.Add(EnemyList.SupportEnemy);

                        wavePoints = 270;

                        totalLengthInSeconds = 40;

                        desiredPoints = x => (int)(80 + 50 * Mathf.Sin(x / 6.0f - 2.5f * Mathf.PI));
                    }
                    else if (numberOfWaves == 4)
                    {
                        enemyWeights[(int)EnemyList.FodderEnemy] = 2;
                        enemyWeights[(int)EnemyList.RockThrowEnemy] = 5;
                        enemyWeights[(int)EnemyList.SupportEnemy] = 2;

                        wavePoints = 300;

                        totalLengthInSeconds = 45;

                        desiredPoints = x => 125;
                    }
                    else if (numberOfWaves == 5)
                    {
                        enemyWeights[(int)EnemyList.FodderEnemy] = 4;
                        enemyWeights[(int)EnemyList.RockThrowEnemy] = 3;
                        enemyWeights[(int)EnemyList.SupportEnemy] = 0;

                        guaranteedEnemies.Add(EnemyList.SupportEnemy);
                        guaranteedEnemies.Add(EnemyList.SupportEnemy);
                        guaranteedEnemies.Add(EnemyList.SupportEnemy);
                        guaranteedEnemies.Add(EnemyList.SupportEnemy);
                        guaranteedEnemies.Add(EnemyList.SupportEnemy);
                        guaranteedEnemies.Add(EnemyList.SupportEnemy);

                        wavePoints = 350;

                        totalLengthInSeconds = 45;

                        desiredPoints = x => 50 + 3 * (int)x;
                    }

                    Spawner.CreateWave(wavePoints, totalLengthInSeconds, desiredPoints, enemyWeights, guaranteedEnemies);
                    //Spawner.CreateWave(WavePoints, enemies, 20);
                    m_WaveTime = WaveSpawner.TIME_BETWEEN_WAVES;
                    m_InWave = true;

                    // Set how many points are left in this wave
                    for (int i = 0; i < Spawner.Enemies.Count; i++)
                    {
                        pointsLeft += (int)Spawner.Enemies[i].GetComponent<CharacterStats>().ScorePointValue;
                    }
                    numberOfWaves++;
                }
                if (numberOfWaves > 5 && !m_InWave && m_BossSpawned == false && pointsLeft <= 0)
                {
                    CharacterStats bossStats = Boss.GetComponent<CharacterStats>();
                    pointsLeft += bossStats.ScorePointValue;
                    bossStats.Game = gameObject;
                    Spawner.SpawnBoss(Boss);
                    m_InWave = true;
                    //GameManager.audioManager.PlayMusic(AudioManager.Sounds.MUSIC_BOSS, 1);
                    m_BossSpawned = true;
                }
            }
            // how the game will update in the cutscene
            else
            {

                if (Time.time + CutsceneTime - CutsceneTimer > 5f)
                {
                    Color roofPanelColor = RoofPanel.GetComponent<Renderer>().material.color;
                    if (roofPanelColor.a > 0)
                    {
                        roofPanelColor.a -= Time.deltaTime / m_RoofPanelFazeOutTime;
                        RoofPanel.GetComponent<Renderer>().material.color = roofPanelColor;
                    }
                    else
                    {
                        RoofPanel.SetActive(false);
                    }
                }

                if (Time.time + CutsceneTime - CutsceneTimer <= 7f)
                {
                    CutSceneCam.transform.position = CamHelpers[0].transform.position;
                    CutSceneCam.transform.rotation = CamHelpers[0].transform.rotation;
                }
                else if (Time.time + CutsceneTime - CutsceneTimer <= 23) 
                {
                    CutSceneCam.transform.position = CamHelpers[1].transform.position;
                    CutSceneCam.transform.rotation = CamHelpers[1].transform.rotation;
                }
                else
                {
                    CutSceneCam.transform.position = CamHelpers[2].transform.position;
                    CutSceneCam.transform.rotation = CamHelpers[2].transform.rotation;
                }

                // end the cutscene if my timer is done
                if (CutsceneTimer <= Time.time)
                {
                    GameObject.FindGameObjectWithTag("Boss").GetComponent<Boss1AI>().SwitchToInGame();

                    cutsceneAudio.Stop();

                    GameManager.audioManager.PlayMusic(AudioManager.Sounds.MUSIC_BOSS, 1);

                    RemoveCutSceneInput();
                    SetUpInput();
                    inCutscene = false;
                    CutSceneCam.gameObject.SetActive(false);

                    foreach (GameObject player in Players)
                    {
                        player.transform.Find("Camera").GetComponent<Camera>().enabled = true;
                        player.transform.Find("Player_UI_Prefab").GetComponent<PlayerUI>().enabled = true;
                        player.GetComponent<NavMeshAgent>().enabled = true;
                    }
                }
            }
            if (debugPoints != pointsLeft)
            {
                //Debug.Log(pointsLeft);
                debugPoints = pointsLeft;
            }
        }
    }

    public void SwitchToInGame()
    {
        CutsceneTimer = 0;
    }

    AudioSource cutsceneAudio;

    public void StartCutscene()
    {
        // if there are enemies when I start the cutscene kill them all as they will cause some problems
        foreach(GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Command.KillEnemy(enemy);
        }


        cutsceneAudio = GameManager.audioManager.PlaySoundAtPositionAndMaybeStopLater(gameObject, AudioManager.Sounds.CUTSCENE_AUDIO, transform, new Vector3(0,1,1));

        //GameManager.audioManager.PlaySound(AudioManager.Sounds.CUTSCENE_AUDIO);
        GameManager.audioManager.StopCurrentMusic();

        foreach (GameObject player in Players)
        {
            PlayerUI UI = player.transform.Find("Player_UI_Prefab").GetComponent<PlayerUI>();
            UI.CurrentWaveText.SetActive(false);
            UI.BossHealth.SetActive(true);
            UI.enabled = false;
            player.transform.Find("Camera").GetComponent<Camera>().enabled = false;
            player.GetComponent<NavMeshAgent>().enabled = false;
        }

        foreach (Controllers controller in GameManager.playerManager.Players.Keys)
        {
            switch (GameManager.playerManager.PlayerNumbers[controller])
            {
                case global::Players.PLAYER_ONE:
                    GameManager.playerManager.Players[controller].transform.rotation = PlayerCinematicPositions[0].transform.rotation;
                    GameManager.playerManager.Players[controller].transform.position = PlayerCinematicPositions[0].transform.position;
                    break;
                case global::Players.PLAYER_TWO:
                    GameManager.playerManager.Players[controller].transform.rotation = PlayerCinematicPositions[1].transform.rotation;
                    GameManager.playerManager.Players[controller].transform.position = PlayerCinematicPositions[1].transform.position;
                    break;
                case global::Players.PLAYER_THREE:
                    GameManager.playerManager.Players[controller].transform.rotation = PlayerCinematicPositions[2].transform.rotation;
                    GameManager.playerManager.Players[controller].transform.position = PlayerCinematicPositions[2].transform.position;
                    break;
                case global::Players.PLAYER_FOUR:
                    GameManager.playerManager.Players[controller].transform.rotation = PlayerCinematicPositions[3].transform.rotation;
                    GameManager.playerManager.Players[controller].transform.position = PlayerCinematicPositions[3].transform.position;
                    break;
                default:
                    break;
            }

        }

        inCutscene = true;
        RemoveInput();
        SetUpCutSceneInput();
        CutsceneTimer = Time.time + CutsceneTime;

        CutSceneCam.gameObject.SetActive(true);

        CutSceneAnimator.GetComponent<Animator>().SetTrigger("StartCutscene");
    }
}

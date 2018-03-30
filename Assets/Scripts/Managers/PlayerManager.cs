
using System.Collections.Generic;
using UnityEngine;

#region Public Enums
public enum Controllers
{
    GAMEPAD_1,
    GAMEPAD_2,
    GAMEPAD_3,
    GAMEPAD_4,
    KEYBOARD_MOUSE,
}

public enum Players
{
    PLAYER_ONE,
    PLAYER_TWO,
    PLAYER_THREE,
    PLAYER_FOUR
}
#endregion

public class PlayerManager : MonoBehaviour
{
    [SerializeField]
    Vector3[] m_StartingPoints;
    [SerializeField]
    GameObject PlayerPrefab;

    public int numberOfPlayers { get { return Players.Count; } }

    public Dictionary<Controllers, GameObject> Players { get; private set; }
    public Dictionary<Controllers, Players> PlayerNumbers { get; private set; }

    public Dictionary<Controllers, Vector2> InvertedAxis { get; private set; }
    public Dictionary<Controllers, float> SensitivityMultiplier { get; private set; }

    public Material[] PlayerMaterials;

    private void Awake()
    {
        DontDestroyOnLoad(this);

        Players = new Dictionary<Controllers, GameObject>();
        PlayerNumbers = new Dictionary<Controllers, Players>();
        InvertedAxis = new Dictionary<Controllers, Vector2>();
        SensitivityMultiplier = new Dictionary<Controllers, float>();
    }

    public List<GameObject> PlayerList()
    {
        List<GameObject> players = new List<GameObject>();

        foreach (GameObject obj in Players.Values)
        {
            players.Add(obj);
        }

        return players;
    }

    public void SetInvertedAxis(Controllers controller, Vector2 inverted)
    {
        InvertedAxis[controller] = inverted;
    }


    public void SetSensitivityMultiplier(Controllers controller, float sensitivityMultiplier)
    {
        SensitivityMultiplier[controller] = sensitivityMultiplier;
    }

    public List<Controllers> ControllerList()
    {
        List<Controllers> controllers = new List<Controllers>();

        foreach (Controllers obj in Players.Keys)
        {
            controllers.Add(obj);
        }

        return controllers;
    }

    public Controllers GetController(int controllerIndex)
    {
        return ControllerList()[controllerIndex];
    }

    public void RemovedPlayer(Controllers controller)
    {
        Players.Remove(controller);

        Players playerNumber = PlayerNumbers[controller];

        PlayerNumbers.Remove(controller);

        //foreach (Controllers key in PlayerNumbers.Keys)
        //{
        //    if (PlayerNumbers[key] > playerNumber)
        //        PlayerNumbers[key]--;
        //}
    }

    public void SetPlayerData()
    {
        List<GameObject> players = PlayerList();

        int i = 1;

        foreach (GameObject player in players)
        {
            player.transform.position = m_StartingPoints[i - 1];

            player.GetComponent<PlayerSettings>().controller = GetController(i - 1);
        }
    }

    public void GetPlayerControllerDebug()
    {
        List<GameObject> players = PlayerList();

        foreach (GameObject pl in players)
        {
            Debug.Log(pl.GetComponent<PlayerSettings>().controller.ToString());
        }
    }

    public void EnablePlayers()
    {
        if (Players.Count <= 0)
        {
            CreateGenericCharacter(Controllers.KEYBOARD_MOUSE);

            GameManager.cameraManager.SetupCameras(PlayerList());

            EnablePlayers();
        }

        int i = 1;
        foreach (KeyValuePair<Controllers, GameObject> player in Players)
        {
            player.Value.SetActive(true);

            string playerSpawn = "PlayerSpawn" + i;

            player.Value.transform.position = GameObject.Find(playerSpawn).transform.position;

            if (player.Value.GetComponent<PlayerStats>().Abilities[0].AbilityName == "Sword and Shield")
            {
                player.Value.transform.Find("SwordAndShield").gameObject.SetActive(true);
            }
            else
            {
                player.Value.transform.Find("Bow").gameObject.SetActive(true);
            }

           // player.Value.transform.Find("Mesh_Player").GetComponent<Renderer>().material = PlayerMaterials[i - 1];
           // player.Value.GetComponent<PlayerStats>().PlayerMaterial = PlayerMaterials[i - 1];

            i++;
        }
    }

    public void DisablePlayers()
    {
        foreach (KeyValuePair<Controllers, GameObject> player in Players)
        {
            player.Value.SetActive(false);
        }
    }

    public void CreateGenericCharacter(Controllers controller)
    {
        GameObject player = Instantiate(PlayerPrefab);
        player.name = "Player" + (numberOfPlayers + 1).ToString();
        player.SetActive(false);
        player.GetComponent<PlayerSettings>().controller = controller;

        // setup generic character abilities
        {
            PlayerStats playerStats = player.GetComponent<PlayerStats>();

            //playerStats.Abilities[0] = new BasicMeleeAbility(playerStats);
            playerStats.Abilities[0] = new BasicRangedAbility(playerStats);
            playerStats.Abilities[4] = new BasicKnockbackAbility(playerStats);
            playerStats.Abilities[1] = new WeaponSmashAbility(playerStats);
            playerStats.Abilities[2] = new GroundShockAbility(playerStats);
            playerStats.Abilities[3] = new EnergyBombAbility(playerStats);

            playerStats.Revive = new ReviveAbility(playerStats);
        }
        //do this before we add a player
        int numPlayers = numberOfPlayers;

        Players.Add(controller, player);
        PlayerNumbers.Add(controller, (Players)numPlayers);
        InvertedAxis.Add(controller, Vector2.one);
        SensitivityMultiplier.Add(controller, 1);
    }

    /// <summary>
    /// They will all have the same controller. 
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="numberOfPlayers"></param>
    public void CreateGenericCharacter(Controllers controller, int aNumberOfPlayers)
    {
        for (int i = 0; i < aNumberOfPlayers; i++)
        {
            GameObject player = Instantiate(PlayerPrefab);
            player.name = "Player" + (i + 1).ToString();
            player.SetActive(false);
            player.GetComponent<PlayerSettings>().controller = controller;

            //do this before we add a player
            int numPlayers = numberOfPlayers;

            Players.Add(controller, player);
            PlayerNumbers.Add(controller, (Players)numPlayers);
            InvertedAxis.Add(controller, Vector2.one);
            SensitivityMultiplier.Add(controller, 1);
        }
    }


    //public void LoadPlayerLoadouts(List<PlayerLoadout> aPlayerLoadout)
    //{
    //    m_PlayerLoadouts = aPlayerLoadout;
    //}

    //public List<Controllers> GetPlayerControllers()
    //{
    //    List<Controllers> controllers = new List<Controllers>();

    //    foreach (PlayerLoadout settings in m_PlayerLoadouts)
    //    {
    //        controllers.Add(settings.controller);
    //    }

    //    return controllers;
    //}

    //public List<PlayerLoadout> GetPlayerLoadouts()
    //{
    //    return m_PlayerLoadouts;
    //}

    //public void CreatePlayers()
    //{
    //    if (m_PlayersCreated)
    //        return;

    //    m_PlayersCreated = true;

    //    int i = 1;

    //    foreach (PlayerLoadout pl in m_PlayerLoadouts)
    //    {
    //        Debug.Log(pl.ToString() + " " + pl.controller.ToString());

    //        m_PlayerReferences.Add(CreatePlayer("Player" + i.ToString(), "Player", pl.controller, i--));
    //        m_PlayerStats.Add(m_PlayerReferences[m_PlayerReferences.Count - 1].GetComponent<CharacterStats>());

    //        i++;
    //    }
    //}

    //public bool SetupCameras()
    //{
    //    if (m_PlayerReferences.Count == 0)
    //        return false;

    //    GameManager.cameraManager.SetupCameras(m_PlayerReferences);

    //    return true;
    //}

    //public GameObject CreatePlayer(string aName, string aTag, Controllers aController, int PlayerNumber)
    //{
    //    GameObject tempPlayer = Instantiate<GameObject>(m_PlayerPrefab, m_StartingPoints[PlayerNumber], Quaternion.identity);
    //    tempPlayer.name = aName;
    //    tempPlayer.tag = aTag;
    //    tempPlayer.GetComponent<PlayerSettings>().controller = aController;

    //    return tempPlayer;
    //}
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class LoadoutBehaviour : UIBehaviour
{
    private static Color m_ChainLightningColour = new Color32(0x6c, 0xf1, 0xf6, 0xff),
                         m_ChargeColour = new Color32(0xff, 0xcc, 0x4b, 0xff),
                         m_EnergyBombColour = new Color32(0x29, 0xbc, 0xe4, 0xff),
                         m_EnrageColour = new Color32(0xff, 0x6b, 0x42, 0xff),
                         m_GroundShockColour = new Color32(0x9f, 0x46, 0x32, 0xff),
                         m_HealColour = new Color32(0x5f, 0xc3, 0x62, 0xff),
                         m_ProtectMeColour = new Color32(0x41, 0xf3, 0xbf, 0xff),
                         m_VortexColour = new Color32(0x98, 0x4b, 0xab, 0xff),
                         m_WeaponSmashColour = new Color32(0xf3, 0x53, 0xb3, 0xff),
                         m_EquippedAbilityColour = Color.grey;

    public enum LoadoutScene
    {
        ColourSelector,
        WeaponSelector,
        AbilitySelector1,
        AbilitySelector2,
        AbilitySelector3,
        Ready,
    }

    [ColorUsageAttribute(false, true, 0.0f, 4.0f, 0.0f, 4.0f)]
    public Color[] PlayerColours;

    [ColorUsageAttribute(false, true, 0.0f, 4.0f, 0.0f, 4.0f)]
    public Vector4 tempColour;

    public GameObject m_PlayerPrefab;

    // Need this for starting the game from this scene
    public GameObject m_GameManagerPrefab;

    public List<GameObject> JoinButtons;
    public List<GameObject> ColourPickers;
    public List<GameObject> WeaponSelects;
    public List<GameObject> PlayerLocations;
    public List<Material> Materials;

    protected PlayerManager m_PlayerManager;

    protected Dictionary<Controllers, LoadoutScene> CurrentPlayerScreen = new Dictionary<Controllers, LoadoutScene>();
    public Dictionary<GameObject, bool> LoadoutSpotTaken = new Dictionary<GameObject, bool>();
    public Dictionary<Controllers, GameObject> playerLoadoutPosition = new Dictionary<Controllers, GameObject>();
    public Dictionary<Controllers, bool> AbilityDescriptionsActive = new Dictionary<Controllers, bool>();

    protected Button m_Player1SelectedButton,
                     m_Player2SelectedButton,
                     m_Player3SelectedButton,
                     m_Player4SelectedButton;

    protected string m_Player1CurrentlySelectedAbility,
                     m_Player2CurrentlySelectedAbility,
                     m_Player3CurrentlySelectedAbility,
                     m_Player4CurrentlySelectedAbility;

    // this order should match m_Abilities
    public Sprite[] Icons;

    protected List<string> m_Abilities = new List<string>();
    protected StringReader m_Reader;
    protected TextAsset textFile;
    protected List<string> m_AbilityDescriptions = new List<string>();
    protected List<Color> m_AbilityColours = new List<Color>();

    public GameObject Game;

    public GameObject StartText;
    public GameObject AbilityDescriptionPrompt;

    //private bool m_DescriptionsActivated = false;

    protected override void OnEnable()
    {
        if (GameObject.FindGameObjectWithTag("GameManager") == null)
        {
            Instantiate(m_GameManagerPrefab);
        }

        GameManager.audioManager.PlayMusic(AudioManager.Sounds.MUSIC_LOADOUT, 1);

        GameObject.Find("EventSystem").SetActive(false); // This is here because if the event system is active we get issues in the pause menu, we need the event system for the console window, it deals with enabling it

        m_Abilities.Add("Chain Lightning");
        m_Abilities.Add("Charge");
        m_Abilities.Add("Energy Bomb");
        m_Abilities.Add("Enrage");
        m_Abilities.Add("Ground Shock");
        m_Abilities.Add("Heal");
        m_Abilities.Add("Protect Me");
        m_Abilities.Add("Vortex");
        m_Abilities.Add("Weapon Smash");

        m_AbilityColours.Add(m_ChainLightningColour);
        m_AbilityColours.Add(m_ChargeColour);
        m_AbilityColours.Add(m_EnergyBombColour);
        m_AbilityColours.Add(m_EnrageColour);
        m_AbilityColours.Add(m_GroundShockColour);
        m_AbilityColours.Add(m_HealColour);
        m_AbilityColours.Add(m_ProtectMeColour);
        m_AbilityColours.Add(m_VortexColour);
        m_AbilityColours.Add(m_WeaponSmashColour);

        for (int i = 0; i < PlayerLocations.Count; ++i)
        {
            LoadoutSpotTaken.Add(PlayerLocations[i], false);
        }
    }

    protected override void Start()
    {
        Time.timeScale = 1;
        if (m_GameManager == null)
        {
            m_GameManager = GameObject.FindGameObjectWithTag("GameManager");
            if (m_GameManager == null)
            {
                m_GameManager = Instantiate(m_GameManagerPrefab);
            }
        }
        m_PlayerInput = m_GameManager.GetComponent<GameManager>().playerInput;
        SetUpControls();
        m_PlayerManager = m_GameManager.GetComponent<PlayerManager>();
        JoyStickUpdateTimer = Time.time + JoyStickUpdateTime;
        GetAbilityDescriptions("AbilityDescriptions/ChainLightning");
        GetAbilityDescriptions("AbilityDescriptions/Charge");
        GetAbilityDescriptions("AbilityDescriptions/EnergyBomb");
        GetAbilityDescriptions("AbilityDescriptions/Enrage");
        GetAbilityDescriptions("AbilityDescriptions/GroundShock");
        GetAbilityDescriptions("AbilityDescriptions/Heal");
        GetAbilityDescriptions("AbilityDescriptions/ProtectMe");
        GetAbilityDescriptions("AbilityDescriptions/Vortex");
        GetAbilityDescriptions("AbilityDescriptions/WeaponSmash");
    }

    protected void GetAbilityDescriptions(string path)
    {
        textFile = (TextAsset)(Resources.Load(path));
        m_Reader = new StringReader(textFile.text);
        m_AbilityDescriptions.Add(m_Reader.ReadToEnd());
    }

    protected override void Update()
    {
        foreach (Controllers controller in CurrentPlayerScreen.Keys)
        {
            if (CurrentPlayerScreen[controller] == LoadoutScene.ColourSelector)
            {
                Button colourSwatch = GetPlayerSelectedButtonFromController(controller);
                if (colourSwatch.GetComponent<ColourSwatchButtonNode>() == null)
                {
                    return;
                }
                colourSwatch.image.sprite = colourSwatch.gameObject.GetComponent<ColourSwatchButtonNode>().m_SelectedButtonImage;
            }

        }

        int numReadyPlayers = 0;

        //AbilityDescriptionPrompt.SetActive(false);
        //foreach (Controllers controller in CurrentPlayerScreen.Keys)
        //{
        //    if (CurrentPlayerScreen[controller] == LoadoutScene.Ready)
        //    {
        //        numReadyPlayers++;
        //    }
        //    if (CurrentPlayerScreen[controller] == LoadoutScene.AbilitySelector1 ||
        //        CurrentPlayerScreen[controller] == LoadoutScene.AbilitySelector2 ||
        //        CurrentPlayerScreen[controller] == LoadoutScene.AbilitySelector3)
        //    {
        //        AbilityDescriptionPrompt.SetActive(true);
        //    }
        //}

        if (numReadyPlayers == m_PlayerManager.Players.Count && numReadyPlayers != 0)
        {
            StartText.SetActive(true);
        }
        else
        {
            StartText.SetActive(false);
        }

    }

    public override void SetUpControls()
    {
        //This stops the error from showing, so I guess we need it at every
        //SetUpControls()...
        if (m_PlayerInput == null)
        {
            return;
        }

        m_PlayerInput.HandleDPadLeft = MoveToPreviousButton;
        m_PlayerInput.HandleDPadRight = MoveToNextButton;
        m_PlayerInput.HandleLeftStick = MoveToButtonStick;
        m_PlayerInput.HandleBButton = GoBack;
        m_PlayerInput.HandleAButton = OnClick;
        m_PlayerInput.HandleBack = ToggleDescriptions;
    }

    public override void RemoveControls()
    {
        m_PlayerInput.HandleDPadLeft = null;
        m_PlayerInput.HandleDPadRight = null;
        m_PlayerInput.HandleLeftStick = null;
        m_PlayerInput.HandleBButton = null;
        m_PlayerInput.HandleAButton = null;
        m_PlayerInput.HandleBack = null;
    }


    #region Button Controls
    protected void GoBack(Controllers controller)
    {
        if (!CurrentPlayerScreen.ContainsKey(controller))
        {
            GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_BACK);
            GameManager.sceneManager.ResetGame();
        }

        CharacterStats stats = m_PlayerManager.Players[controller].GetComponent<CharacterStats>();
        int playerNumber = (int)(m_PlayerManager.PlayerNumbers[controller]);
        if (CurrentPlayerScreen[controller] == LoadoutScene.Ready)
        {
            GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_BACK);
            stats.Abilities[3] = null;
            JoinButtons[playerNumber].SetActive(true);
            WeaponSelects[playerNumber].transform.Find("RightButton").gameObject.SetActive(true);
            WeaponSelects[playerNumber].transform.Find("LeftButton").gameObject.SetActive(true);
            WeaponSelects[playerNumber].transform.Find("Icons").gameObject.SetActive(true);
            WeaponSelects[playerNumber].transform.Find("BButton").transform.Find("Icon").gameObject.SetActive(false);
            ChangeIcon(controller, GetCurrentSelectedAbilityFromController(controller));

            SwitchToAbilitySelector(controller, LoadoutScene.AbilitySelector3);
            NextAbility(controller);
            NextAbility(controller);

            WeaponSelects[playerNumber].transform.Find("BButton").GetComponent<Image>().enabled = true;


        }
        else if (CurrentPlayerScreen[controller] == LoadoutScene.AbilitySelector3)
        {
            GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_BACK);
            stats.Abilities[2] = null;

            WeaponSelects[playerNumber].transform.Find("YButton").transform.Find("Icon").gameObject.SetActive(false);
            ChangeIcon(controller, GetCurrentSelectedAbilityFromController(controller));

            SwitchToAbilitySelector(controller, LoadoutScene.AbilitySelector2);
            StopCurrentAnimation(m_PlayerManager.Players[controller].GetComponentInChildren<Animator>());
            NextAbility(controller);
            NextAbility(controller);

            WeaponSelects[playerNumber].transform.Find("YButton").GetComponent<Image>().enabled = true;

        }
        else if (CurrentPlayerScreen[controller] == LoadoutScene.AbilitySelector2)
        {
            GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_BACK);
            stats.Abilities[1] = null;

            WeaponSelects[playerNumber].transform.Find("XButton").transform.Find("Icon").gameObject.SetActive(false);
            ChangeIcon(controller, GetCurrentSelectedAbilityFromController(controller));

            SwitchToAbilitySelector(controller, LoadoutScene.AbilitySelector1);
            StopCurrentAnimation(m_PlayerManager.Players[controller].GetComponentInChildren<Animator>());
            NextAbility(controller);
            NextAbility(controller);

            WeaponSelects[playerNumber].transform.Find("XButton").GetComponent<Image>().enabled = true;

        }
        else if (CurrentPlayerScreen[controller] == LoadoutScene.AbilitySelector1)
        {
            if (AbilityDescriptionsActive[controller])
            {
                ToggleDescriptions(controller);
            }

            JoinButtons[playerNumber].GetComponent<Text>().text = "Press A";
            JoinButtons[playerNumber].SetActive(false);

            GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_BACK);
            StopCurrentAnimation(m_PlayerManager.Players[controller].GetComponentInChildren<Animator>());
            CurrentPlayerScreen[controller] = LoadoutScene.WeaponSelector;
            Button button = WeaponSelects[playerNumber].GetComponentInChildren<Button>();
            SetPlayerSelectedButtonFromController(controller, button);
            WeaponSelects[playerNumber].transform.Find("Text").GetComponent<Text>().text = stats.Abilities[0].AbilityName;
            WeaponSelects[playerNumber].transform.Find("Icons").gameObject.SetActive(false);
            WeaponSelects[playerNumber].transform.Find("XButton").gameObject.SetActive(false);
            WeaponSelects[playerNumber].transform.Find("YButton").gameObject.SetActive(false);
            WeaponSelects[playerNumber].transform.Find("BButton").gameObject.SetActive(false);

            if (stats.Abilities[0].AbilityName == "Bow")
            {
                m_PlayerManager.Players[controller].transform.Find("Bow").gameObject.SetActive(true);
                Animator animator = stats.gameObject.GetComponentInChildren<Animator>();
                animator.SetBool("PlayBasicBow", true);
            }
            else
            {
                m_PlayerManager.Players[controller].transform.Find("SwordAndShield").gameObject.SetActive(true);
                Animator animator = stats.gameObject.GetComponentInChildren<Animator>();
                animator.SetBool("PlayBasicSword", true);
            }
        }
        else if (CurrentPlayerScreen[controller] == LoadoutScene.WeaponSelector)
        {
            GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_BACK);
            if (stats.Abilities[0].AbilityName == "Bow")
            {
                m_PlayerManager.Players[controller].transform.Find("Bow").gameObject.SetActive(false);
            }
            else
            {
                m_PlayerManager.Players[controller].transform.Find("SwordAndShield").gameObject.SetActive(false);
            }

            stats.Abilities[0] = null;

            CurrentPlayerScreen[controller] = LoadoutScene.ColourSelector;
            EnablePlayerColourPicker(playerNumber);

            Vector4 playerColour = m_PlayerManager.Players[controller].transform.Find("Mesh_Player").transform.Find("Body").GetComponent<Renderer>().material.GetColor("_EmissionColor");

            for (int i = 1; i < 9; i++)
            {
                string buttonName = "Colour" + (i);
                Vector4 colorToCheck = ColourPickers[playerNumber].transform.Find(buttonName).GetComponent<Image>().sprite.texture.GetPixel(32, 64);

                if (colorToCheck.x > colorToCheck.y && colorToCheck.x > colorToCheck.z)
                {
                    colorToCheck.x += 1;
                    colorToCheck.x = Mathf.Clamp(colorToCheck.x, 0, 1.25f);
                }
                else if (colorToCheck.y > colorToCheck.x && colorToCheck.y > colorToCheck.z)
                {
                    colorToCheck.y += 1;
                    colorToCheck.y = Mathf.Clamp(colorToCheck.y, 0, 1.25f);
                }
                else if (colorToCheck.z > colorToCheck.y && colorToCheck.z > colorToCheck.x)
                {
                    colorToCheck.z += 1;
                    colorToCheck.z = Mathf.Clamp(colorToCheck.z, 0, 1.25f);
                }

                if (colorToCheck == playerColour)
                {
                    SetPlayerSelectedButtonFromController(controller, ColourPickers[playerNumber].transform.Find(buttonName).GetComponent<Button>());
                }
            }

            WeaponSelects[playerNumber].SetActive(false);
            StopCurrentAnimation(m_PlayerManager.Players[controller].GetComponentInChildren<Animator>());
        }
        else if (CurrentPlayerScreen[controller] == LoadoutScene.ColourSelector)
        {
            GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_BACK);

            Button buttonToDeselect = GetPlayerSelectedButtonFromController(controller);
            buttonToDeselect.image.sprite = buttonToDeselect.gameObject.GetComponent<ColourSwatchButtonNode>().m_UnSelectedButtonImage;

            LoadoutSpotTaken[playerLoadoutPosition[controller]] = false;
            playerLoadoutPosition.Remove(controller);

            Destroy(m_PlayerManager.Players[controller]);
            m_PlayerManager.RemovedPlayer(controller);

            ColourPickers[playerNumber].SetActive(false);
            JoinButtons[playerNumber].SetActive(true);

            CurrentPlayerScreen.Remove(controller);
        }
    }

    protected override void OnClick(Controllers controller)
    {

        if (m_PlayerManager.Players.ContainsKey(controller))
        {
            int playerNumber = (int)(m_PlayerManager.PlayerNumbers[controller]);

            if (CurrentPlayerScreen[controller] == LoadoutScene.ColourSelector)
            {
                GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_CONFIRM);
                SwitchToWeaponSelector(controller);
                return;
            }
            else if (CurrentPlayerScreen[controller] == LoadoutScene.WeaponSelector)
            {
                GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_CONFIRM);
                SwitchToAbilitySelector(controller, LoadoutScene.AbilitySelector1);
                WeaponSelects[playerNumber].transform.Find("XButton").GetComponent<Image>().enabled = true;
                WeaponSelects[playerNumber].transform.Find("YButton").GetComponent<Image>().enabled = true;
                WeaponSelects[playerNumber].transform.Find("BButton").GetComponent<Image>().enabled = true;

                return;
            }
            else if (CurrentPlayerScreen[controller] == LoadoutScene.AbilitySelector1)
            {
                GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_CONFIRM);
                GivePlayerSelectedAbility(controller, 1);

                return;
            }
            else if (CurrentPlayerScreen[controller] == LoadoutScene.AbilitySelector2)
            {
                GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_CONFIRM);
                GivePlayerSelectedAbility(controller, 2);
                //WeaponSelects[playerNumber - 1].transform.Find("YButton").GetComponent<Image>().enabled = false;

                return;
            }
            else if (CurrentPlayerScreen[controller] == LoadoutScene.AbilitySelector3)
            {
                GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_CONFIRM);
                GivePlayerSelectedAbility(controller, 3);
                //WeaponSelects[playerNumber - 1].transform.Find("BButton").GetComponent<Image>().enabled = false;

                SwitchToReadyScreen(controller);
                return;
            }

            else if (CurrentPlayerScreen[controller] == LoadoutScene.Ready)
            {
                foreach (Controllers checkController in CurrentPlayerScreen.Keys)
                {
                    if (CurrentPlayerScreen[checkController] != LoadoutScene.Ready)
                    {
                        return;
                    }
                }
                foreach (Controllers checkController in CurrentPlayerScreen.Keys)
                {
                    m_PlayerManager.Players[checkController].transform.Find("Camera").GetComponent<Camera>().enabled = true;
                    m_PlayerManager.Players[checkController].transform.Find("Player_UI_Prefab").GetComponent<PlayerUI>().enabled = true;
                    m_PlayerManager.Players[checkController].GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = true;
                    //m_PlayerManager.Players[checkController].GetComponent<UnityEngine.AI.NavMeshAgent>().isStopped = true;
                }
                transform.parent.Find("LoadoutCamera").gameObject.SetActive(false);
                GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_CONFIRM);
                Game.SetActive(true);
                gameObject.SetActive(false);
                //Play In Game Music
                GameManager.audioManager.PlayMusic(AudioManager.Sounds.MUSIC_IN_GAME, 1);

            }
        }
        else
        {
            GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_CONFIRM);
            StartText.SetActive(false);
            SetUpPlayer(controller);
            // add the player to our key value pair so we can keep track of what screen everyone is on
            CurrentPlayerScreen.Add(controller, LoadoutScene.ColourSelector);
            SwitchPlayerToColourPicker(controller);
        }
    }

    protected void ToggleDescriptions(Controllers controller)
    {
        if (CurrentPlayerScreen[controller] == LoadoutScene.AbilitySelector1 ||
            CurrentPlayerScreen[controller] == LoadoutScene.AbilitySelector2 ||
            CurrentPlayerScreen[controller] == LoadoutScene.AbilitySelector3)
        {
            int playerNumber = (int)(m_PlayerManager.PlayerNumbers[controller]);
            GameObject Description = WeaponSelects[playerNumber].transform.Find("Description").gameObject;
            Description.SetActive(!Description.activeSelf);
            AbilityDescriptionsActive[controller] = !AbilityDescriptionsActive[controller];
            Debug.Log(AbilityDescriptionsActive[controller]);
        }
    }

    public override void MoveToButtonStick(Controllers controller, Vector2 joyStick)
    {
        if (joyStick.magnitude <= Mathf.Epsilon)
        {
            JoyStickUpdateTimer = 0;
        }
        if (JoyStickUpdateTimer <= Time.time)
        {
            JoyStickUpdateTimer = Time.time + JoyStickUpdateTime;

            if (joyStick.x > 0.1f)
            {
                MoveToNextButton(controller);
            }
            else if (joyStick.x < -0.1f)
            {
                MoveToPreviousButton(controller);
            }

        }
    }

    public override void MoveToNextButton(Controllers controller)
    {
        if (!m_PlayerManager.Players.ContainsKey(controller))
        {
            return;
        }

        GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_SELECT);
        Button button = GetPlayerSelectedButtonFromController(controller);
        Button nextButton = button.GetComponent<ButtonNode>().NextButton;
        button = nextButton;
        button.Select();
        SetPlayerSelectedButtonFromController(controller, button);
        if (CurrentPlayerScreen[controller] == LoadoutScene.ColourSelector)
        {
            Button buttonToDeselect = button.GetComponent<ColourSwatchButtonNode>().PreviousButton;
            buttonToDeselect.image.sprite = buttonToDeselect.gameObject.GetComponent<ColourSwatchButtonNode>().m_UnSelectedButtonImage;

            SwitchColour(controller, true);
        }
        else if (CurrentPlayerScreen[controller] == LoadoutScene.WeaponSelector)
        {
            SwitchWeapon(controller);
        }
        else if (CurrentPlayerScreen[controller] == LoadoutScene.AbilitySelector1 ||
                 CurrentPlayerScreen[controller] == LoadoutScene.AbilitySelector2 ||
                 CurrentPlayerScreen[controller] == LoadoutScene.AbilitySelector3)
        {
            NextAbility(controller);
        }
    }

    public override void MoveToPreviousButton(Controllers controller)
    {
        if (!m_PlayerManager.Players.ContainsKey(controller))
        {
            return;
        }

        GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_SELECT);
        Button button = GetPlayerSelectedButtonFromController(controller);
        Button previousButton = button.GetComponent<ButtonNode>().PreviousButton;
        button = previousButton;
        button.Select();
        SetPlayerSelectedButtonFromController(controller, button);
        if (CurrentPlayerScreen[controller] == LoadoutScene.ColourSelector)
        {
            Button buttonToDeselect = button.GetComponent<ColourSwatchButtonNode>().NextButton;
            buttonToDeselect.image.sprite = buttonToDeselect.gameObject.GetComponent<ColourSwatchButtonNode>().m_UnSelectedButtonImage;

            SwitchColour(controller, false);
        }
        else if (CurrentPlayerScreen[controller] == LoadoutScene.WeaponSelector)
        {
            SwitchWeapon(controller);
        }
        else if (CurrentPlayerScreen[controller] == LoadoutScene.AbilitySelector1 ||
                 CurrentPlayerScreen[controller] == LoadoutScene.AbilitySelector2 ||
                 CurrentPlayerScreen[controller] == LoadoutScene.AbilitySelector3)
        {
            PreviousAbility(controller);
        }
    }

    protected Button GetPlayerSelectedButtonFromController(Controllers controller)
    {
        int playerNumber = (int)(m_PlayerManager.PlayerNumbers[controller]);
        switch (playerNumber)
        {
            case 0:
                return m_Player1SelectedButton;
            case 1:
                return m_Player2SelectedButton;
            case 2:
                return m_Player3SelectedButton;
            case 3:
                return m_Player4SelectedButton;
            default:
                return null;
        }
    }

    protected void SetPlayerSelectedButtonFromController(Controllers controller, Button button)
    {
        int playerNumber = (int)(m_PlayerManager.PlayerNumbers[controller]);
        switch (playerNumber)
        {
            case 0:
                m_Player1SelectedButton = button;
                break;
            case 1:
                m_Player2SelectedButton = button;
                break;
            case 2:
                m_Player3SelectedButton = button;
                break;
            case 3:
                m_Player4SelectedButton = button;
                break;
            default:
                break;
        }
    }

    #endregion

    protected void SetUpPlayer(Controllers controller)
    {
        GameObject player = Instantiate(m_PlayerPrefab);

        SpawnInFirstAvailableLoadoutPosition(player, controller);


        player.transform.Find("Camera").GetComponent<Camera>().enabled = false;
        player.transform.Find("Player_UI_Prefab").GetComponent<PlayerUI>().enabled = false;
        PlayerStats stats = player.GetComponent<PlayerStats>();
        stats.Abilities[4] = new BasicKnockbackAbility(player.GetComponent<CharacterStats>());
        stats.Controller = controller;
    }

    protected void SpawnInFirstAvailableLoadoutPosition(GameObject player, Controllers controller)
    {
        int i = 1;
        foreach (GameObject loadoutPosition in LoadoutSpotTaken.Keys)
        {
            if (!LoadoutSpotTaken[loadoutPosition])
            {
                player.name = "Player" + (i);

                // position the player
                player.transform.position = loadoutPosition.transform.position;

                // rotate them to face the camera
                Transform target = GameObject.Find("LoadoutCamera").transform;
                Vector3 dir = target.position - player.transform.position;
                dir.y = 0;
                Quaternion rot = player.transform.rotation;
                rot.eulerAngles.Set(-90, 0, 0);
                player.transform.rotation = rot;
                player.transform.forward = dir.normalized;

                // setup the playerManager with the correct information
                m_PlayerManager.Players.Add(controller, player);
                m_PlayerManager.PlayerNumbers.Add(controller, (Players)(i - 1));

                int playerNumber = (int)(m_PlayerManager.PlayerNumbers[controller]);
                GameObject playerMesh = player.transform.Find("Mesh_Player").transform.Find("Body").gameObject;
                playerMesh.GetComponent<Renderer>().material = Materials[playerNumber];

                if (!m_PlayerManager.InvertedAxis.ContainsKey(controller))
                {
                    m_PlayerManager.InvertedAxis.Add(controller, Vector2.one);
                    m_PlayerManager.SensitivityMultiplier.Add(controller, 1);
                }

                LoadoutSpotTaken[loadoutPosition] = true;
                playerLoadoutPosition[controller] = loadoutPosition;
                AbilityDescriptionsActive[controller] = false;

                break;
            }
            i++;
        }
    }

    #region Colour Picker
    protected void SwitchPlayerToColourPicker(Controllers controller)
    {
        CurrentPlayerScreen[controller] = LoadoutScene.ColourSelector;

        int playerNumber = (int)(m_PlayerManager.PlayerNumbers[controller]);
        EnablePlayerColourPicker(playerNumber);
        SelectFirstColour(controller, playerNumber);
    }

    protected void EnablePlayerColourPicker(int playerNumber)
    {
        JoinButtons[playerNumber].SetActive(false);
        ColourPickers[playerNumber].SetActive(true);
    }

    protected void SelectFirstColour(Controllers controller, int playerNumber)
    {
        Button tempSelectedButton = ColourPickers[playerNumber].GetComponentInChildren<Button>();

        //Color tempColour = tempSelectedButton.gameObject.GetComponent<Image>().sprite.texture.GetPixel(32, 64); // the colour swatches are 64x128 so this is the middle position
        
        tempColour = tempSelectedButton.gameObject.GetComponent<Image>().sprite.texture.GetPixel(32, 64); // the colour swatches are 64x128 so this is the middle position

        GameObject playerMesh = m_PlayerManager.Players[controller].transform.Find("Mesh_Player").transform.Find("Body").gameObject;
        playerMesh.GetComponent<Renderer>().material.EnableKeyword("_EmissionColor");
        //[ColorUsageAttribute(true, true, 0.0f, 2.0f, 0.125f, 3.0f)] public Color color;
        

        //playerMesh.GetComponent<Renderer>().material.color.

        // loop through every controller for as many times as there are players to make sure if you switch your colour it isn;t the same as someone you already checked
        int i = 0;
        for (int j = 0; j < m_PlayerManager.numberOfPlayers; j++)
        {
            foreach (Controllers controllerCheck in m_PlayerManager.Players.Keys)
            {
                if (controllerCheck == controller)
                {
                    continue;
                }

                Vector4 checkColour = tempColour;


                if (checkColour.x > checkColour.y && checkColour.x > checkColour.z)
                { 
                    checkColour.x += 1;
                    checkColour.x = Mathf.Clamp(tempColour.x, 0, 1.25f);
                }

                else if (checkColour.y > checkColour.x && checkColour.y > checkColour.z)
                {
                    checkColour.y += 1;
                    checkColour.y = Mathf.Clamp(checkColour.y, 0, 1.25f);
                }

                else if (checkColour.z > checkColour.y && checkColour.z > checkColour.x)
                {
                    checkColour.z += 1;
                    checkColour.z = Mathf.Clamp(checkColour.z, 0, 1.25f);

                }


                if (m_PlayerManager.Players[controllerCheck].transform.Find("Mesh_Player").transform.Find("Body").GetComponent<Renderer>().material.GetColor("_EmissionColor") == (Color)checkColour)
                {
                    i++;
                    string buttonName = "Colour" + (i + 1);
                    tempSelectedButton = ColourPickers[playerNumber].transform.Find(buttonName).GetComponent<Button>();
                    tempColour = tempSelectedButton.gameObject.GetComponent<Image>().sprite.texture.GetPixel(32, 64);
                }
            }
        }


        if (tempColour.x > tempColour.y && tempColour.x > tempColour.z)
        {
            tempColour.x += 1;
            tempColour.x = Mathf.Clamp(tempColour.x, 0, 1.25f);
        }

        else if (tempColour.y > tempColour.x && tempColour.y > tempColour.z)
        {
            tempColour.y += 1;
            tempColour.y = Mathf.Clamp(tempColour.y, 0, 1.25f);

        }

        else if (tempColour.z > tempColour.y && tempColour.z > tempColour.x)
        {
            tempColour.z += 1;
            tempColour.z = Mathf.Clamp(tempColour.z, 0, 1.25f);

        }

        playerMesh.GetComponent<Renderer>().material.SetColor("_EmissionColor", tempColour);
        //playerMesh.GetComponent<Renderer>().material.color = tempColour;
        PlayerStats stats = m_PlayerManager.Players[controller].GetComponent<PlayerStats>();

        stats.PlayerColour = tempColour;
        

        Button button = GetPlayerSelectedButtonFromController(controller);
        button = tempSelectedButton;
        button.Select();
        SetPlayerSelectedButtonFromController(controller, button);
    }

    [ColorUsageAttribute(false, true, 0.0f, 4.0f, 0.0f, 4.0f)]
    Vector4 colour;

    protected void SwitchColour(Controllers controller, bool movingRight)
    {
        int playerNumber = (int)(m_PlayerManager.PlayerNumbers[controller]);
        Button button = GetPlayerSelectedButtonFromController(controller);


        colour = button.gameObject.GetComponent<Image>().sprite.texture.GetPixel(32, 64); // the colour swatches are 64x128 so this is the middle position
        PlayerStats stats = m_PlayerManager.Players[controller].GetComponent<PlayerStats>();
        GameObject playerMesh = m_PlayerManager.Players[controller].transform.Find("Mesh_Player").transform.Find("Body").gameObject;

        for (int j = 0; j < m_PlayerManager.numberOfPlayers; j++)
        {
            foreach (Controllers controllerCheck in m_PlayerManager.Players.Keys)
            {
                if (controllerCheck == controller)
                {
                    continue;
                }


                Vector4 checkColour = colour;


                if (checkColour.x > checkColour.y && checkColour.x > checkColour.z)
                {
                    checkColour.x += 1;
                    checkColour.x = Mathf.Clamp(checkColour.x, 0, 1.25f);
                    //Debug.Log("Increase red");
                }

                else if (checkColour.y > checkColour.x && checkColour.y > checkColour.z)
                {
                    checkColour.y += 1;
                    checkColour.y = Mathf.Clamp(checkColour.y, 0, 1.25f);
                    //Debug.Log("Increase green");
                }

                else if (checkColour.z > checkColour.y && checkColour.z > checkColour.x)
                {
                    checkColour.z += 1;
                    checkColour.z = Mathf.Clamp(checkColour.z, 0, 1.25f);
                    //Debug.Log("Increase blue");
                }


                if (m_PlayerManager.Players[controllerCheck].transform.Find("Mesh_Player").transform.Find("Body").GetComponent<Renderer>().material.GetColor("_EmissionColor") == (Color)checkColour)
                {
                    if (movingRight)
                    {
                        MoveToNextButton(controller);
                        return;
                    }
                    else
                    {
                        MoveToPreviousButton(controller);
                        return;
                    }
                }
            }
        }

        if (colour.x > colour.y && colour.x > colour.z)
        {
            colour.x += 1;
            colour.x = Mathf.Clamp(colour.x, 0, 1.25f);
        }

        else if (colour.y > colour.x && colour.y > colour.z)
        {
            colour.y += 1;
            colour.y = Mathf.Clamp(colour.y, 0, 1.25f);

        }

        else if (colour.z > colour.y && colour.z > colour.x)
        {
            colour.z += 1;
            colour.z = Mathf.Clamp(colour.z, 0, 1.25f);

        }

        playerMesh.GetComponent<Renderer>().material.SetColor("_EmissionColor", colour);
        //playerMesh.GetComponent<Renderer>().material.color = colour;
        stats.PlayerColour = colour;
    }
    #endregion

    #region Weapon Selector
    protected void SwitchToWeaponSelector(Controllers controller)
    {
        int playerNumber = (int)(m_PlayerManager.PlayerNumbers[controller]);
        ColourPickers[playerNumber].SetActive(false);
        WeaponSelects[playerNumber].SetActive(true);

        CurrentPlayerScreen[controller] = LoadoutScene.WeaponSelector;
        Button button = WeaponSelects[playerNumber].GetComponentInChildren<Button>();
        SetPlayerSelectedButtonFromController(controller, button);

        CharacterStats stats = m_PlayerManager.Players[controller].GetComponent<CharacterStats>();
        stats.Abilities[0] = new BasicMeleeAbility(stats);
        m_PlayerManager.Players[controller].transform.Find("SwordAndShield").gameObject.SetActive(true);
        WeaponSelects[playerNumber].transform.Find("Text").GetComponent<Text>().text = stats.Abilities[0].AbilityName;

        Animator animator = stats.gameObject.transform.Find("Mesh_Player").GetComponent<Animator>();
        animator.SetBool("PlayBasicSword", true);
    }

    protected void SwitchWeapon(Controllers controller)
    {
        CharacterStats stats = m_PlayerManager.Players[controller].GetComponent<CharacterStats>();
        Animator animator = stats.gameObject.transform.Find("Mesh_Player").GetComponent<Animator>();

        StopCurrentAnimation(animator);

        if (stats.Abilities[0].AbilityName == "Sword and Shield")
        {
            stats.Abilities[0] = new BasicRangedAbility(stats);
            m_PlayerManager.Players[controller].transform.Find("Bow").gameObject.SetActive(true);
            m_PlayerManager.Players[controller].transform.Find("SwordAndShield").gameObject.SetActive(false);
            animator.SetBool("PlayBasicBow", true);
            Animator bowAnimator = stats.gameObject.transform.Find("Bow").GetComponentInChildren<Animator>();
            bowAnimator.SetBool("LoadoutFire", true);
        }
        else
        {
            stats.Abilities[0] = new BasicMeleeAbility(stats);
            m_PlayerManager.Players[controller].transform.Find("SwordAndShield").gameObject.SetActive(true);
            m_PlayerManager.Players[controller].transform.Find("Bow").gameObject.SetActive(false);
            animator.SetBool("PlayBasicSword", true);
        }

        int playerNumber = (int)(m_PlayerManager.PlayerNumbers[controller]);
        WeaponSelects[playerNumber].transform.Find("Text").GetComponent<Text>().text = stats.Abilities[0].AbilityName;
    }
    #endregion

    #region Ability Selector

    protected string GetCurrentSelectedAbilityFromController(Controllers controller)
    {
        int playerNumber = (int)(m_PlayerManager.PlayerNumbers[controller]);
        switch (playerNumber)
        {
            case 0:
                return m_Player1CurrentlySelectedAbility;
            case 1:
                return m_Player2CurrentlySelectedAbility;
            case 2:
                return m_Player3CurrentlySelectedAbility;
            case 3:
                return m_Player4CurrentlySelectedAbility;
            default:
                return null;
        }
    }

    protected void SetCurrentSelectedAbilityFromController(Controllers controller, string currentSelectedAbility)
    {
        int playerNumber = (int)(m_PlayerManager.PlayerNumbers[controller]);
        switch (playerNumber)
        {
            case 0:
                m_Player1CurrentlySelectedAbility = currentSelectedAbility;
                break;
            case 1:
                m_Player2CurrentlySelectedAbility = currentSelectedAbility;
                break;
            case 2:
                m_Player3CurrentlySelectedAbility = currentSelectedAbility;
                break;
            case 3:
                m_Player4CurrentlySelectedAbility = currentSelectedAbility;
                break;
            default:
                break;
        }
    }
    protected void SwitchToAbilitySelector(Controllers controller, LoadoutScene abilitySelectorScreen)
    {
        StopCurrentAnimation(m_PlayerManager.Players[controller].GetComponentInChildren<Animator>());
        int playerNumber = (int)(m_PlayerManager.PlayerNumbers[controller]);
        CurrentPlayerScreen[controller] = abilitySelectorScreen;

        JoinButtons[playerNumber].SetActive(true);
        JoinButtons[playerNumber].GetComponent<Text>().text = "Press 'back' for Description";

        Text abilityText = WeaponSelects[playerNumber].transform.Find("Text").GetComponent<Text>();
        abilityText.text = m_Abilities[0];
        SetCurrentSelectedAbilityFromController(controller, abilityText.text);

        WeaponSelects[playerNumber].transform.Find("Icons").gameObject.SetActive(true);
        WeaponSelects[playerNumber].transform.Find("XButton").gameObject.SetActive(true);
        WeaponSelects[playerNumber].transform.Find("YButton").gameObject.SetActive(true);
        WeaponSelects[playerNumber].transform.Find("BButton").gameObject.SetActive(true);

        PlayAnimation(controller, m_Abilities[0]);
        ChangeIconColours(controller);

        ChangeAbilityDescription(controller);
    }

    protected void NextAbility(Controllers controller)
    {
        int playerNumber = (int)(m_PlayerManager.PlayerNumbers[controller]);
        Text abilityText = WeaponSelects[playerNumber].transform.Find("Text").GetComponent<Text>();

        int i = 0;
        foreach (string ability in m_Abilities)
        {
            if (ability == abilityText.text)
            {
                if (i == m_Abilities.Count - 1)
                {
                    i = -1;
                }

                abilityText.text = m_Abilities[i + 1];
                SetCurrentSelectedAbilityFromController(controller, abilityText.text);
                break;
            }
            i++;
        }


        CharacterStats stats = m_PlayerManager.Players[controller].GetComponent<CharacterStats>();
        string currentSelectedAbility = GetCurrentSelectedAbilityFromController(controller);
        foreach (Ability ability in stats.Abilities)
        {
            if (ability != null)
            {
                string abilityName = ability.AbilityName;
                if (abilityName == currentSelectedAbility)
                {
                    SetCurrentSelectedAbilityFromController(controller, currentSelectedAbility);
                    MoveToNextButton(controller);
                    return;
                }
            }
        }

        SetCurrentSelectedAbilityFromController(controller, currentSelectedAbility);

        ChangeIcon(controller, currentSelectedAbility);

        PlayAnimation(controller, currentSelectedAbility);
        ChangeIconColours(controller);


        ChangeAbilityDescription(controller);

    }

    protected void PreviousAbility(Controllers controller)
    {
        int playerNumber = (int)(m_PlayerManager.PlayerNumbers[controller]);
        Text abilityText = WeaponSelects[playerNumber].transform.Find("Text").GetComponent<Text>();
        if (abilityText.text == "Select Your Abilities")
        {
            abilityText.text = m_Abilities[0];
            SetCurrentSelectedAbilityFromController(controller, abilityText.text);
        }
        else
        {
            int i = 0;
            foreach (string ability in m_Abilities)
            {
                if (ability == abilityText.text)
                {
                    if (i == 0)
                    {
                        i = m_Abilities.Count;
                    }

                    abilityText.text = m_Abilities[i - 1];
                    SetCurrentSelectedAbilityFromController(controller, abilityText.text);
                    break;
                }
                i++;
            }
        }

        CharacterStats stats = m_PlayerManager.Players[controller].GetComponent<CharacterStats>();
        string currentSelectedAbility = GetCurrentSelectedAbilityFromController(controller);
        foreach (Ability ability in stats.Abilities)
        {
            if (ability != null)
            {
                string abilityName = ability.AbilityName;
                if (abilityName == currentSelectedAbility)
                {
                    SetCurrentSelectedAbilityFromController(controller, currentSelectedAbility);
                    MoveToPreviousButton(controller);
                    return;
                }
            }
        }

        SetCurrentSelectedAbilityFromController(controller, currentSelectedAbility);

        ChangeIcon(controller, currentSelectedAbility);

        PlayAnimation(controller, currentSelectedAbility);
        ChangeIconColours(controller);

        ChangeAbilityDescription(controller);

    }

    protected void ChangeAbilityDescription(Controllers controller)
    {
        int playerNumber = (int)(m_PlayerManager.PlayerNumbers[controller]);
        Text abilityText = WeaponSelects[playerNumber].transform.Find("Text").GetComponent<Text>();
        int abilityNumber = 0;
        foreach (string ability in m_Abilities)
        {
            if (ability == abilityText.text)
            {
                break;
            }
            abilityNumber++;
        }

        //abilityNumber++;

        GameObject Description = WeaponSelects[playerNumber].transform.Find("Description").gameObject;
        Description.transform.Find("Text").GetComponent<Text>().text = m_AbilityDescriptions[abilityNumber];
    }
    protected void ChangeIconColours(Controllers controller)
    {
        int playerNumber = (int)(m_PlayerManager.PlayerNumbers[controller]);
        Text abilityText = WeaponSelects[playerNumber].transform.Find("Text").GetComponent<Text>();
        CharacterStats stats = m_PlayerManager.Players[controller].GetComponent<CharacterStats>();
        Transform unhighlightedIcons = WeaponSelects[playerNumber].transform.Find("Icons").Find("UnhighlightedIcons");
        int numIcons = unhighlightedIcons.childCount;
        List<Image> Icons = new List<Image>();
        for (int i = 0; i < numIcons; ++i)
        {
            Icons.Add(unhighlightedIcons.GetChild(i).gameObject.GetComponent<Image>());
        }

        int abilityNumber = 0;
        foreach (string ability in m_Abilities)
        {
            if (ability == abilityText.text)
            {
                if (abilityNumber == m_Abilities.Count - 1)
                {
                    abilityNumber = -1;
                }
                break;
            }
            abilityNumber++;
        }

        abilityNumber++;

        int numAbilitiesColoured = 0;
        while (numAbilitiesColoured < 8)
        {
            Icons[numAbilitiesColoured].color = m_AbilityColours[abilityNumber];
            foreach (Ability ability in stats.Abilities)
            {
                if (ability != null)
                {
                    string abilityName = ability.AbilityName;
                    if (abilityName == m_Abilities[abilityNumber])
                    {
                        Icons[numAbilitiesColoured].color = m_EquippedAbilityColour;
                    }
                }
            }

            numAbilitiesColoured++;
            abilityNumber++;

            if (abilityNumber == m_Abilities.Count)
            {
                abilityNumber = 0;
            }
        }
    }

    protected void ChangeIcon(Controllers controller, string currrentSelectedAbility)
    {
        int playerNumber = (int)(m_PlayerManager.PlayerNumbers[controller]);
        Sprite icon = WeaponSelects[playerNumber].transform.Find("Icons").transform.Find("Icon").GetComponent<Image>().sprite;

        for (int i = 0; i < m_Abilities.Count; i++)
        {
            if (currrentSelectedAbility == m_Abilities[i]) { icon = Icons[i]; } // as long as these two arrays stay in the same order this should work
        }

        WeaponSelects[playerNumber].transform.Find("Icons").transform.Find("Icon").gameObject.SetActive(true);
        WeaponSelects[playerNumber].transform.Find("Icons").transform.Find("Icon").GetComponent<Image>().sprite = icon;
    }

    protected void ChangeIcon(Controllers controller, string ability, int abilityNumber)
    {
        string button = string.Empty;
        int playerNumber = (int)(m_PlayerManager.PlayerNumbers[controller]);

        switch (abilityNumber)
        {
            case 1:
                button = "XButton";
                break;
            case 2:
                button = "YButton";
                break;
            case 3:
                button = "BButton";
                break;
            default:
                return;
        }


        Sprite icon = WeaponSelects[playerNumber].transform.Find(button).transform.Find("Icon").GetComponent<Image>().sprite;

        for (int i = 0; i < m_Abilities.Count; i++)
        {
            if (ability == m_Abilities[i]) { icon = Icons[i]; break; } // as long as these two arrays stay in the same order this should work
        }

        WeaponSelects[playerNumber].transform.Find(button).transform.Find("Icon").gameObject.SetActive(true);
        WeaponSelects[playerNumber].transform.Find(button).transform.Find("Icon").GetComponent<Image>().sprite = icon;
    }

    protected void PlayAnimation(Controllers controller, string currentSelectedAbility)
    {
        GameObject player = m_PlayerManager.Players[controller];
        Animator animator = player.GetComponentInChildren<Animator>();
        CharacterStats stats = player.GetComponent<CharacterStats>();

        StopCurrentAnimation(animator);

        player.transform.Find("SwordAndShield").gameObject.SetActive(false);
        player.transform.Find("Bow").gameObject.SetActive(false);

        // find what ability I have selected and play an animation based off of that
        if (currentSelectedAbility == "Chain Lightning")
        {
            animator.SetBool("PlayChainLightning", true);
        }
        else if (currentSelectedAbility == "Charge" && stats.Abilities[0].AbilityName == "Sword and Shield")
        {
            player.transform.Find("SwordAndShield").gameObject.SetActive(true);
            animator.SetBool("PlayChargeSword", true);
        }
        else if (currentSelectedAbility == "Charge" && stats.Abilities[0].AbilityName == "Bow")
        {
            player.transform.Find("Bow").gameObject.SetActive(true);
            animator.SetBool("PlayChargeBow", true);
        }
        else if (currentSelectedAbility == "Energy Bomb")
        {
            animator.SetBool("PlayEnergyBomb", true);
        }
        else if (currentSelectedAbility == "Enrage")
        {
            animator.SetBool("PlayEnrage", true);
        }
        else if (currentSelectedAbility == "Ground Shock")
        {
            animator.SetBool("PlayGroundShock", true);
        }
        else if (currentSelectedAbility == "Heal")
        {
            animator.SetBool("PlayHeal", true);
        }
        else if (currentSelectedAbility == "Protect Me")
        {
            animator.SetBool("PlayProtectMe", true);
        }
        else if (currentSelectedAbility == "Vortex")
        {
            animator.SetBool("PlayVortex", true);
        }
        else if (currentSelectedAbility == "Weapon Smash" && stats.Abilities[0].AbilityName == "Sword and Shield")
        {
            player.transform.Find("SwordAndShield").gameObject.SetActive(true);
            animator.SetBool("PlayWeaponSmashSword", true);
        }
        else if (currentSelectedAbility == "Weapon Smash" && stats.Abilities[0].AbilityName == "Bow")
        {
            player.transform.Find("Bow").gameObject.SetActive(true);
            animator.SetBool("PlayWeaponSmashBow", true);
        }
        else
        {
            // give me back my weapon and stop whatever ability animation I was on
            if (stats.Abilities[0].AbilityName == "Sword and Shield")
            {
                player.transform.Find("SwordAndShield").gameObject.SetActive(true);
            }
            else
            {
                player.transform.Find("Bow").gameObject.SetActive(true);
            }
            StopCurrentAnimation(animator);
        }
    }

    protected void StopCurrentAnimation(Animator animator)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Bool)
            {
                animator.SetBool(parameter.name, false);
            }
        }
    }

    protected void GivePlayerSelectedAbility(Controllers controller, int abilityNumber)
    {
        CharacterStats stats = m_PlayerManager.Players[controller].GetComponent<CharacterStats>();
        int playerNumber = (int)(m_PlayerManager.PlayerNumbers[controller]);
        string ability = WeaponSelects[playerNumber].transform.Find("Text").GetComponent<Text>().text;
        if (ability == "Select Your Abilities")
        {
            return;
        }

        if (ability == m_Abilities[0]) { stats.Abilities[abilityNumber] = new ChainLightningAbility(stats); }
        else if (ability == m_Abilities[1]) { stats.Abilities[abilityNumber] = new ChargeAbility(stats); }
        else if (ability == m_Abilities[2]) { stats.Abilities[abilityNumber] = new EnergyBombAbility(stats); }
        else if (ability == m_Abilities[3]) { stats.Abilities[abilityNumber] = new EnrageAbility(stats); }
        else if (ability == m_Abilities[4]) { stats.Abilities[abilityNumber] = new GroundShockAbility(stats); }
        else if (ability == m_Abilities[5]) { stats.Abilities[abilityNumber] = new HealAbility(stats); }
        else if (ability == m_Abilities[6]) { stats.Abilities[abilityNumber] = new ProtectMeAbility(stats); }
        else if (ability == m_Abilities[7]) { stats.Abilities[abilityNumber] = new VortexAbility(stats); }
        else if (ability == m_Abilities[8]) { stats.Abilities[abilityNumber] = new WeaponSmashAbility(stats); }


        ChangeIcon(controller, ability, abilityNumber);

        switch (CurrentPlayerScreen[controller])
        {
            case LoadoutScene.AbilitySelector1:
                CurrentPlayerScreen[controller] = LoadoutScene.AbilitySelector2;
                WeaponSelects[playerNumber].transform.Find("XButton").GetComponent<Image>().enabled = false;

                break;
            case LoadoutScene.AbilitySelector2:
                CurrentPlayerScreen[controller] = LoadoutScene.AbilitySelector3;
                WeaponSelects[playerNumber].transform.Find("YButton").GetComponent<Image>().enabled = false;

                break;
            default:
                WeaponSelects[playerNumber].transform.Find("BButton").GetComponent<Image>().enabled = false;

                break;
        }

        if (abilityNumber != 3)
        {
            string abilityName = FindFirstAvailableAbility(controller);
            SetCurrentSelectedAbilityFromController(controller, abilityName);
            WeaponSelects[playerNumber].transform.Find("Text").GetComponent<Text>().text = abilityName;
            ChangeIcon(controller, abilityName);
            PlayAnimation(controller, abilityName);
        }

        ChangeIconColours(controller);
        ChangeAbilityDescription(controller);
        //StopCurrentAnimation(m_PlayerManager.Players[controller].GetComponentInChildren<Animator>());
    }

    protected string FindFirstAvailableAbility(Controllers controller)
    {
        CharacterStats stats = m_PlayerManager.Players[controller].GetComponent<CharacterStats>();

        string tempAbilityName = string.Empty;
        string currentAbility = GetCurrentSelectedAbilityFromController(controller);

        int i = 0;
        foreach (string abilityName in m_Abilities)
        {
            if (abilityName == currentAbility)
            {
                break;
            }
            i++;
        }

        if (i == 8) { i = -1; }

        tempAbilityName = m_Abilities[i + 1];

        int j = 1;
        int multiplier = 2;
        while (j < 4)
        {
            if (stats.Abilities[j] != null)
            {
                if (stats.Abilities[j].AbilityName == tempAbilityName)
                {
                    if (i + 1 * multiplier > 8)
                    {
                        i = -2;
                    }
                    tempAbilityName = m_Abilities[i + 1 * multiplier];
                    multiplier++;
                    j = 0;
                }
            }
            j++;
        }

        return tempAbilityName;
    }
    #endregion

    protected void SwitchToReadyScreen(Controllers controller)
    {
        StopCurrentAnimation(m_PlayerManager.Players[controller].GetComponentInChildren<Animator>());

        int playerNumber = (int)(m_PlayerManager.PlayerNumbers[controller]);
        CurrentPlayerScreen[controller] = LoadoutScene.Ready;
        JoinButtons[playerNumber].SetActive(false);
        WeaponSelects[playerNumber].transform.Find("Text").GetComponent<Text>().text = "Ready";
        WeaponSelects[playerNumber].transform.Find("RightButton").gameObject.SetActive(false);
        WeaponSelects[playerNumber].transform.Find("LeftButton").gameObject.SetActive(false);
        WeaponSelects[playerNumber].transform.Find("Icons").gameObject.SetActive(false);


    }
}

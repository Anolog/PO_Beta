using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class PostGameUIBehaviour : UIBehaviour
{
    //Button state enum to change displays with only having one button
    public enum PlayerContinueButtonState
    {
        Awards,
        ScoreBreakdown,
        WaitingForOthers

    };

    //Enum to keep track of the different awards
    public enum PlayerAwards
    {
        KilledBoss,
        MostRevives,
        MostAbilities,
        MostBasicAttacks,
        MostJumps,
        MostDistance,
        MostKnockback,

        Null
    };

    //private PlayerContinueButtonState[] m_PlayerButtonStates = new PlayerContinueButtonState[4] { 0, 0, 0, 0 };

    //For players controlling
    protected Dictionary<Controllers, PlayerContinueButtonState> m_PlayerButtonStates = new Dictionary<Controllers, PlayerContinueButtonState>();
    protected PlayerManager m_PlayerManager;

    private const int AMOUNT_OF_AWARDS_CAN_WIN = 3;

    public GameObject[] m_PlayerUIBlocks = new GameObject[4];
    public Text[] m_StatsText = new Text[4];
    public Text[] m_PlayerNum = new Text[4];
    public Text[] m_TotalScoreText = new Text[4];
    public Text m_WinOrLossText;

    //Relating to the player helmets for points
    public Image[] m_PlayerHelmetImage = new Image[4];
    public Sprite[] m_HelmetImages = new Sprite[3];
    private int[] m_TopScores = { 0, 0, 0, 0 };


    //Need for starting inthis scene
    public GameObject m_GameManagerPrefab;

    private List<GameObject> m_PlayerList;

    //In pixels for canvas
    private float m_SpacingBetweenBlocks = 100.0f;
    private float m_PlayerBlockWidth;

    private int m_PlayersOnContinue = 0;
    private int m_PlayerCount = 0;

    //Keep track of awards, put them in array
    //Index == enum order
    private int[] m_AwardWinners = new int[(int)PlayerAwards.Null] { -1, -1, -1, -1, -1, -1, -1 };

    //Reflections calls
    Dictionary<string, System.Reflection.MethodInfo> m_AwardListFunctions;

    // Use this for initialization
    protected override void Start()
    {
        if (m_GameManager == null)
        {
            m_GameManager = GameObject.FindGameObjectWithTag("GameManager");
            if (m_GameManager == null)
            {
                m_GameManager = Instantiate(m_GameManagerPrefab);
            }
        }

        GameManager.sceneManager.LoadingScreen.SetActive(false);
        m_PlayerInput = m_GameManager.GetComponent<GameManager>().playerInput;
        m_PlayerManager = m_GameManager.GetComponent<PlayerManager>();

        GameManager.audioManager.PlayMusic(AudioManager.Sounds.MUSIC_POST_GAME, 1);

        //Set the players
        foreach (Controllers controller in m_PlayerManager.Players.Keys)
        {
            //Debug.Log("Setting Up First Player Screen");
            m_PlayerButtonStates.Add(controller, PlayerContinueButtonState.Awards);
        }

        //Set first button selected (req for UI)
        FirstSelectedButton.Select();
        m_CurrentSelectedButton = FirstSelectedButton;

        //Set a playerlist from the game manager
        m_PlayerList = GameManager.playerManager.PlayerList();

        //Dictionary of the functions to call
        //System.Type functionType = m_PlayerList[0].GetComponent<PlayerStats>().GetType();
        m_AwardListFunctions = new Dictionary<string, System.Reflection.MethodInfo>();

        //Create another methods for reference for the loop
        System.Type t = m_PlayerList[0].GetComponent<PlayerStats>().GetType();
        System.Reflection.MethodInfo[] methods = t.GetMethods();

        for (int i = 0; i < methods.Length; i++)
        {
            //Create a reflection call to get the methods and store their addresses
            System.Reflection.MethodInfo m = methods[i];
            System.Attribute[] attrs = (System.Attribute[])m.GetCustomAttributes(true);

            //Add the attributes into the dictionary
            for (int j = 0; j < attrs.Length; j++)
            {
                //System.Attribute at = attrs[j];
                //Debug.Log("Adding Reflection: " + m.Name);

                //Check if key exists
                if (!m_AwardListFunctions.ContainsKey(m.Name))
                {
                    m_AwardListFunctions.Add(m.Name, m);

                }

            }
        }

        //m_AwardListFunctions.Add("MostAbilities", methods[0]);

        //Start setting up things for the players
        for (int i = 0; i < m_PlayerList.Count; i++)
        {
            m_PlayerCount++;

            //Set name
            m_PlayerNum[i].text = "Player " + (i + 1).ToString();

            //Set score
            //ShowTotalScore(m_PlayerList[i], ref m_StatsText[i]);


            int tempScore = m_PlayerList[i].GetComponent<PlayerStats>().ScorePointsEarned;

            for (int j = 0; j <= i; j++)
            {
                if (tempScore >= m_TopScores[j])
                {
                    if (tempScore == 0)
                    {

                        break;
                    }


                    for (int k = i; k >= j; k--)
                    {
                        if (k - 1 >= 0)
                        {
                            m_TopScores[k] = m_TopScores[k - 1];
                        }
                    }

                    m_TopScores[j] = tempScore;
                    break;
                }
            }

            /*
            int[] temp = new int[4];

            //Sort the scores real quick
            for (int j = 0; j < m_TopScores.Length; j++)
            {
                
                temp = m_TopScores;

                if (m_PlayerList[i].GetComponent<PlayerStats>().ScorePointsEarned == 0)
                {
                    break;
                }

                //Start at 0 being the top score, this also is useful for the enun

                else if (m_PlayerList[i].GetComponent<PlayerStats>().ScorePointsEarned > m_TopScores[j])
                {

                    for (int k = j; k < m_TopScores.Length - 1; k++)
                    {
                        m_TopScores[k] = temp[k + 1];
                    }

                    //Set their score in the table
                    m_TopScores[j] = m_PlayerList[i].GetComponent<PlayerStats>().ScorePointsEarned;


                    break;
                }
            }
            */

            //Find out who won what award
            //The string has to be the same name as the function
            m_AwardWinners[(int)PlayerAwards.KilledBoss] = DecidePlayerAwards("GetKilledBoss");
            m_AwardWinners[(int)PlayerAwards.MostAbilities] = DecidePlayerAwards("GetAbilitiesUsed");
            m_AwardWinners[(int)PlayerAwards.MostBasicAttacks] = DecidePlayerAwards("GetBasicAttacksUsed");
            m_AwardWinners[(int)PlayerAwards.MostJumps] = DecidePlayerAwards("GetJumps");
            m_AwardWinners[(int)PlayerAwards.MostDistance] = DecidePlayerAwards("GetDistanceTravelled");
            m_AwardWinners[(int)PlayerAwards.MostKnockback] = DecidePlayerAwards("GetKnockbacksUsed");
            m_AwardWinners[(int)PlayerAwards.MostRevives] = DecidePlayerAwards("GetRevives");

            //Show score
            m_TotalScoreText[i].text = "Total Score: " + m_PlayerList[i].GetComponent<PlayerStats>().ScorePointsEarned;

            //Show initial part
            ShowAwardsBreakdown(m_PlayerList[i], ref m_StatsText[i], i);
        }

        //Set the images
        for (int i = 0; i < m_PlayerList.Count; i++)
        {
            m_PlayerHelmetImage[i].gameObject.SetActive(false);
            //Debug.Log("Giving helmets");

            for (int j = 0; j < m_TopScores.Length - 1; j++)
            {
                if (m_PlayerList[i].GetComponent<PlayerStats>().ScorePointsEarned == 0)
                {
                    //m_PlayerHelmetImage[i].gameObject.SetActive(false);
                    break;
                }

                if (m_PlayerList[i].GetComponent<PlayerStats>().ScorePointsEarned == m_TopScores[j])
                {
                    m_PlayerHelmetImage[i].sprite = m_HelmetImages[j];
                    m_PlayerHelmetImage[i].gameObject.SetActive(true);
                    break;
                }
            }
        }

        //Get the size of the block
        m_PlayerBlockWidth = m_PlayerUIBlocks[0].GetComponent<RectTransform>().rect.width;

        //Set up how spaced out things are
        SetupSpacing();

        //Set win/lose
        if (m_PlayerList.Count != 0)
        {
            if (m_PlayerList[0].GetComponent<PlayerStats>().WinOrLoseGame == true)
            {
                m_WinOrLossText.text = "Mission Success!";
            }

            else
            {
                m_WinOrLossText.text = "Mission Failure!";
            }
        }

        else
        {
            m_WinOrLossText.text = "Error: PlayerList";
        }

        SetUpControls();
    }

    public void ShowTotalScore(GameObject player, ref Text displayText)
    {
        displayText.text = "Total Score: " + player.GetComponent<PlayerStats>().ScorePointsEarned.ToString();
    }

    public void ShowScoreBreakdown(GameObject player, ref Text displayText)
    {
        PlayerStats statsRef = player.GetComponent<PlayerStats>();

        //Reset
        displayText.text = string.Empty;

        //Start adding stuff
        //displayText.text += "Total Score: " + statsRef.ScorePointsEarned + "\n";
        //displayText.text += "Enemies Killed: " + statsRef.EnemiesKilled + "\n";

        displayText.text += "Damage Dealt: " + statsRef.DamageDealt + "\n";
        displayText.text += "Damage Received :" + statsRef.DamageTaken + "\n";
        displayText.text += "Downed: " + statsRef.GetDowns() + "\n";
        displayText.text += "Revived: " + statsRef.GetRevives() + "\n";
        displayText.text += "Healing Dealt: " + statsRef.HealingDone + "\n";
        displayText.text += "Healing Received: " + statsRef.HealingRecieved + "\n";
        //displayText.text += "Abilities Used: " + statsRef.GetAbilitiesUsed() + "\n";
        //displayText.text += "Basic Attacks Used: " + statsRef.GetBasicAttacksUsed() + "\n";
        //displayText.text += "Distance Travelled: " + statsRef.GetDistanceTravelled() + "\n";
        //displayText.text += "Amount of jumps: " + statsRef.GetJumps() + "\n";
        //displayText.text += "Knockbacks Used: " + statsRef.GetKnockbacksUsed() + "\n";

    }

    public void ShowAwardsBreakdown(GameObject player, ref Text displayText, int playerNum)
    {
        //PlayerStats statsRef = player.GetComponent<PlayerStats>();

        displayText.text = string.Empty;

        int counter = 0;


        //Loop through amount of awards to give to 1 person
        for (int i = 0; i < AMOUNT_OF_AWARDS_CAN_WIN; i++)
        {
            //Go through every award
            for (int awardNum = counter; awardNum < (int)PlayerAwards.Null - 1; awardNum++)
            {
                //If the player has the value, they win
                if (playerNum == m_AwardWinners[awardNum])
                {
                    //Find the value to give apropriate title
                    switch (awardNum)
                    {
                        case (int)PlayerAwards.KilledBoss:
                            displayText.text += "Boss Slayer \n";
                            counter = awardNum;
                            break;

                        case (int)PlayerAwards.MostAbilities:
                            displayText.text += "Ability Caster \n";
                            counter = awardNum;
                            break;

                        case (int)PlayerAwards.MostBasicAttacks:
                            displayText.text += "Relentless Attacker \n";
                            counter = awardNum;
                            break;

                        case (int)PlayerAwards.MostDistance:
                            displayText.text += "Marathon Runner \n";
                            counter = awardNum;
                            break;

                        case (int)PlayerAwards.MostJumps:
                            displayText.text += "Bunny Hopper \n";
                            counter = awardNum;
                            break;

                        case (int)PlayerAwards.MostKnockback:
                            displayText.text += "Personal Space Issues \n";
                            counter = awardNum;
                            break;
                        case (int)PlayerAwards.MostRevives:
                            displayText.text += "Combat Medic \n";
                            counter = awardNum;
                            break;

                        default:
                            break;
                    }

                    //Add 1 to counter so you can't check the same award
                    //Break out of this loop so you don't go through every award 3 times
                    counter++;
                    break;
                }
            }

            //If they didn't win anything
            if (displayText.text == "" || displayText.text == " ")
            {
                displayText.text = "No Accolades Achieved";
            }
        }
    }

    public void WaitingOnOtherPlayers(ref Text displaytext)
    {
        displaytext.text = "Waiting for other players";
    }

    public override void SetUpControls()
    {
        if (m_PlayerInput == null)
        {
            return;
        }

        m_PlayerInput.HandleAButton = OnClick;
        //m_PlayerInput.HandleBButton = OnClick;
    }

    public override void RemoveControls()
    {
        m_PlayerInput.HandleAButton = null;
    }

    public void SetupMainMenu()
    {
        gameObject.SetActive(false);
        GameManager.sceneManager.SwitchScenes("Main_Menu");
    }

    protected override void Update()
    {
    }

    public void SetupSpacing()
    {
        //Enable blocks
        for (int i = 0; i < m_PlayerCount; i++)
        {
            //Debug.Log("Setting Player " + i + " Block Active");
            m_PlayerUIBlocks[i].gameObject.SetActive(true);
        }

        //canvas transform reference
        RectTransform canvasRectTransform = this.GetComponent<RectTransform>();

        //If one just center
        if (m_PlayerCount == 1)
        {
            m_PlayerUIBlocks[0].transform.position = new Vector3(canvasRectTransform.rect.width * 0.5f, canvasRectTransform.rect.height * 0.5f, canvasRectTransform.position.z);
        }

        //If it's 2, take the center and move them evenly spaced away from it
        else if (m_PlayerCount == 2)
        {
            m_PlayerUIBlocks[0].transform.position = new Vector3(canvasRectTransform.rect.width * 0.5f - (m_PlayerBlockWidth + m_SpacingBetweenBlocks),
                                                                 canvasRectTransform.rect.height * 0.5f, canvasRectTransform.position.z);

            m_PlayerUIBlocks[1].transform.position = new Vector3(canvasRectTransform.rect.width * 0.5f + (m_PlayerBlockWidth + m_SpacingBetweenBlocks),
                                                     canvasRectTransform.rect.height * 0.5f, canvasRectTransform.position.z);
        }

        //If it's 3, put one in the middle, then move the other 2 away from it
        else if (m_PlayerCount == 3)
        {
            //Put player 2 in the middle
            m_PlayerUIBlocks[1].transform.position = new Vector3(canvasRectTransform.rect.width * 0.5f, canvasRectTransform.rect.height * 0.5f, canvasRectTransform.position.z);

            m_PlayerUIBlocks[0].transform.position = new Vector3(m_PlayerUIBlocks[1].transform.position.x - (m_PlayerBlockWidth + m_SpacingBetweenBlocks), canvasRectTransform.rect.height * 0.5f,
                                                                 canvasRectTransform.position.z);

            m_PlayerUIBlocks[2].transform.position = new Vector3(m_PlayerUIBlocks[1].transform.position.x + (m_PlayerBlockWidth + m_SpacingBetweenBlocks), canvasRectTransform.rect.height * 0.5f,
                                                                 canvasRectTransform.position.z);
        }

        //If it's 4, then put one close too the edge of the left side, and just make the next ones use the previous pos and spread out
        else if (m_PlayerCount == 4)
        {
            //Set the middle blocks
            m_PlayerUIBlocks[1].transform.position = new Vector3((canvasRectTransform.rect.width * 0.5f) - (m_PlayerBlockWidth * 0.5f),
                                                                 canvasRectTransform.rect.height * 0.5f,
                                                                 canvasRectTransform.position.z);

            m_PlayerUIBlocks[2].transform.position = new Vector3((canvasRectTransform.rect.width * 0.5f) + (m_PlayerBlockWidth * 0.5f),
                                                     canvasRectTransform.rect.height * 0.5f,
                                                     canvasRectTransform.position.z);

            //Set the ones beside them

            m_PlayerUIBlocks[0].transform.position = new Vector3(m_PlayerUIBlocks[1].transform.position.x - m_PlayerBlockWidth,
                                                                 m_PlayerUIBlocks[1].transform.position.y,
                                                                 m_PlayerUIBlocks[1].transform.position.z);

            m_PlayerUIBlocks[3].transform.position = new Vector3(m_PlayerUIBlocks[2].transform.position.x + m_PlayerBlockWidth,
                                                     m_PlayerUIBlocks[2].transform.position.y,
                                                     m_PlayerUIBlocks[2].transform.position.z);

            /*
            //Set the first one a bit manually                                              //0.62f
            m_PlayerUIBlocks[0].transform.position = new Vector3(0.0f + (m_PlayerBlockWidth), canvasRectTransform.rect.height * 0.5f, canvasRectTransform.position.z);

            //Set the rest with a for loop
            for (int i = 1; i < m_PlayerCount; i++)
            {
                m_PlayerUIBlocks[i].transform.position = new Vector3(m_PlayerUIBlocks[i - 1].transform.position.x + (m_PlayerBlockWidth + m_SpacingBetweenBlocks), canvasRectTransform.rect.height * 0.5f,
                                                                     canvasRectTransform.position.z);
            }
            */
        }
    }

    //Override onclick to do this stuff
    protected override void OnClick(Controllers controller)
    {
        int playerNumber = (int)(m_PlayerManager.PlayerNumbers[controller]);
        //playerNumber -= 1;

        if (m_PlayerManager.Players.ContainsKey(controller))
        {
            if (m_PlayerButtonStates[controller] == PlayerContinueButtonState.Awards)
            {
                GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_CONFIRM);
                m_PlayerButtonStates[controller] = PlayerContinueButtonState.ScoreBreakdown;
                ShowScoreBreakdown(m_PlayerList[playerNumber], ref m_StatsText[playerNumber]);
                return;
            }

            else if (m_PlayerButtonStates[controller] == PlayerContinueButtonState.ScoreBreakdown)
            {
                if (GameManager.playerManager.PlayerList().Count == 1)
                {
                    GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_CONFIRM);
                    GameManager.sceneManager.ResetGame();

                    return;
                }

                GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_CONFIRM);
                m_PlayerButtonStates[controller] = PlayerContinueButtonState.WaitingForOthers;
                //ShowAwardsBreakdown(m_PlayerList[playerNumber], ref m_StatsText[playerNumber]);
                WaitingOnOtherPlayers(ref m_StatsText[playerNumber]);
                m_PlayersOnContinue += 1;
                return;
            }

            /*
            else if (m_PlayerButtonStates[controller] == PlayerContinueButtonState.Awards)
            {
                GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_CONFIRM);
                m_PlayerButtonStates[controller] = PlayerContinueButtonState.WaitingForOthers;
                WaitingOnOtherPlayers(ref m_StatsText[playerNumber]);
                m_PlayersOnContinue += 1;
            }
            */

            else if (m_PlayerButtonStates[controller] == PlayerContinueButtonState.WaitingForOthers)
            {
                if (m_PlayersOnContinue == m_PlayerList.Count)
                {
                    GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_CONFIRM);
                    GameManager.sceneManager.ResetGame();
                }

                return;
            }
        }
    }

    /*
    public void ContinueButtonPressed(Controller)
    {
        m_PlayerButtonStates[whichPlayerPressed] += 1;

        switch (m_PlayerButtonStates[whichPlayerPressed])
        {
            case PlayerContinueButtonState.TotalScore:
                ShowTotalScore(m_PlayerList[whichPlayerPressed], ref m_StatsText[whichPlayerPressed]);
                return;

            case PlayerContinueButtonState.ScoreBreakdown:
                ShowScoreBreakdown(m_PlayerList[whichPlayerPressed], ref m_StatsText[whichPlayerPressed]);
                return;

            case PlayerContinueButtonState.WaitingForOthers:
                WaitingOnOtherPlayers(ref m_StatsText[whichPlayerPressed]);
                //Check other players for if they're on this screen
                int stateCountCheck = 0;
                for (int i = 0; i < m_PlayerCount; i++)
                {
                    stateCountCheck += (int)m_PlayerButtonStates[i];
                }

                //playercount * 2 because of how the enum is set
                if (stateCountCheck == m_PlayerCount * 2)
                {
                    GameManager.sceneManager.ResetGame();
                }
                return;

            default:
                return;
        }
    }
    */

    //Takes int as param from the function getter, returns int as winner
    public int DecidePlayerAwards(string function)
    {
        int winningPlayer = -1;
        int tempTop = 0;
        float tempTopFloat = 0;

        for (int i = 0; i < m_PlayerList.Count; i++)
        {
            int tempNew;
            float tempNewFloat;
            //Call the function by name
            //object[] a = new object[] { };

            //For float
            if (function == "GetDistanceTravelled")
            {
                tempNewFloat = (float)m_AwardListFunctions[function].Invoke(m_PlayerList[i].GetComponent<PlayerStats>(), null);

                if (tempNewFloat > tempTopFloat && tempNewFloat > 10.0f)
                {
                    tempTopFloat = tempNewFloat;
                    winningPlayer = i;
                }
            }

            //Specific for bool
            else if (function == "GetKilledBoss")
            {
                if ((bool)m_AwardListFunctions[function].Invoke(m_PlayerList[i].GetComponent<PlayerStats>(), null) == true)
                {
                    winningPlayer = i;
                }

            }

            //Defauly for ints
            else
            {
                tempNew = (int)m_AwardListFunctions[function].Invoke(m_PlayerList[i].GetComponent<PlayerStats>(), null);

                if (tempNew > tempTop && tempNew != 0)
                {
                    tempTop = tempNew;
                    winningPlayer = i;
                }
            }


        }

        //Debug.Log("TempTop: " + tempTop);
        return winningPlayer;
    }

}
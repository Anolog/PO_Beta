
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutManager : MonoBehaviour
{
    [SerializeField]
    GameObject m_PlayerPrefab;

    TF_XINPUT m_ControllerInput;
    PlayerManager m_PlayerManager;

    //Put these in order
    [SerializeField]
    Text[] m_PlayerLoadoutText;

    List<string> m_Abilities;
    List<string> m_Weapons;

    public int NumPlayersInLoadout = 0;
    public int NumPlayersReady = 0;

    public bool LockOutP1 = false;

    //Listener for onClick
    public Button MainMenuButton;

    void Start()
    {
        //Listener for when you click on main menu, can't have in menu manager script because this object isn't active.
        //MainMenuButton.onClick.AddListener(() => 
        //{ GameManager.musicManager.PlayMusic(GameManager.musicManager.MusicTracks[0], true, true); });
        
        GameManager.audioManager.PlayMusic(AudioManager.Sounds.MUSIC_LOADOUT, 1);
        AddAbilitiesAndWeapons();

        m_ControllerInput = GameManager.controllerInput;
        m_PlayerManager = GameManager.playerManager;

        for (int i = 0; i < 4; i++)
        {
            m_PlayerLoadoutText[i].text = "Press Enter or A to Join";
        }
    }

    void AddAbilitiesAndWeapons()
    {
        m_Abilities = new List<string>();
        m_Weapons = new List<string>();

        m_Weapons.Add("Sword and Shield\n");
        m_Weapons.Add("Bow\n");

        m_Abilities.Add("<color=#0000ffff>Charge</color>\n");
        m_Abilities.Add("<color=#008080ff>Energy Bomb</color>\n");
        m_Abilities.Add("<color=#800080ff>Ground Shock</color>\n");
        m_Abilities.Add("<color=#ff0000ff>Weapon Smash</color>\n");
        m_Abilities.Add("<color=#008000ff>Heal</color>\n");
        m_Abilities.Add("<color=#ffa500ff>Enrage</color>\n");
        m_Abilities.Add("<color=#fdd023ff>Chain Lightning</color>\n");
        m_Abilities.Add("<color=#551a8bff>Vortex</color>\n");
        m_Abilities.Add("<color=#003400ff>Protect Me</color>\n");
    }

    Ability CreateAbility(string ability, CharacterStats character)
    {
        if (ability == "Sword and Shield")
            return new BasicMeleeAbility(character);
        else if (ability == "Bow")
            return new BasicRangedAbility(character);

        else if (ability == "<color=#0000ffff>Charge</color>")
            return new ChargeAbility(character);
        else if (ability == "<color=#008080ff>Energy Bomb</color>")
            return new EnergyBombAbility(character);
        else if (ability == "<color=#800080ff>Ground Shock</color>")
            return new GroundShockAbility(character);
        else if (ability == "<color=#ff0000ff>Weapon Smash</color>")
            return new WeaponSmashAbility(character);
        else if (ability == "<color=#008000ff>Heal</color>")
            return new HealAbility(character);
        else if (ability == "<color=#ffa500ff>Enrage</color>")
            return new EnrageAbility(character);
        else if (ability == "<color=#fdd023ff>Chain Lightning</color>")
            return new ChainLightningAbility(character);
        else if (ability == "<color=#551a8bff>Vortex</color>")
            return new VortexAbility(character);
        else if (ability == "<color=#003400ff>Protect Me</color>")
            return new ProtectMeAbility(character);

        else
            return new Ability(character);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!m_PlayerManager.Players.ContainsKey(Controllers.KEYBOARD_MOUSE))
            {
                AddPlayer(Controllers.KEYBOARD_MOUSE);
                NumPlayersInLoadout++;
            }

            StepForward(Controllers.KEYBOARD_MOUSE);
        }
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            StepBack(Controllers.KEYBOARD_MOUSE);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            MoveRight(Controllers.KEYBOARD_MOUSE);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            MoveLeft(Controllers.KEYBOARD_MOUSE);
        }


        for (int i = 0; i < 4; i++)
        {
            if (m_ControllerInput.IsControllerConnected((Controllers)i))
            {
                if (i == 0)
                {
                    if (LockOutP1)
                    {
                        continue;
                    }
                }
                GamePad pad = m_ControllerInput.GetController((Controllers)i);

                if (pad.GetButton(GAMEPAD_BUTTONS.A_BUTTON, BUTTON_STATES.PRESSED))
                {
                    if (!m_PlayerManager.Players.ContainsKey((Controllers)i))
                    {
                        AddPlayer((Controllers)i);
                        NumPlayersInLoadout++;
                    }

                    StepForward((Controllers)i);
                }
                if (pad.GetButton(GAMEPAD_BUTTONS.B_BUTTON, BUTTON_STATES.PRESSED))
                {
                    StepBack((Controllers)i);
                }
                if (pad.GetButton(GAMEPAD_BUTTONS.DPAD_RIGHT, BUTTON_STATES.PRESSED))
                {
                    MoveRight((Controllers)i);
                }
                if (pad.GetButton(GAMEPAD_BUTTONS.DPAD_LEFT, BUTTON_STATES.PRESSED))
                {
                    MoveLeft((Controllers)i);
                }

            }
        }

        //HandleLoadoutSwitching();

    }

    void AddPlayer(Controllers control)
    {
        GameObject player = Instantiate(m_PlayerPrefab);
        player.GetComponent<PlayerStats>().Controller = control;
        player.name = "Player" + (m_PlayerManager.numberOfPlayers + 1).ToString();
        player.SetActive(false);

        //do this before we add a player
        int numPlayers = m_PlayerManager.numberOfPlayers;

        m_PlayerManager.Players.Add(control, player);
        m_PlayerManager.PlayerNumbers.Add(control, (Players)numPlayers);

        m_PlayerLoadoutText[numPlayers].text = string.Empty;
    }

    void StepForward(Controllers controller)
    {
        int index = (int)m_PlayerManager.PlayerNumbers[controller];

        //check how many carriage returns are in the text
        string[] lines = m_PlayerLoadoutText[index].text.Split('\n');
        //there is always an extra empty string in the array
        int numLines = lines.Length - 1;

        CharacterStats player = m_PlayerManager.Players[controller].GetComponent<CharacterStats>();

        if (numLines == 0)
        {
            m_PlayerLoadoutText[index].text = m_Weapons[0];
            return;
        }
        else if (numLines == 1)
        {
            player.Abilities[numLines - 1] = CreateAbility(lines[numLines - 1], player);
            player.Abilities[4] = new BasicKnockbackAbility(player);
        }
        else if (numLines <= 4)
        {
            player.Abilities[numLines - 1] = CreateAbility(lines[numLines - 1], player);

            if (numLines == 4)
            {
                m_PlayerLoadoutText[index].text += "Ready!\n";
                NumPlayersReady++;
                return;
            }
        }
        else
        {
            return;
        }

        foreach (string str in m_Abilities)
        {
            string text = str.Remove(str.Length - 1);

            foreach (string line in lines)
            {
                if (text == line)
                {
                    break;
                }
                if (line == lines[numLines - 1])
                {
                    m_PlayerLoadoutText[index].text += str;
                    return;
                }
            }
        }


    }

    public void StepBack(Controllers controller)
    {
        if (m_PlayerManager.PlayerNumbers.ContainsKey(controller))
        {
            int index = (int)m_PlayerManager.PlayerNumbers[controller];
            m_PlayerLoadoutText[index].text = m_PlayerLoadoutText[index].text.Remove(m_PlayerLoadoutText[index].text.Length - 1);
            int retIndex = m_PlayerLoadoutText[index].text.LastIndexOf('\n');

            //character not found
            if (retIndex == -1)
            {
                m_PlayerLoadoutText[index].text = "Press Enter or A to Join";
                NumPlayersInLoadout--;

                if (m_PlayerManager.Players.ContainsKey(controller))
                {
                    m_PlayerManager.RemovedPlayer(controller);
                }
            }
            else
            {
                //check how many carriage returns are in the text
                string[] lines = m_PlayerLoadoutText[index].text.Split('\n');
                //there is always an extra empty string in the array
                int numLines = lines.Length - 1;
                if (numLines == 4)
                {
                    NumPlayersReady--;
                }
                m_PlayerLoadoutText[index].text = m_PlayerLoadoutText[index].text.Remove(retIndex + 1);
            }
        }
    }

    void MoveRight(Controllers controller)
    {
        if (m_PlayerManager.PlayerNumbers.ContainsKey(controller))
        {
            int index = (int)m_PlayerManager.PlayerNumbers[controller];

            //check how many carriage returns are in the text
            string[] lines = m_PlayerLoadoutText[index].text.Split('\n');
            //there is always an extra empty string in the array
            int numLines = lines.Length - 1;

            List<string> names = new List<string>();

            if (numLines == 1)
            {
                names = m_Weapons;
            }
            else if (numLines <= 4)
            {
                names = m_Abilities;
            }
            else // if i get here I am at the ready selection and do not need to, or want to move my position
            {
                return;
            }

            lines[numLines - 1] += '\n';
            int spot = names.IndexOf(lines[numLines - 1]) + 1;
            lines[numLines - 1] = lines[numLines - 1].Remove(lines[numLines - 1].Length - 1);

            if (spot == names.Count)
                spot = 0;

            m_PlayerLoadoutText[index].text = m_PlayerLoadoutText[index].text.Remove(m_PlayerLoadoutText[index].text.Length - 1);
            int retIndex = m_PlayerLoadoutText[index].text.LastIndexOf('\n');
            m_PlayerLoadoutText[index].text = m_PlayerLoadoutText[index].text.Remove(retIndex + 1);

            bool foundNewName = false;

            while (foundNewName == false)
            {
                foundNewName = true;

                foreach (string str in lines)
                {
                    if (str == names[spot].Remove(names[spot].Length - 1))
                    {
                        spot++;
                        if (spot == names.Count)
                            spot = 0;

                        foundNewName = false;
                        break;
                    }
                }
            }

            m_PlayerLoadoutText[index].text += names[spot];
        }
    }

    void MoveLeft(Controllers controller)
    {
        if (m_PlayerManager.PlayerNumbers.ContainsKey(controller))
        {
            int index = (int)m_PlayerManager.PlayerNumbers[controller];

            //check how many carriage returns are in the text
            string[] lines = m_PlayerLoadoutText[index].text.Split('\n');
            //there is always an extra empty string in the array
            int numLines = lines.Length - 1;

            List<string> names = new List<string>();

            if (numLines == 1)
            {
                names = m_Weapons;
            }
            else if (numLines <= 4)
            {
                names = m_Abilities;
            }
            else // if i get here I am at the ready selection and do not need to, or want to move my position
            {
                return;
            }


            lines[numLines - 1] += '\n';
            int spot = names.IndexOf(lines[numLines - 1]) - 1;
            lines[numLines - 1] = lines[numLines - 1].Remove(lines[numLines - 1].Length - 1);

            if (spot < 0)
                spot = names.Count - 1;

            m_PlayerLoadoutText[index].text = m_PlayerLoadoutText[index].text.Remove(m_PlayerLoadoutText[index].text.Length - 1);
            int retIndex = m_PlayerLoadoutText[index].text.LastIndexOf('\n');
            m_PlayerLoadoutText[index].text = m_PlayerLoadoutText[index].text.Remove(retIndex + 1);

            bool foundNewName = false;

            while (foundNewName == false)
            {
                foundNewName = true;

                foreach (string str in lines)
                {
                    if (str == names[spot].Remove(names[spot].Length - 1))
                    {
                        spot--;
                        if (spot < 0)
                            spot = names.Count - 1;

                        foundNewName = false;
                        break;
                    }
                }
            }

            m_PlayerLoadoutText[index].text += names[spot];
        }
    }

    //private void HandleLoadoutSwitching()
    //{
    //    if (m_PlayerSettings.Count <= 0)
    //        return;

    //    List<PlayerLoadout> keyList = new List<PlayerLoadout>(m_PlayerSettings.Keys);

    //    foreach (PlayerLoadout player in keyList)
    //    {
    //        byte currentSkill = m_PlayerSettings[player];

    //        if (player.controller == Controllers.KEYBOARD_MOUSE)
    //        {
    //            if (Input.GetKeyDown(KeyCode.W))
    //            {
    //                currentSkill++;

    //                if (currentSkill >= 5)
    //                    currentSkill = 4;

    //                m_PlayerSettings[player] = currentSkill;
    //            }
    //            else if (Input.GetKeyDown(KeyCode.S))
    //            {
    //                currentSkill--;

    //                if (currentSkill >= 5)
    //                    currentSkill = 0;

    //                m_PlayerSettings[player] = currentSkill;
    //            }

    //            if (Input.GetKeyDown(KeyCode.A))
    //            {
    //                player.PreviousAbility(currentSkill);
    //            }
    //            else if (Input.GetKeyDown(KeyCode.D))
    //            {
    //                player.NextAbility(currentSkill);
    //            }
    //        }
    //        else
    //        {
    //            GamePad controller = m_ControllerInput.GetController((int)player.controller);

    //            if (controller.GetButtonState(GAMEPAD_BUTTONS.DPAD_UP) == BUTTON_STATES.PRESSED)
    //            {
    //                currentSkill++;

    //                if (currentSkill >= 5)
    //                    currentSkill = 4;

    //                m_PlayerSettings[player] = currentSkill;
    //            }
    //            else if (controller.GetButtonState(GAMEPAD_BUTTONS.DPAD_DOWN) == BUTTON_STATES.PRESSED)
    //            {
    //                currentSkill--;

    //                if (currentSkill >= 5)
    //                    currentSkill = 0;

    //                m_PlayerSettings[player] = currentSkill;
    //            }

    //            if (controller.GetButtonState(GAMEPAD_BUTTONS.DPAD_RIGHT) == BUTTON_STATES.PRESSED)
    //            {
    //                player.NextAbility(currentSkill);
    //            }
    //            else if (controller.GetButtonState(GAMEPAD_BUTTONS.DPAD_LEFT) == BUTTON_STATES.PRESSED)
    //            {
    //                player.PreviousAbility(currentSkill);
    //            }
    //        }

    //        Debug.Log(currentSkill.ToString());
    //    }
    //}
}
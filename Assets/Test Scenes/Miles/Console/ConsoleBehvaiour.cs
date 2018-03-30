
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Reflection;

/// <summary>
/// Main hub for running commands off scripts and the ui for the console window
/// </summary>
public class ConsoleBehvaiour : MonoBehaviour
{

    #region Serialized Fields

    //Print the given buffer commands, might clutter up
    [SerializeField]
    private bool m_PrintBufferCommands;

    [SerializeField]
    private bool m_ToggleMouse = true;

    //Focus on the input field when the panel is awake
    [SerializeField]
    private bool m_AutoFocus = true;

    [SerializeField]
    private bool m_IgnoreToggleKey = true;

    [SerializeField]
    private bool m_ResetOnOpen = true;

    [SerializeField]
    private int m_MaxPreviousCommands = 5;

    [SerializeField]
    private int m_CanvasSortingOrder = 100;

    [SerializeField]
    private int m_MaxLines = 100;

    [SerializeField]
    private KeyCode m_ToggleKey = KeyCode.BackQuote;

    [SerializeField]
    private KeyCode m_NextCommandKey = KeyCode.UpArrow;

    [SerializeField]
    private KeyCode m_PreviousCommandKey = KeyCode.DownArrow;

    [SerializeField]
    private KeyCode m_ResetKey = KeyCode.RightArrow;

    [SerializeField]
    private RectTransform m_MainPanel;

    [SerializeField]
    private InputField m_MainInputField;

    [SerializeField]
    private Text m_CommandsText;

    [SerializeField]
    private RectTransform m_ContentRTransform;

    [SerializeField]
    private Scrollbar m_CommandsTextScrollBar;

    [SerializeField]
    private Text m_NumLinesText;

    [SerializeField]
    private Image m_LockImage, m_UnlockImage;

    [SerializeField]
    private GameObject m_EventSystem;

    #endregion

    //Built in commands
    public const string CPrint = "print",
                        CHelp = "help",
                        CClear = "clear",
                        CCommands = "commands",
                        CInvincible = "invinc",
                        CVincible = "vinc",
                        CDownPlayer = "downp",
                        CStopSpawning = "stpspwn",
                        CStartSpawning = "strtspwn",
                        CRemoveCooldowns = "nocds",
                        CResetCooldowns = "resetcds",
                        CJumpToPostGame = "endgame",
                        CSpawnEnemies = "spwn",
                        CEnemyInvincible = "eninvinc",
                        CEnemyVincible = "envinc",
                        CJumpToWave = "gotowave",
                        CFPS = "fps",
                        CNoFPS = "nofps";


    //Help info printout
    private const string HelpBlock = "GameObject method parameter1 parameter2 ...";
    private const string CommandBlock = "Commands: print, help, clear, invinc, vinc, downp, stpspwn, strtspwn, nocds, resetcds, endgame, spwn, eninvinc, envinc, gotowave, fps, nofps";

    //Track previous commands
    private string[] m_PreviousCommands;

    //Which previous command are we on?
    private int m_CommandsIndex = 0;
    private int m_NumPreviousCommands = 0;

    //Lines of commands in the command text box
    private int m_NumLines = 0;

    private Animator m_Animator;

    //Awake, Start, Update -> process buffer and keys
    #region Base Monobehaviour Methods

    private void Start()
    {
        m_MainPanel.gameObject.SetActive(true);

        m_Animator = GetComponentInChildren<Animator>();

        m_PreviousCommands = new string[m_MaxPreviousCommands];
        for (int i = 0; i < m_MaxPreviousCommands; i++) m_PreviousCommands[i] = string.Empty;

        PrintToConsole("Type help for useful informtion");

        GetComponentInChildren<Canvas>().sortingOrder = m_CanvasSortingOrder;

        m_MainPanel.gameObject.SetActive(false);

        if (m_UnlockImage == null)
        {
            m_UnlockImage = null;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(m_ToggleKey))
        {
            if (!m_MainInputField.isFocused)
            {
                ToggleConsoleWindow();
            }
            if (m_IgnoreToggleKey && m_MainInputField.isFocused)
            {
                ToggleConsoleWindow();
            }
        }

        if (Input.GetKeyDown(m_NextCommandKey)) MoveCommand(1);
        else if (Input.GetKeyDown(m_PreviousCommandKey)) MoveCommand(-1);

        if (Input.GetKeyDown(m_ResetKey)) ResetConsole();

        //Run buffer commands
        for (int i = 0; i < m_CommandBuffer.Count; i++)
        {
            if (m_PrintBufferCommands)
                PrintToConsole(m_CommandBuffer[i]);
            ProcessCommand(m_CommandBuffer[i]);
            m_CommandBuffer.RemoveAt(i);
            i--;
        }
    }

    #endregion

    //Handling the previous commands functionality
    #region Previous Command Methods

    /// <summary>
    /// Move through the previous command array and add the correct text to the input
    /// </summary>
    /// <param name="step">Which direction we step in the array, how much</param>
    private void MoveCommand(int step)
    {
        m_CommandsIndex += step;
        if (m_CommandsIndex >= m_NumPreviousCommands) m_CommandsIndex = 0;
        else if (m_CommandsIndex < 0) m_CommandsIndex = m_NumPreviousCommands - 1;

        m_MainInputField.Select(); m_MainInputField.ActivateInputField();
        m_MainInputField.text = m_PreviousCommands[m_CommandsIndex];
    }

    private void AddToPreviousCommands(string command)
    {
        if (m_NumPreviousCommands < m_MaxPreviousCommands) m_NumPreviousCommands++;

        string holder = m_PreviousCommands[0];
        string secondHolder = string.Empty;
        m_PreviousCommands[0] = command;
        for (int i = 1; i < m_MaxPreviousCommands; i++)
        {
            secondHolder = m_PreviousCommands[i];
            m_PreviousCommands[i] = holder;
            holder = secondHolder;
        }
    }

    #endregion

    //Methods pertaining to UI use and functionality
    #region Console UI

    private void ResetConsole()
    {
        LockToTop();
        m_LockImage.gameObject.SetActive(true);
        m_UnlockImage.gameObject.SetActive(false);
        m_MainPanel.anchoredPosition = Vector2.right * (m_MainPanel.sizeDelta.x * 0.5f) + Vector2.up * (m_MainPanel.sizeDelta.y * -0.5f);
    }

    public void LockToTop()
    {
        m_Animator.enabled = true;

        m_MainPanel.anchorMin = Vector2.zero + Vector2.up;
        m_MainPanel.anchorMax = Vector2.zero + Vector2.up;

        m_MainPanel.anchoredPosition = Vector2.right * (m_MainPanel.sizeDelta.x * 0.5f) + Vector2.up * (m_MainPanel.sizeDelta.y * -0.5f);
    }

    public void UnlockFromTop()
    {
        m_Animator.enabled = false;

        m_MainPanel.anchorMin = Vector2.one * 0.5f;
        m_MainPanel.anchorMax = Vector2.one * 0.5f;

        m_MainPanel.anchoredPosition = Vector2.zero;
    }

    private void UpdateNumLinesText()
    {
        m_NumLinesText.text = m_NumLines + string.Empty;
    }

    private void ResizeContent()
    {
        float height = LayoutUtility.GetPreferredHeight(m_CommandsText.rectTransform) + DevUI.MinContentSize;
        m_ContentRTransform.sizeDelta = new Vector2(m_ContentRTransform.sizeDelta.x, height);
    }

    private void PrintToConsole(string text)
    {
        if (m_CommandsText.text == string.Empty)
            m_CommandsText.text += text;
        else
            m_CommandsText.text += DevUI.BackSlashN + text;
        //Resize based on text box size
        ResizeContent();

        ////UI no longer automatically interprets \ codes
        //int numlines = text.Split(new string[] { "\n" }, StringSplitOptions.None).Length - 1;
        m_NumLines++;

        if (m_NumLines > m_MaxLines) TrimConsoleLines();

        UpdateNumLinesText();
    }

    private void TrimConsoleLines()
    {
        //UI no longer automatically interprets \ codes
        //while(m_NumLines > m_MaxLines)
        // {
        m_NumLines--;
        int index = m_CommandsText.text.IndexOf(DevUI.BackSlashN, 0) + 1;
        m_CommandsText.text = m_CommandsText.text.Remove(0, index);
        // }
    }

    /// <summary>
    /// Turn on and off the console UI
    /// </summary>
    public void ToggleConsoleWindow()
    {
        m_EventSystem.gameObject.SetActive(!m_MainPanel.gameObject.activeSelf);
        m_MainPanel.gameObject.SetActive(!m_MainPanel.gameObject.activeSelf);
        if (m_ToggleMouse) Cursor.visible = m_MainPanel.gameObject.activeSelf;
        if (m_MainPanel.gameObject.activeSelf)
        {
            if (m_AutoFocus)
            {
                m_MainInputField.Select();
                m_MainInputField.ActivateInputField();
            }

            if (m_ResetOnOpen) ResetConsole();
        }
    }

    /// <summary>
    /// Submit a command with the current instance, generally the UI is suppose to use this
    /// </summary>
    public void SubmitCommand()
    {
        if (!m_MainPanel.gameObject.activeSelf) return;

        if (m_MainInputField.text == string.Empty) return;

        AddToPreviousCommands(m_MainInputField.text);

        PrintToConsole(m_MainInputField.text);

        ProcessCommand(m_MainInputField.text);

        m_MainInputField.text = string.Empty;

        m_MainInputField.Select();
        m_MainInputField.ActivateInputField();

        m_CommandsTextScrollBar.value = 0;
    }

    #endregion

    //Handle commands
    #region Command Processing

    //Weak Points: Gameobjects can't have " in their names, gameobjects cant share names with built in functions
    private void ProcessCommand(string command)
    {
        #region Convert to data
        string tempCommand = command;
        string[] data;
        string[] tempData = new string[15];
        int numData = 0;
        while (tempCommand.Length > 0)
        {
            int endIndex = 0;
            int length = 0;

            if (tempCommand[0] == ' ') tempCommand = tempCommand.Remove(0, 1);

            if (tempCommand.Length <= 0) break;

            if (tempCommand[0] == '"')
            {
                endIndex = tempCommand.IndexOf('"', 1);

                if (endIndex == -1)
                {
                    PrintToConsole("Error: Lone \"");
                    return;
                }

                length = endIndex + 1;
                tempData[numData] = tempCommand.Substring(0, length);
                tempData[numData] = tempData[numData++].Replace("\"", string.Empty);
                tempCommand = tempCommand.Remove(0, length);
            }
            else
            {
                endIndex = tempCommand.IndexOf(' ', 1);
                if (endIndex == -1)
                {
                    tempData[numData++] = tempCommand;
                    tempCommand = tempCommand.Remove(0, tempCommand.Length);
                }
                else
                {
                    length = endIndex;
                    tempData[numData++] = tempCommand.Substring(0, length);
                    tempCommand = tempCommand.Remove(0, length);
                }
            }
        }

        data = new string[numData];

        for (int i = 0; i < numData; i++)
        {
            data[i] = tempData[i];
        }
        #endregion

        #region Built In Commands
        //check for built in commands first
        if (data[0] == CPrint)
        {
            if (data.Length > 1)
                PrintToConsole(data[1]);
            else
                PrintToConsole("Error: Command *Print* requires 1 parameter of type string");

            return;
        }
        else if (data[0] == CHelp) { PrintToConsole(HelpBlock); return; }
        else if (data[0] == CClear)
        {
            m_NumLines = 0;
            m_CommandsText.text = string.Empty;

            UpdateNumLinesText();

            ResizeContent();
            return;
        }
        else if (data[0] == CCommands) { PrintToConsole(CommandBlock); return; }
        else if (data[0] == CInvincible)
        {
            foreach (GameObject player in GameManager.playerManager.PlayerList())
            {
                player.GetComponent<CharacterStats>().SetInvincible(true);
            }

            PrintToConsole("All players are now invincible");
            return;
        }
        else if (data[0] == CVincible)
        {
            foreach (GameObject player in GameManager.playerManager.PlayerList())
            {
                player.GetComponent<CharacterStats>().SetInvincible(false);
            }

            PrintToConsole("All players can now be damaged");
            return;
        }
        else if (data[0] == CDownPlayer)
        {
            if (data.Length == 1)
            {
                PrintToConsole("Error: Enter \"downp\" and then the player number");
                return;
            }

            int playerNumber;

            if (int.TryParse(data[1], out playerNumber))
            {
                GameObject playerToDown = GameObject.Find("Player" + playerNumber);
                if (playerToDown == null)
                {
                    PrintToConsole("Error: There is no Player" + playerNumber);
                    return;
                }
                Command.DownPlayer(playerToDown);
            }
            else
            {
                PrintToConsole("Error: Enter \"downp\" and then the player number");
                return;
            }

            return;
        }
        else if (data[0] == CStopSpawning)
        {
            WaveSpawner spawner = GameObject.Find("Game").GetComponent<WaveSpawner>();
            spawner.StopSpawning();
            PrintToConsole("Enemies have been killed, and no more will spawn.");
            return;
        }
        else if (data[0] == CStartSpawning)
        {
            WaveSpawner spawner = GameObject.Find("Game").GetComponent<WaveSpawner>();
            spawner.StartSpawning();
            PrintToConsole("Your wave will finish, starting at the " + (spawner.NumberEnemiesSpawned() + 1) + " enemy.");
            return;
        }
        else if (data[0] == CRemoveCooldowns)
        {
            foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
            {
                CharacterStats stats = player.GetComponent<CharacterStats>();
                foreach (Ability ability in stats.Abilities)
                {
                    ability.RemoveCooldown();
                }
            }
            PrintToConsole("All ability cooldowns have been reduced");
            return;
        }
        else if (data[0] == CResetCooldowns)
        {
            foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
            {
                CharacterStats stats = player.GetComponent<CharacterStats>();
                foreach (Ability ability in stats.Abilities)
                {
                    ability.ResetCooldown();
                }
            }
            PrintToConsole("All ability cooldowns have been reset");
            return;
        }
        else if (data[0] == CJumpToPostGame)
        {
            foreach (GameObject player in GameManager.playerManager.PlayerList())
            {
                player.SetActive(false);
            }
            GameManager.sceneManager.SwitchScenes("PostGame_Scene");
            return;
        }
        else if (data[0] == CSpawnEnemies)
        {
            int numToSpawn;
            WaveSpawner spawner = GameObject.Find("Game").GetComponent<WaveSpawner>();
            if (data.Length > 2)
            {
                if (int.TryParse(data[2], out numToSpawn))
                {
                    int time = (int)((numToSpawn * 0.1f) + 0.5f);
                    if (time <= 0)
                    {
                        time = 1;
                    }
                    if (data[1] == "fodder")
                    {
                        GameObject enemy = GameObject.Find("Game").GetComponent<Game>().Enemies[0];
                        spawner.SpawnEnemy(enemy, numToSpawn);
                    }
                    else if (data[1] == "ranged")
                    {
                        GameObject enemy = GameObject.Find("Game").GetComponent<Game>().Enemies[1];
                        spawner.SpawnEnemy(enemy, numToSpawn);
                    }
                    else if (data[1] == "support")
                    {
                        GameObject enemy = GameObject.Find("Game").GetComponent<Game>().Enemies[2];
                        spawner.SpawnEnemy(enemy, numToSpawn);
                    }
                    else if (data[1] == "boss")
                    {
                        for (int i = 0; i < numToSpawn; i++)
                        {
                            spawner.SpawnBoss(GameObject.Find("Game").GetComponent<Game>().Boss);
                        }
                    }
                    else
                    {
                        PrintToConsole("Error: Enter a valid enemy");
                        return;
                    }

                    PrintToConsole("Spawning " + numToSpawn + " " + data[1] + " enemies");
                    return;
                }
                else
                {
                    PrintToConsole("Error: Enter a valid number of enemies to spawn");
                    return;
                }
            }
            else
            {
                if (data[1] == "fodder")
                {
                    GameObject enemy = GameObject.Find("Game").GetComponent<Game>().Enemies[0];
                    spawner.SpawnEnemy(enemy, 1);
                }
                else if (data[1] == "ranged")
                {
                    GameObject enemy = GameObject.Find("Game").GetComponent<Game>().Enemies[1];
                    spawner.SpawnEnemy(enemy, 1);
                }
                else if (data[1] == "support")
                {
                    GameObject enemy = GameObject.Find("Game").GetComponent<Game>().Enemies[2];
                    spawner.SpawnEnemy(enemy, 1);
                }
                else if (data[1] == "boss")
                {
                    spawner.SpawnBoss(GameObject.Find("Game").GetComponent<Game>().Boss);
                }
                else
                {
                    PrintToConsole("Error: Enter a valid enemy");
                    return;
                }

                PrintToConsole("Spawning 1 " + data[1] + " enemy");
                return;
            }
        }
        else if (data[0] == CEnemyInvincible)
        {
            GameObject.Find("Game").GetComponent<WaveSpawner>().SetSpawnInvincible(true);
            foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                enemy.GetComponent<CharacterStats>().SetInvincible(true);
            }
            GameObject boss = GameObject.FindGameObjectWithTag("Boss");
            if (boss != null)
            {
                GameObject.FindGameObjectWithTag("Boss").GetComponent<CharacterStats>().SetInvincible(true);
            }

            PrintToConsole("All enemies are now invincible");

            return;
        }
        else if (data[0] == CEnemyVincible)
        {
            GameObject.Find("Game").GetComponent<WaveSpawner>().SetSpawnInvincible(false);
            foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                enemy.GetComponent<CharacterStats>().SetInvincible(false);
            }
            GameObject boss = GameObject.FindGameObjectWithTag("Boss");
            if (boss != null)
            {
                GameObject.FindGameObjectWithTag("Boss").GetComponent<CharacterStats>().SetInvincible(false);
            }

            PrintToConsole("All enemies can now be damaged");

            return;
        }
        else if (data[0] == CJumpToWave)
        {
            int waveNum;
            if (data.Length == 2)
            {
                if (int.TryParse(data[1], out waveNum))
                {
                    if (waveNum > 6)
                    {
                        PrintToConsole("Error: Enter a number between 1 and 6");
                        return;
                    }
                    else if (waveNum == 6)
                    {
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
                        WaveSpawner spawner = GameObject.Find("Game").GetComponent<WaveSpawner>();
                        spawner.OverrideCurrentWave();
                        spawner.OverrideEnemiesLeft();
                        spawner.SpawnBoss(GameObject.Find("Game").GetComponent<Game>().Boss);

                        PrintToConsole("Jumping to the final wave... Prepare yourself");
                        return;
                    }
                    else
                    {
                        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
                        {
                            Command.KillEnemy(enemy);
                        }
                        foreach (GameObject boss in GameObject.FindGameObjectsWithTag("Boss"))
                        {
                            if (boss != null)
                            {
                                DestroyImmediate(boss);
                            }
                        }
                        Game game = GameObject.Find("Game").GetComponent<Game>();
                        game.OverrideWave(waveNum);

                        PrintToConsole("Jumping to wave " + waveNum);
                        return;
                    }
                }
                else
                {
                    PrintToConsole("Error: Enter " + CJumpToWave + " and then a space, and then the wave number");
                    return;
                }
            }
            else
            {
                PrintToConsole("Error: Enter " + CJumpToWave + " and then a space, and then the wave number");
                return;
            }
        }
        else if (data[0] == CFPS)
        {
            GameObject.Find("FPS Canvas").transform.Find("FPS Counter").gameObject.SetActive(true);
            PrintToConsole("FPS counter now visible");
            return;
        }
        else if (data[0] == CNoFPS)
        {
            GameObject.Find("FPS Canvas").transform.Find("FPS Counter").gameObject.SetActive(false);
            PrintToConsole("FPS counter no longer vicible");
            return;
        }
        #endregion

        #region Standard Command
        //Look for gameobjet given data[0]
        GameObject gameObject = GameObject.Find(data[0]);
        if (gameObject == null) //Doesn't exist, return an error and finish here
        {
            PrintToConsole("Error: No such gameobject " + data[0]);
            return;
        }

        //Get all monobehaviours
        MonoBehaviour[] monoBehaviours = gameObject.GetComponents<MonoBehaviour>();
        if (monoBehaviours == null || monoBehaviours.Length == 0) //Nothing exists, return 
        {
            PrintToConsole("Error: Gameobject " + data[0] + " has no monobehaviours on it");
            return;
        }

        //Loop to find the behaviour with the function name data[1]
        int index = -1;
        MethodInfo methodInfo = null;
        for (int i = 0; i < monoBehaviours.Length; i++)
        {
            methodInfo = monoBehaviours[i].GetType().GetMethod(data[1]);
            if (methodInfo != null)
            {
                index = i;
                break;
            }
        }

        if (index == -1 || methodInfo == null)//No monobehaviour holds the function
        {
            PrintToConsole("Error: Method " + data[1] + " does not exist on gameobject " + data[0]);
            return;
        }

        //check if we have all the parameters
        ParameterInfo[] parameterInfo = methodInfo.GetParameters();
        if (parameterInfo == null) //safety check
        {
            PrintToConsole("Error: Prameter info null");
            return;
        }

        //Quick check if there is 0 parameters, we can just invoke at this point
        if (parameterInfo.Length == 0)
        {
            methodInfo.Invoke(monoBehaviours[index], null);
            return;
        }

        int numParameters = parameterInfo.Length;
        if (numParameters != data.Length - 2)//we don't have enough parameters, data length but subtract the first 2 arguments(they arent parameters)
        {
            PrintToConsole("Error: Not enough parameters");
            return;
        }

        object[] parameters = new object[numParameters];

        int dataIndex = 2;
        //Now try to convert parameters
        for (int i = 0; i < numParameters; i++)
        {
            if (parameterInfo[i].ParameterType == typeof(float))
            {
                try
                {
                    parameters[i] = (float)Convert.ToDouble(data[dataIndex]);
                }
                catch
                {
                    PrintToConsole("Error: Parameter " + parameterInfo[i].Name + " failed to convert data to float");
                    return;
                }
            }
            else if (parameterInfo[i].ParameterType == typeof(int))
            {
                try
                {
                    parameters[i] = Convert.ToInt16(data[dataIndex]);
                }
                catch
                {
                    PrintToConsole("Error: Parameter " + parameterInfo[i].Name + " failed to convert data to int");
                    return;
                }
            }
            else if (parameterInfo[i].ParameterType == typeof(string))
            {
                try
                {
                    parameters[i] = data[dataIndex];
                }
                catch
                {
                    PrintToConsole("Error: String parameter error");
                    return;
                }
            }
            else if (parameterInfo[i].ParameterType.IsEnum)
            {
                try
                {

                    parameters[i] = Convert.ChangeType(Convert.ToInt16(data[dataIndex]), parameterInfo[i].ParameterType);
                }
                catch
                {
                    PrintToConsole("Error: Parameter " + parameterInfo[i].Name + " failed to convert data to enum or int");
                    return;
                }
            }
            else if (parameterInfo[i].ParameterType == typeof(bool))
            {
                try
                {

                    parameters[i] = Convert.ToBoolean(data[dataIndex]);
                }
                catch
                {
                    PrintToConsole("Error: Parameter " + parameterInfo[i].Name + " failed to convert data to bool");
                    return;
                }
            }

            dataIndex++;
        }

        //All done, now run it!
        methodInfo.Invoke(monoBehaviours[index], parameters);
        return;

        #endregion
    }

    #endregion

    #region Static

    //List of commands and a static way to call or print
    public static List<string> m_CommandBuffer = new List<string>();

    /// <summary>
    /// Add a command to the console's buffer to be parsed in update.
    /// </summary>
    /// <param name="command">The command to be parsed during update</param>
    public static void SubmitToBuffer(string command)
    {
        m_CommandBuffer.Add(command);
    }

    #endregion

    #region Misc

    public void Test()
    {
        PrintToConsole("Spicy Bois");
    }

    public void ParameterTest(int number)
    {
        PrintToConsole("Check this " + number + " out!");
    }

    /// <summary>
    /// Change the layer on which the canvas is drawn, this is suppose to be used lightly.
    /// </summary>
    public void ChangeCanvasSortingOrder(int sortingOrder)
    {
        m_CanvasSortingOrder = sortingOrder;
        GetComponentInChildren<Canvas>().sortingOrder = m_CanvasSortingOrder;
    }

    #endregion
}

#region UI Developer

/// <summary>
/// Helpers and builders for UI
/// </summary>
public static class DevUI
{
    public const string BackSlashN = "\n";
    public const int MinContentSize = 26;

    /// <summary>
    /// Build a given gameobject into a canvas
    /// </summary>
    public static void BuildIntoCanvas(ref GameObject gameObject, int renderOrder, RenderMode renderMode = RenderMode.ScreenSpaceOverlay, bool scale = true)
    {
        gameObject.AddComponent<RectTransform>();
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = renderMode;
        canvas.sortingOrder = renderOrder;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();

        if (scale)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Shrink;
        }
    }

    /// <summary>
    /// Build given gameobject into an event system
    /// </summary>
    public static void BuildIntoEventSystem(ref GameObject gameObject)
    {
        gameObject.AddComponent<EventSystem>();
        gameObject.AddComponent<StandaloneInputModule>();
    }

    /// <summary>
    /// Turn given gameobject into a ui text element
    /// </summary>
    public static void BuildIntoText(ref GameObject gameObject)
    {
        Text text = gameObject.AddComponent<Text>();

        text.color = Color.black;
        text.fontSize = 14;
        text.lineSpacing = 1;
        text.supportRichText = true;
        text.alignment = TextAnchor.UpperLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.resizeTextForBestFit = false;
        text.raycastTarget = false;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        gameObject.AddComponent<CanvasRenderer>();
        gameObject.AddComponent<RectTransform>();
    }

    /// <summary>
    /// Turn given gameobject into a ui image element
    /// </summary>
    public static void BuildIntoUIImage(ref GameObject gameObject)
    {
        Image image = gameObject.AddComponent<Image>();

        image.color = Color.white;
        image.raycastTarget = false;

        gameObject.AddComponent<CanvasRenderer>();
        gameObject.AddComponent<RectTransform>();
    }

    /// <summary>
    /// Turn given gameobject into a ui button element
    /// </summary>
    public static void BuildIntoButton(ref GameObject gameObject)
    {
        Button button = gameObject.AddComponent<Button>();
        Image image = gameObject.AddComponent<Image>();

        image.sprite = Resources.GetBuiltinResource<Sprite>("UISprite");
        image.color = Color.white;
        image.raycastTarget = true;
        image.type = Image.Type.Sliced;
        image.fillCenter = true;

        button.interactable = true;
        button.transition = Selectable.Transition.ColorTint;
        button.targetGraphic = image;
        #region Color Block
        ColorBlock colorBlock = new ColorBlock();
        colorBlock.normalColor = Color.white;
        colorBlock.highlightedColor = Color.gray;
        colorBlock.pressedColor = Color.red;
        colorBlock.disabledColor = Color.gray;
        colorBlock.colorMultiplier = 1;
        colorBlock.fadeDuration = 0.1f;
        button.colors = colorBlock;
        #endregion

        gameObject.AddComponent<CanvasRenderer>();
        gameObject.AddComponent<RectTransform>();
    }

    //This is kinda a test for other elements above
    /// <summary>
    /// Turn given gameobject into a ui button element
    /// </summary>
    public static void BuildIntoInputField(ref GameObject gameObject)
    {
        InputField inputField = gameObject.AddComponent<InputField>();
        Image image = gameObject.AddComponent<Image>();

        GameObject textGO = new GameObject("InputField Text");
        textGO.transform.SetParent(gameObject.transform);
        DevUI.BuildIntoText(ref textGO);
        Text text = textGO.GetComponent<Text>();

        #region Text Configuration
        text.rectTransform.anchorMin = new Vector2(0, 0);
        text.rectTransform.anchorMax = new Vector2(1, 1);
        text.rectTransform.pivot = new Vector2(0.5f, 0.5f);

        //Need to debug this!!
        text.rectTransform.anchoredPosition = new Vector2(10, 10);
        text.rectTransform.sizeDelta = new Vector2(7, 6);
        #endregion

        image.sprite = Resources.GetBuiltinResource<Sprite>("InputFieldBackground");
        image.color = Color.white;
        image.raycastTarget = true;
        image.type = Image.Type.Sliced;
        image.fillCenter = true;

        inputField.interactable = true;
        inputField.transition = Selectable.Transition.ColorTint;
        inputField.targetGraphic = image;
        #region Color Block
        ColorBlock colorBlock = new ColorBlock();
        colorBlock.normalColor = Color.white;
        colorBlock.highlightedColor = Color.gray;
        colorBlock.pressedColor = Color.red;
        colorBlock.disabledColor = Color.gray;
        colorBlock.colorMultiplier = 1;
        colorBlock.fadeDuration = 0.1f;
        inputField.colors = colorBlock;
        #endregion

        inputField.textComponent = text;
        inputField.text = string.Empty;
        inputField.characterLimit = 0;
        inputField.contentType = InputField.ContentType.Standard;
        inputField.lineType = InputField.LineType.SingleLine;
        inputField.placeholder = null;
        inputField.caretBlinkRate = 0.85f;
        inputField.caretWidth = 1;
        inputField.customCaretColor = false;
        inputField.shouldHideMobileInput = false;
        inputField.readOnly = false;
        inputField.selectionColor = Color.cyan;

        gameObject.AddComponent<CanvasRenderer>();
        gameObject.AddComponent<RectTransform>();
    }
}

#endregion

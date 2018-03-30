
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    Canvas[] m_Canvases;
    [SerializeField]
    GameObject m_SettingsMenu;
    [SerializeField]
    GameObject m_MainMenu;

	void Start ()
    {
        //Start main menu music
        GameManager.audioManager.PlayMusic(AudioManager.Sounds.MUSIC_MAIN_MENU, 1);

        //Add listener to the play button to allow going in and out of loadout without having the same music
        //GameObject.Find("Play_Button").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
        //{ GameManager.musicManager.PlayMusic(GameManager.musicManager.MusicTracks[2], true, true); });

    }
	
	void Update ()
    {
        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    PreviousMenu();
        //}
	}

    /// <summary>
    /// Used to open the previous menu, therefore settings or main menu. May need to be updated depending on our menu structure. 
    /// </summary>
    void PreviousMenu()
    {
        if (GetOpenCanvas() == GameObject.Find("AudioSettings_Canvas") || GetOpenCanvas() == GameObject.Find("GraphicsSettings_Canvas"))
        {
            CloseAllCanvasesExcept(m_SettingsMenu);
        }
        else if (GetOpenCanvas() == GameObject.Find("SettingsMenu_Canvas") || GetOpenCanvas() == GameObject.Find("Loadout_Canvas"))
        {
            CloseAllCanvasesExcept(m_MainMenu);

        }
    }

    /// <summary>
    /// Used to close all menus, then open the one that needs to be opened. 
    /// </summary>
    /// <param name="aCanvas"></param>
    public void CloseAllCanvasesExcept(GameObject aCanvas)
    {
        for (int i = 0; i < m_Canvases.Length; i++)
        {
            m_Canvases[i].gameObject.SetActive(false);
        }

        aCanvas.SetActive(true);
    }

    /// <summary>
    /// Return current open canvas. 
    /// </summary>
    /// <returns></returns>
    private GameObject GetOpenCanvas()
    {
        for (int i = 0; i < m_Canvases.Length; i++)
        {
            if (m_Canvases[i].gameObject.activeInHierarchy)
            {
                return m_Canvases[i].gameObject;
            }
        }

        return null;
    }

    /// <summary>
    /// Quit Game. 
    /// </summary>
    public void CloseGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif

        Application.Quit();
    }
}

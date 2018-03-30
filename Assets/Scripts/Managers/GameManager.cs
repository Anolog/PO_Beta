
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static private SimpleSceneManager m_SceneManager;
    /// <summary>
    /// Return scene Manager. 
    /// </summary>
    static public SimpleSceneManager sceneManager { get { return m_SceneManager; } }

    static TF_XINPUT m_ControllerInput;
    /// <summary>
    /// Return TF_XINPUT component. 
    /// </summary>
    static public TF_XINPUT controllerInput { get { return m_ControllerInput; } set { m_ControllerInput = value; } }
    /// <summary>
    /// Return Keyboard/Mouse component
    /// </summary>

    static private CameraManager m_CameraManager;
    /// <summary>
    /// Return Camera Manager
    /// </summary>
    static public CameraManager cameraManager { get { return m_CameraManager; } }

    static private PlayerManager m_PlayerManager;

    static public PlayerManager playerManager { get { return m_PlayerManager; } }

    static private GraphicsSettingsManager m_GraphicsManager;

    static public GraphicsSettingsManager graphicsManager { get { return m_GraphicsManager; } }

    static private MusicManager m_MusicManager;

    static public MusicManager musicManager { get { return m_MusicManager; } }

    static private AudioManager m_AudioManager;

    static public AudioManager audioManager { get { return m_AudioManager; } }

    public PlayerInput playerInput { get; private set; }

    public void SetPlayerInput(PlayerInput PlayerInput) { playerInput = PlayerInput; }
    void Awake ()
    {
        Time.timeScale = 1;

        DontDestroyOnLoad(this);

        playerInput = GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            playerInput = gameObject.AddComponent<PlayerInput>();
        }

        m_SceneManager = GetComponent<SimpleSceneManager>();

        if (m_SceneManager == null)
        {
            m_SceneManager = gameObject.AddComponent<SimpleSceneManager>();
        }

        m_PlayerManager = GetComponent<PlayerManager>();

        if (m_PlayerManager == null)
        {
            m_PlayerManager = gameObject.AddComponent<PlayerManager>();
        }

        m_ControllerInput = GetComponent<TF_XINPUT>();

        if (m_ControllerInput == null)
        {
            m_ControllerInput = gameObject.AddComponent<TF_XINPUT>();
        }

        m_CameraManager = GetComponent<CameraManager>();

        if (m_CameraManager == null)
        {
            m_CameraManager = gameObject.AddComponent<CameraManager>();
        }

        m_GraphicsManager = GetComponent<GraphicsSettingsManager>();

        if (m_GraphicsManager == null)
        {
            m_GraphicsManager = gameObject.AddComponent<GraphicsSettingsManager>();
        }

        m_MusicManager = GetComponent<MusicManager>();

        if (m_MusicManager == null)
        {
            m_MusicManager = gameObject.AddComponent<MusicManager>();
        }

        m_AudioManager = GetComponent<AudioManager>();

        if (m_AudioManager == null)
        {
            m_AudioManager = gameObject.AddComponent<AudioManager>();
        }
    }

	void Update ()
    {
		
	}
}

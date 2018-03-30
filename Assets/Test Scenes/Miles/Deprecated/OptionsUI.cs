//By Henry

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour{

    [SerializeField]
    private Slider m_MasterVolumeSlider;
    [SerializeField]
    private Slider m_MusicVolumeSlider;
    [SerializeField]
    private Slider m_SFXVolumeSlider;

    [SerializeField]
    private SystemManager m_SystemManager;

    [SerializeField]
    private GameObject m_Content;

    [SerializeField]
    private GameObject m_InputPrefab;

    [SerializeField]
    private GameObject m_GameOptionsPanel;

    [SerializeField]
    private GameObject m_GraphicsPanel;

    [SerializeField]
    private GameObject m_AudioPanel;

    [SerializeField]
    private GameObject m_ControlsPanel;

    [SerializeField]
    private GameObject m_WishToQuitPanel;

    [SerializeField]
    private Dropdown m_GraphicsDropDown;

    [SerializeField]
    private Dropdown m_ScreenPresets;

    [SerializeField]
    private Dropdown m_ResolutionDropDown;

    [SerializeField]
    private Toggle m_RunInBackgroundToggle;

    //private List<GameObject> m_InputPanels;

    //unused variable warning
    //private float m_Offset = -35;
    //private float m_Padding = -50;

    private bool m_ControlUIBuilt = false;

    private bool m_FullScreen;

    void Start(){
        if (m_SystemManager == null)
            m_SystemManager = GameObject.FindGameObjectWithTag("System Manager").GetComponent<SystemManager>();

        if (m_InputPrefab == null)
            m_InputPrefab = Resources.Load("Manager/Key Input Button", typeof(GameObject)) as GameObject;

        //m_InputPanels = new List<GameObject>();

        m_ScreenPresets.options.Add(new Dropdown.OptionData("Fullscreen"));
        m_ScreenPresets.options.Add(new Dropdown.OptionData("Window"));

        m_ScreenPresets.value = 0;
        m_ScreenPresets.RefreshShownValue();

        int i = 0;
        for (i = 0; i < QualitySettings.names.Length; i++)
        {
            m_GraphicsDropDown.options.Add(new Dropdown.OptionData(QualitySettings.names[i]));
        }

        m_ResolutionDropDown.ClearOptions();

        if (!Application.isEditor)
        {
            for (i = 0; i < Screen.resolutions.Length; i++)
            {
                m_ResolutionDropDown.options.Add(new Dropdown.OptionData(Screen.resolutions[i].ToString()));
            }

            m_ResolutionDropDown.value = GetResoultionIndex();
        }

        m_ResolutionDropDown.RefreshShownValue();

        m_GraphicsDropDown.value = QualitySettings.GetQualityLevel();
        m_RunInBackgroundToggle.isOn = Application.runInBackground;
    }

    private int GetResoultionIndex()
    {
        int i = 0;
        while (i < Screen.resolutions.Length)
        {
            if (Screen.resolutions[i].width == Screen.width && Screen.resolutions[i].height == Screen.height)
            {
                return i;
            }
            i++;
        }

        //This really should't happen in the build version
        return -1;
    }

    public void UpdateGraphics()
    {
        QualitySettings.SetQualityLevel(m_GraphicsDropDown.value);
        switch (m_ScreenPresets.value)
        {
            case 0:
                m_FullScreen = true;
                break;
            case 1:
                m_FullScreen = false;
                break;
        }

        if(m_FullScreen && Screen.resolutions[m_ResolutionDropDown.value].height != 1080)
        {
            m_FullScreen = false;
            m_ScreenPresets.value = 1;
            m_ScreenPresets.RefreshShownValue();
        }

        Application.runInBackground = m_RunInBackgroundToggle.isOn;
        Screen.SetResolution(Screen.resolutions[m_ResolutionDropDown.value].width, Screen.resolutions[m_ResolutionDropDown.value].height, m_FullScreen);
    }

    private int GetNativeResolution()
    {
        if(Screen.resolutions[Screen.resolutions.Length - 1].width > 1920)
        {
            for(int i = 0; i < Screen.resolutions.Length; i++)
            {
                if (Screen.resolutions[i].width == 1920) return i;
            }
        }
        return Screen.resolutions.Length - 1;
    }

    public void GraphicsDefualt()
    {
        m_GraphicsDropDown.value = QualitySettings.names.Length - 1;
        QualitySettings.SetQualityLevel(m_GraphicsDropDown.value);

        m_ResolutionDropDown.value = GetNativeResolution();
        Screen.SetResolution(Screen.resolutions[m_ResolutionDropDown.value].width, Screen.resolutions[m_ResolutionDropDown.value].height, Screen.fullScreen);
    }

    public void OpenAudioOptions()
    {
        //m_MasterVolumeSlider.value = m_SystemManager.AudioMaster.MasterVolume;
        //m_MusicVolumeSlider.value = m_SystemManager.AudioMaster.MusicVolume;
        //m_SFXVolumeSlider.value = m_SystemManager.AudioMaster.SFXVolume;
    }

    public void PlayButtonSound(){
        //m_SystemManager.PlayClip(AudioManager.UI_Press);
    }

    public void OpenControls(){
        if(!m_ControlUIBuilt){
            m_ControlUIBuilt = true;
            BuildInputPanels();
        }else
        {

        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void CloseOptions()
    {
        m_GameOptionsPanel.SetActive(false);
        m_GraphicsPanel.SetActive(false);
        m_AudioPanel.SetActive(false);
        m_ControlsPanel.SetActive(false);
        m_WishToQuitPanel.SetActive(false);
    }

    public void MusicSliderChange()
    {
        //m_SystemManager.AudioMaster.MusicVolume = m_MusicVolumeSlider.value;
    }

    public void MasterSliderChange()
    {
        //m_SystemManager.AudioMaster.MasterVolume = m_MasterVolumeSlider.value;
    }

    public void SFXSliderChange()
    {
        //m_SystemManager.AudioMaster.SFXVolume = m_SFXVolumeSlider.value;
    }

    public void ApplyControl()
    {
        //for (int i = 0; i < m_SystemManager.ControlMaster.GetControlSize(); i++)
        //{
        //    m_SystemManager.ControlMaster.GetControlData(i).Key = (KeyCode)m_InputPanels[i].GetComponentInChildren<InputField>().text.ToLower()[0];
        //    m_InputPanels[i].GetComponentInChildren<InputField>().text  = m_InputPanels[i].GetComponentInChildren<InputField>().text.ToUpper();
        //}
    }

    private void UpdateInputPanels()
    {
        //for (int i = 0; i < m_SystemManager.ControlMaster.GetControlSize(); i++)
        //{
        //    m_InputPanels[i].GetComponentInChildren<InputField>().text = m_SystemManager.ControlMaster.GetControlData(i).Key.ToString();
        //}
    }

    private void BuildInputPanels(){
        //for(int i = 0; i < m_SystemManager.ControlMaster.GetControlSize(); i++){
        //    ControlData data = m_SystemManager.ControlMaster.GetControlData(i);
        //    GameObject temp = Instantiate<GameObject>(m_InputPrefab, m_Content.transform);

        //    temp.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, m_Offset + i * m_Padding);

        //    temp.GetComponentInChildren<Text>().text = data.Name;
        //    temp.GetComponentInChildren<InputField>().text = data.GetRawKey().ToString();

        //    //Lock control while editing keys
        //    EventTrigger trigger = temp.GetComponentInChildren<EventTrigger>();
        //    EventTrigger.Entry entry = new EventTrigger.Entry();
        //    entry.eventID = EventTriggerType.Select;
        //    entry.callback.AddListener((eventData) => { m_SystemManager.ControlMaster.DisableControl(); });
        //    trigger.triggers.Add(entry);

        //    //Unlock control while editing keys
        //    entry = new EventTrigger.Entry();
        //    entry.eventID = EventTriggerType.Deselect;
        //    entry.callback.AddListener((eventData) => { m_SystemManager.ControlMaster.EnableControl(); });
        //    trigger.triggers.Add(entry);

        //    m_InputPanels.Add(temp);
        //}

        //m_Content.GetComponent<RectTransform>().sizeDelta = new Vector2(m_Content.GetComponent<RectTransform>().sizeDelta.x, -m_Padding * m_SystemManager.ControlMaster.GetControlSize() - m_Offset * 0.5f);
    }
}
using UnityEngine;

public class OptionsBehaviour : MonoBehaviour {

    public GameObject m_OptionsPanel;

	void Start () {
		
	}
	
	void Update () {
		
	}

    public void Play()
    {
        Debug.Log("Edit me, I should play!");
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void OpenOptions()
    {
        m_OptionsPanel.SetActive(true);
    }
}

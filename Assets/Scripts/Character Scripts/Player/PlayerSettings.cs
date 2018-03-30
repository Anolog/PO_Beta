
using UnityEngine;

public class PlayerSettings : MonoBehaviour
{
    Controllers m_Controller;
    
    public Controllers controller { get { return m_Controller; } set { m_Controller = value; } }

	void Start ()
    {
        DontDestroyOnLoad(this);
	}

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    void Update ()
    {
		
	}
}

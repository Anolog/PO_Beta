
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    Camera[] m_Cameras;

    [Range(1, 4)] public int m_Players = 1;

    bool started = false;

	void Awake()
    {
        started = true;

        m_Cameras = new Camera[4];

        //for (int i = 0; i < 4; i++)
        //{
        //    GameObject camera = new GameObject();
        //    BuildIntoCamera(ref camera, i == 0, "Camera " + i);
        //    m_Cameras[i] = camera.GetComponent<Camera>();
        //}

        //CheckPlayerAmount();
	}

    //will need some rework opnce players are joining themselves as opposed to being created in game
    public void SetupCameras(List<GameObject> players)
    {
        for (byte i = 0; i < players.Count; i++)
        {
            m_Cameras[i] = players[i].GetComponentInChildren<Camera>();
        }

        m_Players = players.Count;

        CheckPlayerAmount();
    }

#region static function

    /// <summary>
    /// Turn the given object into a unity default camera
    /// </summary>
    /// <param name="gameObject">Given gameobject</param>
    /// <param name="addAudioListner">Does this camera come with a audio listner</param>
    /// <param name="name">Give the new camera a name?</param>
    public static void BuildIntoCamera(ref GameObject gameObject, bool addAudioListner = false, string name = "Camera")
    {
        gameObject.name = name;
        //Maybe more camera detail control?
        Camera camera = gameObject.AddComponent<Camera>();
        camera.depth = -1;
        camera.transform.position = new Vector3(0,0,-10.0f);

        gameObject.AddComponent<FlareLayer>();
        gameObject.AddComponent<GUILayer>();
        if (addAudioListner) gameObject.AddComponent<AudioListener>();
    }

#endregion

    void Update ()
    {
		
	}

    private void OnValidate()
    {
        if(started)
            CheckPlayerAmount();
    }

    private void CheckPlayerAmount()
    {
        if (m_Players == 1)
            SetupOnePlayer();
        else if (m_Players == 2)
            SetupTwoPlayers();
        else if (m_Players == 3)
            SetupThreePlayers();
        else
            SetupFourPlayers();
    }

    private void SetupOnePlayer()
    {
        //EnableAllCameras();
        m_Cameras[0].rect = new Rect(new Vector2(0.0f, 0.0f), new Vector2(1.0f, 1.0f));
        //DisableOtherCameras(1);
    }

    private void SetupTwoPlayers()
    {
        m_Cameras[0].rect = new Rect(new Vector2(0.0f, 0.5f), new Vector2(1.0f, 0.5f));
        m_Cameras[1].rect = new Rect(new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.5f));
    }

    private void SetupThreePlayers()
    {
        //EnableAllCameras();
        m_Cameras[0].rect = new Rect(new Vector2(0.0f, 0.5f), new Vector2(1.0f, 0.5f));
        m_Cameras[1].rect = new Rect(new Vector2(0.0f, 0.0f), new Vector2(0.5f, 0.5f));
        m_Cameras[2].rect = new Rect(new Vector2(0.5f, 0.0f), new Vector2(0.5f, 0.5f));
        //DisableOtherCameras(3);
    }

    private void SetupFourPlayers()
    {
        //EnableAllCameras();
        m_Cameras[0].rect = new Rect(new Vector2(0.0f, 0.5f), new Vector2(0.5f, 0.5f));
        m_Cameras[1].rect = new Rect(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        m_Cameras[2].rect = new Rect(new Vector2(0.0f, 0.0f), new Vector2(0.5f, 0.5f));
        m_Cameras[3].rect = new Rect(new Vector2(0.5f, 0.0f), new Vector2(0.5f, 0.5f));
    }

    private void EnableAllCameras()
    {
        for (int i = 0; i < 4; i++)
        {
            m_Cameras[i].gameObject.SetActive(true);
        }
    }

    private void DisableOtherCameras(int aStartingRange)
    {
        for(int i = aStartingRange; i < 4; i++)
        {
            m_Cameras[i].gameObject.SetActive(false);
        }
    }
}

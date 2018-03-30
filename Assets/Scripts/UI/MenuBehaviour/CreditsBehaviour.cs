using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


public class CreditsBehaviour : UIBehaviour {

    [SerializeField]
    private GameObject m_MainMenu;

    [SerializeField]
    private float m_Speed = 30;

    StringReader m_Reader;
    Text m_Credits;
    TextAsset textFile;

    float startingY = -2294f;
    float m_StartingTime;

    //bool m_FirstTime = true;

    public float CreditsLenth = 0;
    public GameObject planet;


    protected override void Start()
    {
        Time.timeScale = 1;

        textFile = (TextAsset)(Resources.Load("Credits/Credits", typeof(TextAsset)));
        m_Reader = new StringReader(textFile.text);

        //m_Credits.text = m_Reader.ReadToEnd();

        base.Start();   
    }

    protected override void OnEnable()
    {
        SetUpControls();

        m_StartingTime = Time.time;



        StartCoroutine(SetStartPositionCoroutine());



        planet.SetActive(false);
    }

    IEnumerator SetStartPositionCoroutine()
    {
        yield return new WaitForEndOfFrame();

        m_Credits = transform.Find("Credits").GetComponent<Text>();
        Vector3 pos = m_Credits.gameObject.transform.position;

        pos.y = startingY;

        m_Credits.GetComponent<RectTransform>().position = pos;

        yield return null;
    }

    public override void SetUpControls()
    {
        if (m_PlayerInput == null)
        {
            return;
        }

        m_PlayerInput.HandleAButton = BackToMainMenu;
        m_PlayerInput.HandleStart = BackToMainMenu;
    }

    public override void RemoveControls()
    {
        m_PlayerInput.HandleAButton = null;
        m_PlayerInput.HandleStart = null;

        if (planet != null)
        {
            planet.SetActive(true);
        }
        m_Reader.Close();
    }

    public void BackToMainMenu(Controllers controller)
    {
        gameObject.SetActive(false);
        m_MainMenu.SetActive(true);
    }

    protected override void Update()
    {
        Vector3 pos = m_Credits.transform.position;

        pos.y += m_Speed * Time.deltaTime;

        m_Credits.transform.position = pos;

        //Debug.Log(Time.time - m_StartingTime);

        //Debug.Log(Time.time - m_StartingTime);

        if (Time.time - m_StartingTime >= CreditsLenth)
        {
            BackToMainMenu(Controllers.GAMEPAD_1); // the controller we put here does not matter
        }
    }
}

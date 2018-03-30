using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SplashScreen : MonoBehaviour {

    public float FadeInDuration = 0.75f;
    public float StayDuration = 1.0f;
    public float FadeOutDuration = 0.5f;

    public List<Image> Images;

    public Image Background;

    public GameObject LoadingScreen;

    AsyncOperation m_LoadingOperation;
    bool m_LoadingScene;

    int m_CurrentImage;

	// Use this for initialization
	void Start () {

        m_CurrentImage = 0;
        StartCoroutine(FadeIn(Images[0], Time.time));
        m_LoadingScene = false;
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.anyKeyDown && !m_LoadingScene)
        {
            StopAllCoroutines();

            if (m_CurrentImage < 2)
            {
                SwitchScreens();
            }
            else
            {
                LoadNextScene();
            }
        }
        

	}

    void SwitchScreens()
    {
        Images[m_CurrentImage].gameObject.SetActive(false);
        m_CurrentImage += 1;
        Images[m_CurrentImage].gameObject.SetActive(true);

        int screenColVal = m_CurrentImage % 2;

        Background.color = new Color(screenColVal, screenColVal, screenColVal, 1);

        StartCoroutine(FadeIn(Images[m_CurrentImage], Time.time));
    }

    void LoadNextScene()
    {
        m_LoadingScene = true;
        StopAllCoroutines();
        StartCoroutine(LoadMainMenu());
    }

    IEnumerator FadeIn(Image img, float startTime)
    {
        while(Time.time - startTime < FadeInDuration)
        {
            float timePercentage = (Time.time - startTime) / FadeInDuration;
            timePercentage = Mathf.SmoothStep(0, 1, timePercentage);
            img.color = new Color(1, 1, 1, timePercentage);
            yield return null;
        }

        StartCoroutine(Wait(img));
        yield return null;
    }

    IEnumerator Wait(Image img)
    {
        yield return new WaitForSeconds(StayDuration);

        StartCoroutine(FadeOut(img, Time.time));
    }

    IEnumerator FadeOut(Image img, float startTime)
    {
        while (Time.time - startTime < FadeInDuration)
        {
            float timePercentage = (Time.time - startTime) / FadeInDuration;
            timePercentage = Mathf.SmoothStep(0, 1, timePercentage);
            float oneMinusTime = 1 - timePercentage;
            img.color = new Color(1, 1, 1, oneMinusTime);

            if (m_CurrentImage == 1)
            {
                Background.color = new Color(oneMinusTime, oneMinusTime, oneMinusTime, 1);
            }
            else if (m_CurrentImage == 0)
            {
                Background.color = new Color(timePercentage, timePercentage, timePercentage, 1);
            }

            yield return null;
        }

        if (m_CurrentImage < 2)
        {
            SwitchScreens();
        }
        else
        {
            LoadNextScene();
        }

        yield return null;
    }

    IEnumerator LoadMainMenu()
    {
        m_LoadingOperation = SceneManager.LoadSceneAsync("Main_Menu");

        while (!m_LoadingOperation.isDone)
        {
            LoadingScreen.SetActive(true);

            float progress = m_LoadingOperation.progress;
            LoadingScreen.transform.Find("Loading Bar").GetComponent<Image>().fillAmount = progress;

            float alphaVal = Mathf.Sin(Mathf.PI * Time.time) * 0.5f + 0.5f;
            LoadingScreen.transform.Find("Loading Text").GetComponent<Image>().color = new Color(1, 1, 1, alphaVal);

            yield return null;
        }

        yield return null;
    }
}

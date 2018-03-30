
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class SimpleSceneManager : MonoBehaviour
{
    [SerializeField]
    LoadoutManager m_LM;

    public GameObject LoadingScreen;

    /// <summary>
    /// Change scenes, needs string. 
    /// </summary>
    /// <param name="aSceneName"></param>
    public void SwitchScenes(string aSceneName)
    {
        //SceneManager.LoadScene(aSceneName, LoadSceneMode.Single);

        StartCoroutine(LoadSceneAsynchronously(aSceneName));
    }

    IEnumerator LoadSceneAsynchronously(string sceneName)
    {
        LoadingScreen.SetActive(true);
        LoadingScreen.transform.Find("Loading Bar").GetComponent<Image>().fillAmount = 0;

        AsyncOperation loadScene = SceneManager.LoadSceneAsync(sceneName);

        while (!loadScene.isDone)
        {
            float progress = loadScene.progress;
            LoadingScreen.transform.Find("Loading Bar").GetComponent<Image>().fillAmount = progress;

            float alphaVal = Mathf.Sin(Mathf.PI * Time.unscaledTime) * 0.5f + 0.5f;
            LoadingScreen.transform.Find("Loading Text").GetComponent<Image>().color = new Color(1, 1, 1, alphaVal);

            yield return null;
        }

        LoadingScreen.transform.Find("Loading Bar").GetComponent<Image>().fillAmount = 1;
        //let them see that the bar is full
        yield return new WaitForSecondsRealtime(0.3f);
        LoadingScreen.SetActive(false);
        yield return null;
    }

    public void StartGame()
    {
        //m_LM.LoadPlayerManager();

        StartCoroutine(LoadSceneAsynchronously("Main_Scene"));
    }

    //private void Update()
    //{
    //    float alphaVal = Mathf.Sin(Mathf.PI * Time.unscaledTime) * 0.5f + 0.5f;
    //    LoadingScreen.transform.Find("Loading Text").GetComponent<Image>().color = new Color(1, 1, 1, alphaVal);
    //    LoadingScreen.transform.Find("Loading Bar").GetComponent<Image>().fillAmount = Time.time / 10.0f;
    //}

    public void ResetGame()
    {
        GameObject[] go = GameObject.FindObjectsOfType<GameObject>();

        for (int i = 0; i < go.Length; i++)
        {
            Destroy(go[i]);
        }

        StartCoroutine(LoadSceneAsynchronously("Main_Menu"));
    }
}
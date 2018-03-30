using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphicsSettingsManager : MonoBehaviour
{
    [SerializeField]
    float m_GammaCorrection = 0.5f;
    [SerializeField]
    Slider m_BrightnessSlider;
    [SerializeField]
    Slider m_QualitySlider;
    [SerializeField]
    Slider m_ResolutionSlider;

    List<int> m_ValidResolutions;

    void Awake()
    {
        m_ValidResolutions = new List<int>();

        RenderSettings.ambientLight = new Color(m_GammaCorrection, m_GammaCorrection, m_GammaCorrection, 1.0f);

        if (m_BrightnessSlider != null)
        {
            m_BrightnessSlider.value = m_GammaCorrection;

        }

        if (m_QualitySlider != null)
        {
            m_QualitySlider.value = QualitySettings.GetQualityLevel() / 10.0f;
        }

        if (m_ResolutionSlider != null)
        {
            SetupResolutionSlider();
        }
    }

    void SetScreenResolution(Resolution res)
    {
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }

    void Update()
    {
        if (m_BrightnessSlider == null)
        {
            GameObject obj = GameObject.Find("BrightnessSlider");

            if (obj == null)
            {
                obj = GameObject.Find("Brightness_Setting");

                if (obj != null)
                {
                    m_BrightnessSlider = obj.GetComponentInChildren<Slider>();
                    m_BrightnessSlider.value = m_GammaCorrection;
                }
            }
            else
            {
                m_BrightnessSlider = obj.GetComponent<Slider>();
                m_BrightnessSlider = obj.GetComponentInChildren<Slider>();
                m_BrightnessSlider.value = m_GammaCorrection;
            }
            RenderSettings.ambientLight = new Color(m_GammaCorrection, m_GammaCorrection, m_GammaCorrection, 1.0f);
        }

        if (m_QualitySlider == null)
        {
            GameObject obj = GameObject.Find("GraphicsQualitySlider");

            if (obj == null)
            {
                obj = GameObject.Find("GraphicsQuality_Setting");

                if (obj != null)
                {
                    m_QualitySlider = obj.GetComponentInChildren<Slider>();
                    m_QualitySlider.value = (float)QualitySettings.GetQualityLevel() / 10.0f;
                }
            }
            else
            {
                m_QualitySlider = obj.GetComponent<Slider>();
                m_QualitySlider.value = (float)QualitySettings.GetQualityLevel() / 10.0f;
            }
        }

        if (m_ResolutionSlider == null)
        {
            GameObject obj = GameObject.Find("ResolutionSlider");

            if (obj == null)
            {
                obj = GameObject.Find("Resolution_Settings");

                if (obj != null)
                {
                    m_ResolutionSlider = obj.GetComponentInChildren<Slider>();

                    SetupResolutionSlider();
                }
            }
            else
            {
                m_ResolutionSlider = obj.GetComponent<Slider>();

                SetupResolutionSlider();
            }
        }
    }

    void SetupResolutionSlider()
    {
        Resolution maxRes = Screen.resolutions[Screen.resolutions.Length - 1];
        float aspectRatio = (float)maxRes.width / maxRes.height;

        int currentResolutionIndex = 0;

        for (int i = 0; i <= Screen.resolutions.Length - 1; i++)
        {
            float currentAspect = (float)Screen.resolutions[i].width / Screen.resolutions[i].height;

            if (Mathf.Approximately(currentAspect, aspectRatio))
            {
                m_ValidResolutions.Add(i);
            }
            if (Screen.resolutions[i].height == Screen.currentResolution.height && Screen.resolutions[i].width == Screen.currentResolution.width)
            {
                currentResolutionIndex = i;
            }
        }

        m_ResolutionSlider.maxValue = 0.1f * (m_ValidResolutions.Count - 1);

        while (!m_ValidResolutions.Contains(currentResolutionIndex))
        {
            --currentResolutionIndex;

            if (currentResolutionIndex == 0)
                break;
        }
        if (currentResolutionIndex == 0)
        {
            while (!m_ValidResolutions.Contains(currentResolutionIndex))
            {
                ++currentResolutionIndex;
            }
        }

        SetScreenResolution(Screen.resolutions[currentResolutionIndex]);
    }

    private void OnGUI()
    {
        if (m_BrightnessSlider != null)
        {
            m_GammaCorrection = m_BrightnessSlider.value;
            RenderSettings.ambientLight = new Color(m_GammaCorrection, m_GammaCorrection, m_GammaCorrection, 1.0f);
        }

        if (m_QualitySlider != null)
        {
            //int index = (int)(m_QualitySlider.value * 10);
            QualitySettings.SetQualityLevel((int)(m_QualitySlider.value * 10));
            //GetComponent<GraphicsMenuBehaviour>().m_QualityText.text = GetQualityText((int)(m_QualitySlider.value * 10));
        }

        if (m_ResolutionSlider != null && m_ValidResolutions.Count != 0)
        {
            int index = (int)(m_ResolutionSlider.value * 10.0f);
            Resolution res = Screen.resolutions[m_ValidResolutions[index]];
            SetScreenResolution(res);
        }
    }

    string GetQualityText(int qualityLevel)
    {
        switch (qualityLevel)
        {
            case 0:
                return "Very Low";
            case 1:
                return "Low";
            case 2:
                return "Medium";
            case 3:
                return "High";
            case 4:
                return "Very High";
            case 5:
                return "Ultra";
            default:
                return "Quality Error";
        }
    }
}

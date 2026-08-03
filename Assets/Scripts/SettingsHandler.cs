using System;
using System.Collections;
using DG.Tweening;
using NUnit.Framework.Constraints;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class PauseEvent : UnityEvent<bool> { }
public class SettingsHandler : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject infoPanel;

    [Header("Sliders")]
    [SerializeField] private Slider maxTileSlider;
    [SerializeField] private Slider rowsSlider;
    [SerializeField] private Slider columnsSlider;
    [SerializeField] private Slider fpsSlider;
    [SerializeField] private Slider animSlider;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider bgMusicSlider;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI maxTileText;
    [SerializeField] private TextMeshProUGUI rowsText;
    [SerializeField] private TextMeshProUGUI columnsText;
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private TextMeshProUGUI animText;
    [SerializeField] private TextMeshProUGUI masterVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;
    [SerializeField] private TextMeshProUGUI bgMusicVolumeText;
    [SerializeField] private RectTransform fpsCounterRect;

    [Header("Toggles")]
    [SerializeField] private Toggle fpsCounterToggle;

    [Header("Other")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private PauseEvent pauseEvent;

    private const string mixerParameterNameMaster = "Master";
    private const string mixerParameterNameSFX = "SFX";
    private const string mixerParameterNameBgMusic = "BgMusic";

    void Awake()
    {
        QualitySettings.vSyncCount = 0; 
        Application.targetFrameRate = PlayerPrefs.GetInt("FPS", 60);
        float dbValue = Mathf.Log10(PlayerPrefs.GetFloat("MasterVolume", 1.0f)) * 20;
        audioMixer.SetFloat(mixerParameterNameMaster, dbValue);
        dbValue = Mathf.Log10(PlayerPrefs.GetFloat("SFXVolume", 1.0f)) * 20;
        audioMixer.SetFloat(mixerParameterNameSFX, dbValue);
        dbValue = Mathf.Log10(PlayerPrefs.GetFloat("BgMusicVolume", 1.0f)) * 20;
        audioMixer.SetFloat(mixerParameterNameBgMusic, dbValue);
        settingsPanel.SetActive(false);
        maxTileSlider.value = PlayerPrefs.GetInt("MaxTile", 2048);
        rowsSlider.value = PlayerPrefs.GetInt("Rows", 4);
        columnsSlider.value = PlayerPrefs.GetInt("Columns", 4);
        fpsSlider.value = PlayerPrefs.GetInt("FPS", 60);
        animSlider.value = PlayerPrefs.GetFloat("AnimDuration", 0.05f);
        masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
        bgMusicSlider.value = PlayerPrefs.GetFloat("BgMusicVolume", 1.0f);
        fpsCounterRect.gameObject.SetActive(PlayerPrefs.GetInt("FPSCounter", 0) != 1);
        fpsCounterToggle.isOn = PlayerPrefs.GetInt("FPSCounter", 0) != 1;
    }

    public void ShowSettingsPanel()
    {
        pauseEvent.Invoke(true);
        var rect = settingsPanel.GetComponent<RectTransform>();
        rect.localScale = Vector3.zero;
        settingsPanel.SetActive(true);
        rect.DOScale(new Vector3(1, 1, 1), 0.3f).SetEase(Ease.OutBack);
    }

    public void ShowInfoPanel()
    {
        var rect = infoPanel.GetComponent<RectTransform>();
        rect.localScale = Vector3.zero;
        infoPanel.SetActive(true);
        rect.DOScale(new Vector3(1, 1, 1), 0.3f).SetEase(Ease.OutBack);
    }

    public void HideSettingsPanel()
    {
        pauseEvent.Invoke(false);
        StartCoroutine(AnimateSettingsPnael());
    }

    public void HideInfoPanel()
    {
        StartCoroutine(AnimateInfoPanel());
    }

    IEnumerator AnimateSettingsPnael()
    {
        var rect = settingsPanel.GetComponent<RectTransform>();
        rect.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
        yield return new WaitForSeconds(0.35f);
        settingsPanel.SetActive(false);
    }

    IEnumerator AnimateInfoPanel()
    {
        var rect = infoPanel.GetComponent<RectTransform>();
        rect.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
        yield return new WaitForSeconds(0.35f);
        infoPanel.SetActive(false);
    }
    
    public void OnMaxTileSlider()
    {
        int value = Convert.ToInt32(RoundDownToPowerOfTwo(Convert.ToDouble(maxTileSlider.value)));
        maxTileText.text = value.ToString();
    }
    public void OnRowsSlider()
    {
        int value = Convert.ToInt32(rowsSlider.value);
        rowsText.text = value.ToString();
    }
    public void OnColumnsSlider()
    {
        int value = Convert.ToInt32(columnsSlider.value);
        columnsText.text = value.ToString();
    }
    public void OnFPSSlider()
    {
        int value = Convert.ToInt32(fpsSlider.value);
        fpsText.text = value.ToString();
    }
    public void OnAnimSlider()
    {
        if(animSlider.value == 0.05f)
        {
            float valueF = animSlider.value;
            animText.text = valueF.ToString();
        } else
        {
            double valueD = animSlider.value;
            valueD = Math.Round(valueD / 0.1) * 0.1;
            float valueF = (float)valueD;
            animText.text = valueF.ToString();
        }
    }

    public void OnMasterVolumeChanged()
    {
        float dbValue = Mathf.Log10(masterVolumeSlider.value) * 20;
        audioMixer.SetFloat(mixerParameterNameMaster, dbValue);

        masterVolumeText.text = $"{masterVolumeSlider.value:P0}";
    }

    public void OnSFXVolumeChanged()
    {
        float dbValue = Mathf.Log10(sfxVolumeSlider.value) * 20;
        audioMixer.SetFloat(mixerParameterNameSFX, dbValue);

        sfxVolumeText.text = $"{sfxVolumeSlider.value:P0}";
    }

    public void OnBGVolumeChanged()
    {
        float dbValue = Mathf.Log10(bgMusicSlider.value) * 20;
        audioMixer.SetFloat(mixerParameterNameBgMusic, dbValue);

        bgMusicVolumeText.text = $"{bgMusicSlider.value:P0}";
    }

    public void OnFPSCounterToggle(bool value)
    {
        PlayerPrefs.SetInt("FPSCounter", value ? 0 : 1);
        if(value)
        {
            fpsCounterRect.localScale = Vector3.zero;
            fpsCounterRect.gameObject.SetActive(true);
            fpsCounterRect.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack);
        } else
        {
            fpsCounterRect.DOScale(Vector3.zero, 0.35f).SetEase(Ease.InBack).OnComplete(() => fpsCounterRect.gameObject.SetActive(true));
        }
    }

    public void ApplySettings()
    {
        int value = Convert.ToInt32(RoundDownToPowerOfTwo(Convert.ToDouble(maxTileSlider.value)));
        PlayerPrefs.SetInt("MaxTile", value);
        value = Convert.ToInt32(rowsSlider.value);
        PlayerPrefs.SetInt("Rows", value);
        value = Convert.ToInt32(columnsSlider.value);
        PlayerPrefs.SetInt("Columns", value);
        value = Convert.ToInt32(fpsSlider.value);
        Application.targetFrameRate = value;
        PlayerPrefs.SetInt("FPS", value);
        if(animSlider.value == 0.05f) PlayerPrefs.SetFloat("AnimDuration", 0.05f);
        else
        {
            double valueD = animSlider.value;
            valueD = Math.Round(valueD / 0.1) * 0.1;
            float valueF = (float)valueD;
            PlayerPrefs.SetFloat("AnimDuration", valueF); 
        }
        PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
        PlayerPrefs.SetFloat("BgMusicVolume", bgMusicSlider.value);
        PlayerPrefs.Save();
    }
    double RoundDownToPowerOfTwo(double x)
    {
        if (x <= 0) return 1;
        return Math.Pow(2, Math.Floor(Math.Log(x, 2)));
    }
}

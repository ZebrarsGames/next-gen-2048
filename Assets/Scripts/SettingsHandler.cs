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
    [SerializeField] private Slider chanceOfFxTextSlider;
    [SerializeField] private Slider thresholdComboTextSlider;

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
    [SerializeField] private TextMeshProUGUI chanceOfFxTextText;
    [SerializeField] private TextMeshProUGUI thresholdComboTextText;

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
        
        int fps = PlayerPrefs.GetInt("FPS", 60);
        Application.targetFrameRate = fps;

        SetMixerVolume(mixerParameterNameMaster, PlayerPrefs.GetFloat("MasterVolume", 1.0f));
        SetMixerVolume(mixerParameterNameSFX, PlayerPrefs.GetFloat("SFXVolume", 1.0f));
        SetMixerVolume(mixerParameterNameBgMusic, PlayerPrefs.GetFloat("BgMusicVolume", 1.0f));
        
        settingsPanel.SetActive(false);

        maxTileSlider.value = PlayerPrefs.GetInt("MaxTile", 2048);
        rowsSlider.value = PlayerPrefs.GetInt("Rows", 4);
        columnsSlider.value = PlayerPrefs.GetInt("Columns", 4);
        fpsSlider.value = fps;
        animSlider.value = PlayerPrefs.GetFloat("AnimDuration", 0.1f);
        masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
        bgMusicSlider.value = PlayerPrefs.GetFloat("BgMusicVolume", 1.0f);
        
        bool isFpsCounterActive = PlayerPrefs.GetInt("FPSCounter", 0) != 1;
        fpsCounterRect.gameObject.SetActive(isFpsCounterActive);
        fpsCounterToggle.isOn = isFpsCounterActive;

        chanceOfFxTextSlider.value = PlayerPrefs.GetInt("ChanceOfFxText", 6);
        thresholdComboTextSlider.value = PlayerPrefs.GetInt("ThresholdComboText", 4);
        
        UpdateAllTextsVisually();
    }

    private void SetMixerVolume(string parameter, float linearVolume)
    {
        float clampedVolume = Mathf.Clamp(linearVolume, 0.0001f, 1.0f);
        float dbValue = Mathf.Log10(clampedVolume) * 20;
        audioMixer.SetFloat(parameter, dbValue);
    }

    public void ShowSettingsPanel() => AnimatePanel(settingsPanel, true);
    public void ShowInfoPanel() => AnimatePanel(infoPanel, true);
    public void HideSettingsPanel() => AnimatePanel(settingsPanel, false);
    public void HideInfoPanel() => AnimatePanel(infoPanel, false);

    private void AnimatePanel(GameObject panel, bool show)
    {
        var rect = panel.GetComponent<RectTransform>();
        if(show)
        {
            pauseEvent.Invoke(true);
            rect.localScale = Vector3.zero;
            panel.SetActive(true);
            rect.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }
        else
        {
            if(panel == settingsPanel) pauseEvent.Invoke(false);
            rect.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() => panel.SetActive(false));
        }
    }
    
    public void OnMaxTileSlider()
    {
        int value = (int)RoundDownToPowerOfTwo(maxTileSlider.value);
        maxTileText.SetText("{0}", value);
    }
    public void OnRowsSlider() => rowsText.SetText("{0}", (int)rowsSlider.value);
    public void OnColumnsSlider() => columnsText.SetText("{0}", (int)columnsSlider.value);
    public void OnFPSSlider() => fpsText.SetText("{0}", (int)fpsSlider.value);
    
    public void OnAnimSlider()
    {
        float value = animSlider.value == 0.05f ? 0.05f : Mathf.Round(animSlider.value / 0.1f) * 0.1f;
        animText.SetText("{0:2}", value);
    }

    public void OnMasterVolumeChanged()
    {
        SetMixerVolume(mixerParameterNameMaster, masterVolumeSlider.value);
        masterVolumeText.SetText("{0}%", Mathf.RoundToInt(masterVolumeSlider.value * 100));
    }

    public void OnSFXVolumeChanged()
    {
        SetMixerVolume(mixerParameterNameSFX, sfxVolumeSlider.value);
        sfxVolumeText.SetText("{0}%", Mathf.RoundToInt(sfxVolumeSlider.value * 100));
    }

    public void OnBGVolumeChanged()
    {
        SetMixerVolume(mixerParameterNameBgMusic, bgMusicSlider.value);
        bgMusicVolumeText.SetText("{0}%", Mathf.RoundToInt(bgMusicSlider.value * 100));
    }

    public void OnChanceOfFxTextChanged() => chanceOfFxTextText.SetText("{0}", (int)chanceOfFxTextSlider.value);
    public void OnThresholdComboTextChanged() => thresholdComboTextText.SetText("{0}", (int)thresholdComboTextSlider.value);

    public void OnFPSCounterToggle(bool value)
    {
        PlayerPrefs.SetInt("FPSCounter", value ? 0 : 1);
        if(value)
        {
            fpsCounterRect.localScale = Vector3.zero;
            fpsCounterRect.gameObject.SetActive(true);
            fpsCounterRect.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack);
        }
        else
        {
            fpsCounterRect.DOScale(Vector3.zero, 0.35f).SetEase(Ease.InBack).OnComplete(() => fpsCounterRect.gameObject.SetActive(false));
        }
    }

    public void ApplySettings()
    {
        PlayerPrefs.SetInt("MaxTile", (int)RoundDownToPowerOfTwo(maxTileSlider.value));
        PlayerPrefs.SetInt("Rows", (int)rowsSlider.value);
        PlayerPrefs.SetInt("Columns", (int)columnsSlider.value);
        
        int fpsValue = (int)fpsSlider.value;
        Application.targetFrameRate = fpsValue;
        PlayerPrefs.SetInt("FPS", fpsValue);

        PlayerPrefs.SetInt("ChanceOfFxText", (int)chanceOfFxTextSlider.value);
        PlayerPrefs.SetInt("ThresholdComboText", (int)thresholdComboTextSlider.value);

        float animDuration = animSlider.value == 0.05f ? 0.05f : Mathf.Round(animSlider.value / 0.1f) * 0.1f;
        PlayerPrefs.SetFloat("AnimDuration", animDuration);

        PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
        PlayerPrefs.SetFloat("BgMusicVolume", bgMusicSlider.value);

        PlayerPrefs.Save();
    }

    private int RoundDownToPowerOfTwo(float x)
    {
        if(x < 2) return 2;

        int val = (int)x;

        val |= val >> 1;
        val |= val >> 2;
        val |= val >> 4;
        val |= val >> 8;
        val |= val >> 16;

        return val - (val >> 1);
    }


    private void UpdateAllTextsVisually()
    {
        OnMaxTileSlider();
        OnRowsSlider();
        OnColumnsSlider();
        OnFPSSlider();
        OnAnimSlider();
        OnMasterVolumeChanged();
        OnSFXVolumeChanged();
        OnBGVolumeChanged();
        OnChanceOfFxTextChanged();
        OnThresholdComboTextChanged();
    }
}

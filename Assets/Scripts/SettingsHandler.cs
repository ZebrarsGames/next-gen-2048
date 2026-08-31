using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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

    private const string MixerParameterMaster = "Master";
    private const string MixerParameterSFX = "SFX";
    private const string MixerParameterBgMusic = "BgMusic";

    private RectTransform settingsPanelRect;
    private RectTransform infoPanelRect;

    private TweenCallback onSettingsHideComplete;
    private TweenCallback onInfoHideComplete;
    private TweenCallback onFpsCounterHideComplete;

    private void Awake()
    {
        DOTween.Init(true, true, LogBehaviour.ErrorsOnly).SetCapacity(200, 50);

        QualitySettings.vSyncCount = 0;

        if(settingsPanel != null)
        {
            settingsPanelRect = settingsPanel.GetComponent<RectTransform>();
            onSettingsHideComplete = OnSettingsHideFinished;
        }

        if(infoPanel != null)
        {
            infoPanelRect = infoPanel.GetComponent<RectTransform>();
            onInfoHideComplete = OnInfoHideFinished;
        }

        if(fpsCounterRect != null)
        {
            onFpsCounterHideComplete = OnFpsCounterHideFinished;
        }

        int fps = PlayerPrefs.GetInt("FPS", 60);
        if(fps >= 121)
        {
            Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
        }
        else
        {
            Application.targetFrameRate = fps;
        }

        SetMixerVolume(MixerParameterMaster, PlayerPrefs.GetFloat("MasterVolume", 1.0f));
        SetMixerVolume(MixerParameterSFX, PlayerPrefs.GetFloat("SFXVolume", 1.0f));
        SetMixerVolume(MixerParameterBgMusic, PlayerPrefs.GetFloat("BgMusicVolume", 1.0f));

        settingsPanel.SetActive(false);
        infoPanel.SetActive(false);

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
        float dbValue = Mathf.Log10(clampedVolume) * 20f;
        audioMixer.SetFloat(parameter, dbValue);
    }

    public void ShowSettingsPanel() => AnimatePanel(settingsPanel, settingsPanelRect, true, onSettingsHideComplete);
    public void ShowInfoPanel() => AnimatePanel(infoPanel, infoPanelRect, true, onInfoHideComplete);
    public void HideSettingsPanel() => AnimatePanel(settingsPanel, settingsPanelRect, false, onSettingsHideComplete);
    public void HideInfoPanel() => AnimatePanel(infoPanel, infoPanelRect, false, onInfoHideComplete);

    private void AnimatePanel(GameObject panel, RectTransform rect, bool show, TweenCallback onCompleteCallback)
    {
        if(panel == null || rect == null) return;

        rect.DOKill();

        if(show)
        {
            if(panel == settingsPanel) pauseEvent?.Invoke(true);
            rect.localScale = Vector3.zero;
            panel.SetActive(true);
            rect.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }
        else
        {
            if(panel == settingsPanel) pauseEvent?.Invoke(false);
            rect.DOScale(0f, 0.3f)
                .SetEase(Ease.InBack)
                .OnComplete(onCompleteCallback);
        }
    }

    private void OnSettingsHideFinished() => settingsPanel.SetActive(false);
    private void OnInfoHideFinished() => infoPanel.SetActive(false);

    public void OnRowsSlider() => rowsText.SetText("{0}", (int)rowsSlider.value);
    public void OnColumnsSlider() => columnsText.SetText("{0}", (int)columnsSlider.value);
    public void OnFPSSlider()
    {
        int fpsValue = (int)fpsSlider.value;

        if(fpsValue >= 121)
        {
            fpsText.SetText("Unlimited");
        }
        else
        {
            fpsText.SetText("{0}", fpsValue);
        }
    }

    public void OnMaxTileSlider()
    {
        int value = RoundDownToPowerOfTwo((int)maxTileSlider.value);
        maxTileText.SetText("{0}", value); 
    }

    public void OnAnimSlider()
    {
        float rawValue = animSlider.value;
        float value = rawValue == 0.05f ? 0.05f : Mathf.Round(rawValue * 10f) * 0.1f;
        animText.SetText("{0:0.00}", value);
    }

    public void OnMasterVolumeChanged()
    {
        float val = masterVolumeSlider.value;
        SetMixerVolume(MixerParameterMaster, val);
        masterVolumeText.SetText("{0}%", Mathf.RoundToInt(val * 100f));
    }

    public void OnSFXVolumeChanged()
    {
        float val = sfxVolumeSlider.value;
        SetMixerVolume(MixerParameterSFX, val);
        sfxVolumeText.SetText("{0}%", Mathf.RoundToInt(val * 100f));
    }

    public void OnBGVolumeChanged()
    {
        float val = bgMusicSlider.value;
        SetMixerVolume(MixerParameterBgMusic, val);
        bgMusicVolumeText.SetText("{0}%", Mathf.RoundToInt(val * 100f));
    }

    public void OnChanceOfFxTextChanged() => chanceOfFxTextText.SetText("{0}", (int)chanceOfFxTextSlider.value);
    public void OnThresholdComboTextChanged() => thresholdComboTextText.SetText("{0}", (int)thresholdComboTextSlider.value);

    public void OnFPSCounterToggle(bool value)
    {
        PlayerPrefs.SetInt("FPSCounter", value ? 0 : 1);
        fpsCounterRect.DOKill();

        if(value)
        {
            fpsCounterRect.localScale = Vector3.zero;
            fpsCounterRect.gameObject.SetActive(true);
            fpsCounterRect.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack);
        }
        else
        {
            fpsCounterRect.DOScale(Vector3.zero, 0.35f)
                .SetEase(Ease.InBack)
                .OnComplete(onFpsCounterHideComplete);
        }
    }

    private void OnFpsCounterHideFinished() => fpsCounterRect.gameObject.SetActive(false);

    public void ApplySettings()
    {
        PlayerPrefs.SetInt("MaxTile", RoundDownToPowerOfTwo((int)maxTileSlider.value));
        PlayerPrefs.SetInt("Rows", (int)rowsSlider.value);
        PlayerPrefs.SetInt("Columns", (int)columnsSlider.value);

        int fpsValue = (int)fpsSlider.value;
        if(fpsValue >= 121)
        {
            Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
        }
        else
        {
            Application.targetFrameRate = fpsValue;
        }

        PlayerPrefs.SetInt("ChanceOfFxText", (int)chanceOfFxTextSlider.value);
        PlayerPrefs.SetInt("ThresholdComboText", (int)thresholdComboTextSlider.value);

        float animRaw = animSlider.value;
        float animDuration = animRaw == 0.05f ? 0.05f : Mathf.Round(animRaw * 10f) * 0.1f;
        PlayerPrefs.SetFloat("AnimDuration", animDuration);

        PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
        PlayerPrefs.SetFloat("BgMusicVolume", bgMusicSlider.value);

        PlayerPrefs.Save();
    }

    private static int RoundDownToPowerOfTwo(int val)
    {
        if(val < 2) return 2;

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
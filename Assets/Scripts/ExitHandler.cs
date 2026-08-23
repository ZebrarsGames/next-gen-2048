using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class ExitHandler : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private GameObject exitPanel;

    [Header("Time Settings")]
    [SerializeField] private float doubleClickDelay = 0.5f;

    [Header("Dependencies")]
    [SerializeField] private SoundManager soundManager;

    [Header("Animation Settings")]
    [SerializeField] private float animDuration = 0.2f;

    private RectTransform _panelRect;
    private float _lastClickTime;
    private TweenCallback _onHideComplete;
    private Keyboard _keyboard;

    private void Awake()
    {
        if(exitPanel != null)
        {
            _panelRect = exitPanel.GetComponent<RectTransform>();
        }

        _onHideComplete = OnHidePanelAnimationComplete;
    }

    private void Start()
    {
        if(exitPanel != null)
        {
            exitPanel.SetActive(false);
        }
    }

    private void Update()
    {
        _keyboard = Keyboard.current;
        if(_keyboard == null) return;

        if(_keyboard.escapeKey.wasPressedThisFrame)
        {
            if(exitPanel.activeSelf)
            {
                HideExitPanel();
            }
            else
            {
                HandleBackButton();
            }
        }
    }

    private void HandleBackButton()
    {
        float currentTime = Time.time;
        if(currentTime - _lastClickTime < doubleClickDelay)
        {
            ShowExitPanel();
        }
        else
        {
            _lastClickTime = currentTime;
        }
    }

    public void ShowExitPanel()
    {
        if(soundManager != null)
        {
            soundManager.PlayWarningSound();
        }

        if(_panelRect == null) return;

        _panelRect.DOKill();

        _panelRect.localScale = Vector3.zero;
        exitPanel.SetActive(true);

        _panelRect.DOScale(Vector3.one, animDuration)
                  .SetEase(Ease.OutBack)
                  .SetUpdate(true);
    }

    public void HideExitPanel()
    {
        if(_panelRect == null) return;

        _panelRect.DOKill();

        _panelRect.DOScale(Vector3.zero, animDuration)
                  .SetEase(Ease.InBack)
                  .SetUpdate(true)
                  .OnComplete(_onHideComplete);
    }

    private void OnHidePanelAnimationComplete()
    {
        if(exitPanel != null)
        {
            exitPanel.SetActive(false);
        }
    }

    public void ConfirmExit()
    {
        PlayerPrefs.Save();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void CancelExit()
    {
        HideExitPanel();
    }

    private void OnDestroy()
    {
        if(_panelRect != null)
        {
            _panelRect.DOKill();
        }
    }
}
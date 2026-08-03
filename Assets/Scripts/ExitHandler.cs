using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using System.Collections;

public class ExitHandler : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject exitPanel;

    [Header("Time Settings")]
    [SerializeField] private float doubleClickDelay = 0.5f;

    [Header("Other")]
    [SerializeField] private SoundManager soundManager;

    private float lastClickTime = 0f;

    void Start()
    {
        exitPanel.SetActive(false);
    }
    void Update()
    {
        if(Keyboard.current.escapeKey.wasPressedThisFrame)
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
        if(Time.time - lastClickTime < doubleClickDelay)
        {
            ShowExitPanel();
        }
        else
        {
            lastClickTime = Time.time;
        }
    }
    public void ShowExitPanel()
    {
        soundManager.PlayWarningSound();
        var rect = exitPanel.GetComponent<RectTransform>();
        rect.localScale = Vector3.zero;
        exitPanel.SetActive(true);
        rect.DOScale(new Vector3(1, 1, 1), 0.2f).SetEase(Ease.OutBack);
    }

    public void HideExitPanel()
    {
        var rect = exitPanel.GetComponent<RectTransform>();
        rect.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() => exitPanel.SetActive(false));
    }

    public void ConfirmExit()
    {
        PlayerPrefs.Save();
        Application.Quit();
    }
    public void CancelExit()
    {
        HideExitPanel();
    }
}

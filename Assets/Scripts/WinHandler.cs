using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class WinHandler : MonoBehaviour
{
    [Header("UI elements")]
    [SerializeField] private GameObject winLosePanel;
    [SerializeField] private TextMeshProUGUI winLoseText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;

    [Header("References")]
    [SerializeField] private GameManager gameManager;

    private RectTransform winLoseRect;

    private void Awake()
    {
        if(winLosePanel != null)
        {
            winLoseRect = winLosePanel.GetComponent<RectTransform>();
        }
    }

    private void Start()
    {
        if(winLosePanel != null)
        {
            winLosePanel.SetActive(false);
        }
    }

    public void OnWinLose(GameState gameState)
    {
        winLoseText.text = gameState == GameState.Won ? "You Win!" : "You Lose!";

        int currentScore = gameManager.GetScore();
        int savedHighScore = SaveManager.GetPlayerData().highScore;

        if(currentScore > savedHighScore)
        {
            savedHighScore = currentScore;
            SaveManager.SaveSync(savedHighScore);
        }

        scoreText.SetText(gameManager.ScoreText.text);
        bestScoreText.SetText($"High score: {savedHighScore}");

        winLoseRect.DOKill();
        winLoseRect.localScale = Vector3.zero;
        winLosePanel.SetActive(true);
        winLoseRect.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
    }

    public void Restart()
    {
        StartCoroutine(HideWinPanelAndRestart());
    }

    private IEnumerator HideWinPanelAndRestart()
    {
        winLoseRect.DOKill();
        winLoseRect.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
        
        yield return new WaitForSeconds(0.3f);

        winLosePanel.SetActive(false);
        gameManager.RestartGame();
    }

    private void OnDestroy()
    {
        winLoseRect?.DOKill();
    }
}
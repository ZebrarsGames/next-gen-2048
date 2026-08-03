using System.Collections;
using DG.Tweening;
using UnityEngine;
using TMPro;

public class WinHandler : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject winLosePanel;
    [SerializeField] private TextMeshProUGUI winLoseText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;

    [Header("Other")]
    [SerializeField] private GameManager gameManager;

    private RectTransform winLoseRect;

    void Awake()
    {
        winLoseRect = winLosePanel.GetComponent<RectTransform>();
    }
    void Start()
    {
        winLosePanel.SetActive(false);
    }
    public void OnWinLose(GameState gameState)
    {
        if(gameState == GameState.Won) winLoseText.text = "You Win!";
        else if(gameState == GameState.Lose) winLoseText.text = "You Lose!";
        if(gameManager.GetScore() > SaveManager.GetPlayerData().highScore)
        {
            SaveManager.Save(gameManager.GetScore());
            scoreText.text = gameManager.ScoreText.text;
            bestScoreText.text = "High score: " + gameManager.GetScore();
        } else if(gameManager.GetScore() < SaveManager.GetPlayerData().highScore)
        {
            SaveManager.Save(SaveManager.GetPlayerData().highScore);
            scoreText.text = gameManager.ScoreText.text;
            bestScoreText.text = "High score: " + SaveManager.GetPlayerData().highScore;
        }
        winLoseRect.localScale = Vector3.zero;
        winLosePanel.SetActive(true);
        winLoseRect.DOScale(new Vector3(1, 1, 1), 1f).SetEase(Ease.OutBack);
    }

    public void Restart()
    {
        StartCoroutine(HideWinPanel());
        gameManager.RestartGame();
    }
    IEnumerator HideWinPanel()
    {
        winLoseRect.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
        yield return new WaitForSeconds(0.35f);
        winLosePanel.SetActive(false);
    }
}

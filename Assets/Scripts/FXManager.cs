using UnityEngine;
using TMPro;
using DG.Tweening;

public class FXManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI popupTextPrefab;
    
    [SerializeField] private Transform uiCanvasTransform; 

    private readonly string[] awesomePhrases = { "COOL!", "AMAZING!", "NEON BLAST!", "PERFECT!", "MEGA!", "INCREDIBLE!", "SUPER!", "LEGENDARY!", "ULTRA!" };
    private readonly Color[] phrasesColors = {Color.magenta, Color.lightCyan, Color.lightGreen, Color.lightPink, Color.orange, Color.aquamarine, Color.lightSkyBlue, Color.skyBlue};
    private int chanceOfFxText = 6;
    private int thresholdComboText = 4;

    public void SpawnPopupText(int comboCount)
    {
        if(popupTextPrefab == null || uiCanvasTransform == null) return;

        chanceOfFxText = PlayerPrefs.GetInt("ChanceOfFxText", 6);
        thresholdComboText = PlayerPrefs.GetInt("ThresholdComboText", 4);
        if(chanceOfFxText <= 1 || thresholdComboText <= 1) return; 

        if(comboCount < thresholdComboText)
        {
            int spawnChance = UnityEngine.Random.Range(1, chanceOfFxText); 
            if(spawnChance != 1) return;
        }

        TextMeshProUGUI newText = Instantiate(popupTextPrefab, uiCanvasTransform);

        newText.rectTransform.localPosition = Vector3.zero;

        if(comboCount >= 2)
        {
            newText.text = $"Combo x{comboCount}!";
            newText.color = Color.cyan;
        }
        else
        {
            int randomIndex = UnityEngine.Random.Range(0, awesomePhrases.Length);
            newText.text = awesomePhrases[randomIndex];
            newText.color = phrasesColors[UnityEngine.Random.Range(0, phrasesColors.Length)];
        }
        newText.transform.localScale = Vector3.zero;

        Sequence textSequence = DOTween.Sequence();

        textSequence
            .Append(newText.transform.DOScale(Vector3.one * 1.3f, 0.2f).SetEase(Ease.OutBack))
            .Join(newText.transform.DOLocalMoveY(150f, 0.6f).SetEase(Ease.OutQuad))
            .Join(newText.DOFade(0f, 0.4f).SetDelay(0.2f))
            .OnComplete(() => 
            {
                if(newText != null) Destroy(newText.gameObject);
            });
    }
}
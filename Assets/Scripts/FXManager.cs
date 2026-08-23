using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class FXManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI popupTextPrefab;
    [SerializeField] private Transform uiCanvasTransform;

    [Header("Pool Settings")]
    [SerializeField] private int initialPoolSize = 10;

    private readonly string[] awesomePhrases = { "COOL!", "AMAZING!", "NEON BLAST!", "PERFECT!", "MEGA!", "INCREDIBLE!", "SUPER!", "LEGENDARY!", "ULTRA!" };
    private readonly Color[] phrasesColors = { Color.magenta, Color.lightCyan, Color.lightGreen, Color.lightPink, Color.orange, Color.aquamarine, Color.lightSkyBlue, Color.skyBlue };

    private static readonly string[] comboStrings = new string[51];

    private readonly Queue<TextMeshProUGUI> pool = new Queue<TextMeshProUGUI>();

    private int chanceOfFxText;
    private int thresholdComboText;

    private void Awake()
    {
        if(comboStrings[1] == null)
        {
            for(int i = 1; i < comboStrings.Length; i++)
            {
                comboStrings[i] = $"Combo x{i}!";
            }
        }

        RefreshSettings();
        InitializePool();
    }

    public void RefreshSettings()
    {
        chanceOfFxText = PlayerPrefs.GetInt("ChanceOfFxText", 6);
        thresholdComboText = PlayerPrefs.GetInt("ThresholdComboText", 4);
    }

    private void InitializePool()
    {
        if(popupTextPrefab == null || uiCanvasTransform == null) return;

        for(int i = 0; i < initialPoolSize; i++)
        {
            TextMeshProUGUI textObj = Instantiate(popupTextPrefab, uiCanvasTransform);
            textObj.gameObject.SetActive(false);
            pool.Enqueue(textObj);
        }
    }

    public void SpawnPopupText(int comboCount)
    {
        if(popupTextPrefab == null || uiCanvasTransform == null) return;
        if(chanceOfFxText <= 1 || thresholdComboText <= 1) return;

        if(comboCount < thresholdComboText)
        {
            int spawnChance = Random.Range(1, chanceOfFxText);
            if(spawnChance != 1) return;
        }

        TextMeshProUGUI newText = GetFromPool();

        if(comboCount >= 2)
        {
            newText.text = comboCount < comboStrings.Length 
                ? comboStrings[comboCount] 
                : $"Combo x{comboCount}!";
            
            newText.color = Color.cyan;
        }
        else
        {
            int randomIndex = Random.Range(0, awesomePhrases.Length);
            newText.text = awesomePhrases[randomIndex];
            newText.color = phrasesColors[Random.Range(0, phrasesColors.Length)];
        }

        Transform textTransform = newText.transform;
        textTransform.localPosition = Vector3.zero;
        textTransform.localScale = Vector3.zero;

        Color targetColor = newText.color;
        targetColor.a = 1f;
        newText.color = targetColor;

        newText.gameObject.SetActive(true);

        textTransform.DOKill();
        newText.DOKill();

        Sequence textSequence = DOTween.Sequence();

        textSequence
            .Append(textTransform.DOScale(Vector3.one * 1.3f, 0.2f).SetEase(Ease.OutBack))
            .Join(textTransform.DOLocalMoveY(150f, 0.6f).SetEase(Ease.OutQuad))
            .Join(newText.DOFade(0f, 0.4f).SetDelay(0.2f))
            .OnComplete(() => ReturnToPool(newText));
    }

    private TextMeshProUGUI GetFromPool()
    {
        if(pool.Count > 0)
        {
            return pool.Dequeue();
        }

        TextMeshProUGUI newObj = Instantiate(popupTextPrefab, uiCanvasTransform);
        return newObj;
    }

    private void ReturnToPool(TextMeshProUGUI textObj)
    {
        if(textObj == null) return;

        textObj.gameObject.SetActive(false);
        pool.Enqueue(textObj);
    }

    private void OnDestroy()
    {
        pool.Clear();
    }
}
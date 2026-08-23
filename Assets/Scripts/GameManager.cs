using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 
using UnityEngine.Events;
using DG.Tweening;
using TMPro;

public enum GameState
{
    Playing,
    Won,
    Lose
}

[Serializable]
public class WinLoseEvent : UnityEvent<GameState> {}

public class GameManager : MonoBehaviour
{
    private GameState gameState = GameState.Playing;
    private ItemArray matrix;

    [Header("Prefabs & Assets")]
    public GameObject GO2, GO4, GO8, GO16, GO32, GO64, GO128, GO256, GO512, GO1024, GO2048, GO4096, GO8192, GO16384, blankGO;
    
    [Header("UI & Events")]
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI DebugText;
    public TextMeshProUGUI HighScoreText;
    public WinLoseEvent winLoseEvent;
    public UnityEvent duplicatedEvent;
    public SoundEvent onPlayBubbleSound;

    [Header("Settings & Managers")]
    public int maxTile = 2048;
    [SerializeField] private SoundManager soundManager;
    [SerializeField] private FXManager fxManager;
    [SerializeField] private float swipeThreshold = 50f;

    public IInputDetector inputDetector;

    private float distance = 0.109f;
    private int ZIndex = 0;
    private int score = 0;
    private float dynamicScale = 1f; 
    private float swipeThresholdSqr; 
    private Vector2 touchStartPos;
    private bool isSwiping = false;
    private bool isMoving = false; 
    private bool isWin = false;
    private bool isLose = false;
    private bool isPause = false;
    private Touchscreen activeTouchscreen;
    private int currentComboCount;
    private Camera mainCamera;

    private readonly List<GameObject> activeTilesPool = new List<GameObject>(16);
    private readonly List<GameObject> activeCellsPool = new List<GameObject>(16);
    private readonly List<GameObject> objectsToDestroyBuffer = new List<GameObject>(32);
    private readonly List<Item> loseAnimationItemsBuffer = new List<Item>(16);
    private readonly Dictionary<int, GameObject> prefabLookup = new Dictionary<int, GameObject>(16);

    private static readonly string ScoreFormat = "Score: {0}";
    private static readonly string HighScoreFormat = "High Score: {0}";

    public int GetScore() => score;

    private void Awake()
    {
        mainCamera = Camera.main;
        InitializePrefabLookup();
    }

    private void Start()
    {
        activeTouchscreen = Touchscreen.current;
        swipeThresholdSqr = swipeThreshold * swipeThreshold; 
        matrix = new ItemArray(Globals.Rows, Globals.Columns);

        inputDetector = GetComponent<ArrowKeysDetector>(); 
        if(Keyboard.current != null)
        {
            InputSystem.EnableDevice(Keyboard.current);
        }

        Initialize();
        InitialPositionBackgroundSprites();
    }

    private void InitializePrefabLookup()
    {
        prefabLookup[2] = GO2;
        prefabLookup[4] = GO4;
        prefabLookup[8] = GO8;
        prefabLookup[16] = GO16;
        prefabLookup[32] = GO32;
        prefabLookup[64] = GO64;
        prefabLookup[128] = GO128;
        prefabLookup[256] = GO256;
        prefabLookup[512] = GO512;
        prefabLookup[1024] = GO1024;
        prefabLookup[2048] = GO2048;
        prefabLookup[4096] = GO4096;
        prefabLookup[8192] = GO8192;
        prefabLookup[16384] = GO16384;
    }

    public void Initialize() 
    {
        CalculateDynamicScale();
        Globals.Apply();
        maxTile = PlayerPrefs.GetInt("MaxTile", 2048);

        ClearActiveTiles();

        matrix = new ItemArray(Globals.Rows, Globals.Columns); 
        
        CreateNewItem();
        CreateNewItem();

        score = 0;
        UpdateScore(0);
        gameState = GameState.Playing;
    }

    private void ClearActiveTiles()
    {
        for(int i = 0; i < activeTilesPool.Count; i++)
        {
            if(activeTilesPool[i] != null) Destroy(activeTilesPool[i]);
        }
        activeTilesPool.Clear();

        for(int i = 0; i < activeCellsPool.Count; i++)
        {
            if(activeCellsPool[i] != null) Destroy(activeCellsPool[i]);
        }
        activeCellsPool.Clear();
    }

    private void Update()
    {
        if(gameState != GameState.Playing || isPause || isMoving) return;

        if(activeTouchscreen == null)
        {
            activeTouchscreen = Touchscreen.current;
        }

        if(activeTouchscreen != null)
        {
            var touch = activeTouchscreen.primaryTouch;

            if(touch.press.wasPressedThisFrame)
            {
                touchStartPos = touch.position.ReadValue();
                isSwiping = true;
            }
            else if(isSwiping && touch.press.wasReleasedThisFrame)
            {
                Vector2 touchEndPos = touch.position.ReadValue();
                DetectSwipe(touchStartPos, touchEndPos);
                isSwiping = false;
            }
        }

        InputDirection? inputDir = inputDetector.DetectInputDirection();

        if(inputDir.HasValue)
        {
            ProcessMovement(inputDir.Value);
        }
    }

    private void ProcessMovement(InputDirection direction)
    {
        List<ItemMovementDetails> movementDetails = null;

        switch(direction)
        {
            case InputDirection.Left:
                movementDetails = matrix.MoveHorizontal(HorizontalMovement.Left);
                break;
            case InputDirection.Right:
                movementDetails = matrix.MoveHorizontal(HorizontalMovement.Right);
                break;
            case InputDirection.Up:
                movementDetails = matrix.MoveVertical(VerticalMovement.Up);
                break;
            case InputDirection.Down:
                movementDetails = matrix.MoveVertical(VerticalMovement.Down);
                break;
        }

        if(movementDetails != null && movementDetails.Count > 0)
        {
            bool hasMergeThisTurn = false;
            
            for(int i = 0; i < movementDetails.Count; i++)
            {
                if(movementDetails[i].GOToAnimateScale != null)
                {
                    hasMergeThisTurn = true;
                    break;
                }
            }

            if(hasMergeThisTurn) currentComboCount++;
            else currentComboCount = 0;

            ExecuteAnimationSequence(movementDetails);
        }
    }

    public void Move(int dir)
    {
        if(isMoving) return;
        
        InputDirection inputDir = dir switch
        {
            1 => InputDirection.Left,
            2 => InputDirection.Right,
            3 => InputDirection.Up,
            4 => InputDirection.Down,
            _ => InputDirection.Left
        };

        ProcessMovement(inputDir);
    }

    private void DetectSwipe(Vector2 start, Vector2 end)
    {
        Vector2 swipeVector = end - start;
        if(swipeVector.sqrMagnitude < swipeThresholdSqr) return;

        float absX = Mathf.Abs(swipeVector.x);
        float absY = Mathf.Abs(swipeVector.y);

        if(absX > absY)
        {
            Move(swipeVector.x > 0 ? 2 : 1);
        }
        else
        {
            Move(swipeVector.y > 0 ? 3 : 4);
        }
    }

    private void ExecuteAnimationSequence(List<ItemMovementDetails> movementDetails)
    {
        isMoving = true;
        objectsToDestroyBuffer.Clear();

        Sequence mainSeq = DOTween.Sequence();

        for(int i = 0; i < movementDetails.Count; i++)
        {
            var item = movementDetails[i];
            Vector3 newGoPosition = GetCellPosition(item.NewRow, item.NewColumn);

            if(item.GOToAnimatePosition != null)
                mainSeq.Join(item.GOToAnimatePosition.transform.DOLocalMove(newGoPosition, Globals.AnimationDuration));

            if(item.GOToAnimateScale != null)
            {
                mainSeq.Join(item.GOToAnimateScale.transform.DOLocalMove(newGoPosition, Globals.AnimationDuration));
                objectsToDestroyBuffer.Add(item.GOToAnimateScale);
                objectsToDestroyBuffer.Add(item.GOToAnimatePosition);

                Item duplicatedItem = matrix[item.NewRow, item.NewColumn];
                
                TypeOfSound soundType = duplicatedItem.Value switch
                {
                    <= 16 => TypeOfSound.Def,
                    <= 64 => TypeOfSound.Cool,
                    <= 256 => TypeOfSound.Cooler,
                    _ => TypeOfSound.Coolest
                };

                onPlayBubbleSound.Invoke(soundType);
                UpdateScore(duplicatedItem.Value);

                if(fxManager != null) fxManager.SpawnPopupText(currentComboCount);

                if(duplicatedItem.Value == maxTile)
                {
                    gameState = GameState.Won;
                    isWin = true;
                }
            }
        }

        mainSeq.AppendCallback(() =>
        {
            for(int i = 0; i < objectsToDestroyBuffer.Count; i++)
            {
                if(objectsToDestroyBuffer[i] != null)
                {
                    activeTilesPool.Remove(objectsToDestroyBuffer[i]);
                    Destroy(objectsToDestroyBuffer[i]);
                }
            }

            for(int i = 0; i < movementDetails.Count; i++)
            {
                var item = movementDetails[i];
                if(item.GOToAnimateScale != null)
                {
                    Vector3 newGoPosition = GetCellPosition(item.NewRow, item.NewColumn);
                    Item duplicatedItem = matrix[item.NewRow, item.NewColumn];

                    GameObject prefab = GetGOBasedOnValue(duplicatedItem.Value);
                    GameObject newGO = Instantiate(prefab, newGoPosition, Quaternion.identity);
                    activeTilesPool.Add(newGO);

                    newGO.transform.localScale = Vector3.one * 0.01f;
                    newGO.transform.DOScale(dynamicScale, Globals.AnimationDuration * 2f).SetEase(Ease.OutBack);

                    matrix[item.NewRow, item.NewColumn].GO = newGO;
                }
            }
        });

        mainSeq.AppendInterval(Globals.AnimationDuration * 0.5f);

        mainSeq.OnComplete(() =>
        {
            CreateNewItem();
            isLose = CheckIsLose();
            isMoving = false;

            if(isWin)
            {
                winLoseEvent.Invoke(GameState.Won);
                if (soundManager != null) soundManager.PlayWinSound();
                isWin = false;
            }
        });
    }

    private void CreateNewItem(int value = 2, int? row = null, int? column = null)
    {
        CalculateDynamicScale();
        int randomRow, randomColumn;

        if(row == null && column == null)
        {
            matrix.GetRandomRowColumn(out randomRow, out randomColumn);
        }
        else
        {
            randomRow = row.Value;
            randomColumn = column.Value;
        }

        GameObject prefab = GetGOBasedOnValue(value);
        GameObject newGO = Instantiate(prefab, GetCellPosition(randomRow, randomColumn), Quaternion.identity);
        activeTilesPool.Add(newGO);

        newGO.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        newGO.transform.DOScale(new Vector3(dynamicScale, dynamicScale, dynamicScale), Globals.AnimationDuration);

        Item newItem = new Item(value, randomRow, randomColumn, newGO);
        matrix[randomRow, randomColumn] = newItem;
    }

    private void InitialPositionBackgroundSprites()
    {
        CalculateDynamicScale();
        for(int row = 0; row < Globals.Rows; row++)
        {
            for(int column = 0; column < Globals.Columns; column++)
            {
                GameObject cell = Instantiate(blankGO, GetCellPosition(row, column), Quaternion.identity);
                cell.transform.localScale = new Vector3(dynamicScale, dynamicScale, 1f);
                activeCellsPool.Add(cell);
            }
        }
    }

    private void UpdateScore(int toAdd)
    {
        score += toAdd;
        
        if(ScoreText != null)
            ScoreText.SetText(ScoreFormat, score);

        if(HighScoreText != null)
            HighScoreText.SetText(HighScoreFormat, SaveManager.GetPlayerData().highScore);
    }

    private GameObject GetGOBasedOnValue(int value)
    {
        if(prefabLookup.TryGetValue(value, out GameObject prefab))
        {
            return prefab;
        }
        
        Debug.LogError($"[GameManager] Prefab for value {value} not found!");
        return GO2;
    }

    public void RestartGame()
    {
        isWin = false;
        isLose = false;
        matrix = new ItemArray(Globals.Rows, Globals.Columns);
        currentComboCount = 0;
        Initialize();
        InitialPositionBackgroundSprites();
    }

    public void Pause(bool pause) => isPause = pause;

    private Vector3 GetCellPosition(int row, int column)
    {
        float baseStep = 1f + distance; 
        float scaledStep = baseStep * dynamicScale; 

        float offsetX = (Globals.Columns - 1) * scaledStep / 2f;
        float offsetY = (Globals.Rows - 1) * scaledStep / 2f;

        float x = (column * scaledStep) - offsetX;
        float y = (row * scaledStep) - offsetY;

        return transform.position + new Vector3(x, y, ZIndex);
    }

    private void CalculateDynamicScale()
    {
        if(mainCamera == null) mainCamera = Camera.main;

        float screenHeight = mainCamera.orthographicSize * 2f;
        float screenWidth = screenHeight * mainCamera.aspect;

        float padding = 0.9f; 
        float availableWidth = screenWidth * padding;
        float availableHeight = screenHeight * padding - 4.3f;

        float baseStep = 1f + distance;
        float baseGridWidth = Globals.Columns * baseStep;
        float baseGridHeight = Globals.Rows * baseStep;

        float scaleX = availableWidth / baseGridWidth;
        float scaleY = availableHeight / baseGridHeight;

        dynamicScale = Mathf.Min(scaleX, scaleY);
        dynamicScale = Mathf.Min(dynamicScale, 1f); 
    }

    private bool CheckIsLose()
    {
        for(int i = 0; i < Globals.Rows; i++)
        {
            for(int k = 0; k < Globals.Columns; k++)
            {
                if(matrix[i, k] == null) return false;
            }
        }

        for(int i = 0; i < Globals.Rows; i++)
        {
            for(int k = 0; k < Globals.Columns; k++)
            {
                if(k + 1 < Globals.Columns && matrix[i, k].Value == matrix[i, k + 1].Value) 
                    return false;

                if(i + 1 < Globals.Rows && matrix[i, k].Value == matrix[i + 1, k].Value) 
                    return false;
            }
        }

        gameState = GameState.Lose;
        AnimateLose();
        return true;
    }

    private void AnimateLose()
    {
        loseAnimationItemsBuffer.Clear();
        if(soundManager != null) soundManager.PlayLoseSound();
        
        for(int i = 0; i < Globals.Rows; i++)
        {
            for(int k = 0; k < Globals.Columns; k++)
            {
                if(matrix[i, k] != null) loseAnimationItemsBuffer.Add(matrix[i, k]);
            }
        }

        for(int i = loseAnimationItemsBuffer.Count - 1; i > 0; i--)
        {
            int rnd = UnityEngine.Random.Range(0, i + 1);
            Item temp = loseAnimationItemsBuffer[i];
            loseAnimationItemsBuffer[i] = loseAnimationItemsBuffer[rnd];
            loseAnimationItemsBuffer[rnd] = temp;
        }

        float shrinkDuration = 0.35f;   
        float delayBetweenTiles = 0.1f; 

        Sequence mainLoseSequence = DOTween.Sequence();

        for(int i = 0; i < loseAnimationItemsBuffer.Count; i++)
        {
            Item item = loseAnimationItemsBuffer[i];
            if(item?.GO == null) continue;

            Transform itemTransform = item.GO.transform;
            float currentDelay = i * delayBetweenTiles; 

            Sequence tileSequence = DOTween.Sequence();
            tileSequence.AppendInterval(currentDelay) 
                .Append(itemTransform.DOScale(itemTransform.localScale * 1.15f, shrinkDuration * 0.3f).SetEase(Ease.OutQuad))
                .Append(itemTransform.DOScale(Vector3.zero, shrinkDuration * 0.7f).SetEase(Ease.InQuad));

            mainLoseSequence.Insert(0, tileSequence);
        }

        mainLoseSequence.OnComplete(() =>
        {
            winLoseEvent.Invoke(GameState.Lose);
        });
    }
}
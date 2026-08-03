using UnityEngine;
using UnityEngine.InputSystem; 
using UnityEngine.Events;
using System.Collections;
using Assets.Scripts;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

public enum GameState
{
    Playing,
    Won,
    Lose
}
[System.Serializable]
public class WinLoseEvent : UnityEvent<GameState> {}

public class GameManager : MonoBehaviour
{
    private GameState gameState = GameState.Playing;
    ItemArray matrix;
    public GameObject GO2, GO4, GO8, GO16, GO32, GO64, GO128, GO256, GO512, GO1024, GO2048, GO4096, GO8192, GO16384, blankGO;
    public TextMeshProUGUI ScoreText, DebugText, HighScoreText;
    public WinLoseEvent winLoseEvent;
    public UnityEvent duplicatedEvent;
    public SoundEvent onPlayBubbleSound;
    public int maxTile = 2048;
    [SerializeField] private SoundManager soundManager;
    [SerializeField] private FXManager fxManager;
    private float distance = 0.109f;

    public IInputDetector inputDetector;

    private int ZIndex = 0, score = 0;
    public int GetScore() => score;

    [SerializeField]
    private float swipeThreshold = 50f;

    private Vector2 touchStartPos;
    private bool isSwiping = false;
    private bool isMoving = false; 
    private bool isWin = false;
    private bool isLose = false;
    private bool isPause = false;
    private float dynamicScale = 1f; 
    private float swipeThresholdSqr; 
    private Touchscreen activeTouchscreen;
    private int currentComboCount;

    //will read a file from Resources folder
    //and create the matrix with the preloaded data
    void InitArrayWithPremadeData()
    {
        string[,] sampleArray = Utilities.GetMatrixFromResourcesData();
        for (int row = 0; row < Globals.Rows; row++)
        {
            for (int column = 0; column < Globals.Columns; column++)
            {
                int value;
                if (int.TryParse(sampleArray[Globals.Rows - 1 - row, column], out value))
                {
                    CreateNewItem(value, row, column);
                }
            }
        }
    }

    void Start()
    {
        activeTouchscreen = Touchscreen.current;
        swipeThresholdSqr = swipeThreshold * swipeThreshold; 
        matrix = new ItemArray(Globals.Rows, Globals.Columns);
        Initialize();
        InitialPositionBackgroundSprites();

        inputDetector = GetComponent<ArrowKeysDetector>(); 
        if (Keyboard.current != null)
        {
            InputSystem.EnableDevice(Keyboard.current);
        }

        string x = Utilities.ShowMatrixOnConsole(matrix);
        DebugDisplay(x);
    }

    public void Initialize() 
    {
        CalculateDynamicScale();
        Globals.Apply();
        maxTile = PlayerPrefs.GetInt("MaxTile", 2048);

        GameObject[] oldTiles = GameObject.FindGameObjectsWithTag("Tile");
        foreach (GameObject tile in oldTiles)
        {
            Destroy(tile);
        }

        GameObject[] oldCells = GameObject.FindGameObjectsWithTag("GridCell");
        foreach (GameObject cell in oldCells)
        {
            Destroy(cell);
        }

        matrix = new ItemArray(Globals.Rows, Globals.Columns); 
        
        CreateNewItem();
        CreateNewItem();

        score = 0;
        UpdateScore(0);
        gameState = GameState.Playing;
    }


    void DebugDisplay(string content)
    {
        DebugText.text = content;
    }

    private void CreateNewItem(int value = 2, int? row = null, int? column = null)
    {
        CalculateDynamicScale();
        int randomRow, randomColumn;

        if (row == null && column == null)
        {
            matrix.GetRandomRowColumn(out randomRow, out randomColumn);
        }
        else
        {
            randomRow = row.Value;
            randomColumn = column.Value;
        }

        var newItem = new Item();
        newItem.Row = randomRow;
        newItem.Column = randomColumn;
        newItem.Value = value;

        GameObject newGo = GetGOBasedOnValue(value);
        newGo.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        newItem.GO = Instantiate(newGo, GetCellPosition(randomRow, randomColumn), Quaternion.identity) as GameObject;

        newItem.GO.transform.DOScale(new Vector3(dynamicScale, dynamicScale, dynamicScale), Globals.AnimationDuration);

        matrix[randomRow, randomColumn] = newItem;
    }

    private void InitialPositionBackgroundSprites()
    {
        CalculateDynamicScale();
        for (int row = 0; row < Globals.Rows; row++)
        {
            for (int column = 0; column < Globals.Columns; column++)
            {
                GameObject cell = Instantiate(blankGO, GetCellPosition(row, column), Quaternion.identity);
                cell.transform.localScale = new Vector3(dynamicScale, dynamicScale, 1f);
            }
        }
    }


    void Update()
    {
        if(gameState == GameState.Playing)
        {
            if(isPause || isMoving) return;

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

            InputDirection? value = inputDetector.DetectInputDirection();

            if(value.HasValue)
            {
                List<ItemMovementDetails> movementDetails = null;

                if(value == InputDirection.Left)
                    movementDetails = matrix.MoveHorizontal(HorizontalMovement.Left);
                else if(value == InputDirection.Right)
                    movementDetails = matrix.MoveHorizontal(HorizontalMovement.Right);
                else if(value == InputDirection.Top)
                    movementDetails = matrix.MoveVertical(VerticalMovement.Top);
                else if(value == InputDirection.Bottom)
                    movementDetails = matrix.MoveVertical(VerticalMovement.Bottom);

                if (movementDetails != null && movementDetails.Count > 0)
                {
                    bool hasMergeThisTurn = false;
                        
                    foreach(var detail in movementDetails)
                    {
                        if(detail.GOToAnimateScale != null)
                        {
                            hasMergeThisTurn = true;
                            break;
                        }
                    }

                    if(hasMergeThisTurn)
                    {
                         currentComboCount++;
                    }
                    else
                    {
                        currentComboCount = 0;
                    }

                    StartCoroutine(AnimateItemsRoutine(movementDetails));
                }
                
                #if UNITY_EDITOR
                string x = Utilities.ShowMatrixOnConsole(matrix);
                DebugDisplay(x);
                #endif
            }
        } 
        else if(gameState == GameState.Won)
        {
            if(isWin)
            {
                winLoseEvent.Invoke(GameState.Won);
                soundManager.PlayWinSound();
                isWin = false;
            }
        } 
        else if(gameState == GameState.Lose)
        {
            if(isLose)
            {
                soundManager.PlayLoseSound();
                isLose = false;
            }
        }
    }


    private IEnumerator AnimateItemsRoutine(List<ItemMovementDetails> details)
    {
        isMoving = true;

        yield return StartCoroutine(AnimateItems(details)); 
        
        isLose = CheckIsLose();
        isMoving = false;
    }

    public void Move(int dir)
    {
        if(isMoving) return;
        List<ItemMovementDetails> movementDetails = new List<ItemMovementDetails>();
        switch(dir)
        {
            case 1:
            movementDetails = matrix.MoveHorizontal(HorizontalMovement.Left);
                break;
            case 2:
            movementDetails = matrix.MoveHorizontal(HorizontalMovement.Right);
                break;
            case 3:
            movementDetails = matrix.MoveVertical(VerticalMovement.Top);
                break;
            case 4:
            movementDetails = matrix.MoveVertical(VerticalMovement.Bottom);
                break;
        }
        if(movementDetails.Count > 0)
                {
                    StartCoroutine(AnimateItemsRoutine(movementDetails));
                }
                string x = Utilities.ShowMatrixOnConsole(matrix);
                DebugDisplay(x);
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


    IEnumerator AnimateItems(IEnumerable<ItemMovementDetails> movementDetails)
    {
        List<GameObject> objectsToDestroy = new List<GameObject>();

        foreach(var item in movementDetails)
        {
            var newGoPosition = GetCellPosition(item.NewRow, item.NewColumn);

            item.GOToAnimatePosition.transform.DOLocalMove(newGoPosition, Globals.AnimationDuration);

            if(item.GOToAnimateScale != null)
            {
                item.GOToAnimateScale.transform.DOLocalMove(newGoPosition, Globals.AnimationDuration);

                objectsToDestroy.Add(item.GOToAnimateScale);
                objectsToDestroy.Add(item.GOToAnimatePosition);
                
                var duplicatedItem = matrix[item.NewRow, item.NewColumn];
                
                TypeOfSound soundType = TypeOfSound.coolest;
                if(duplicatedItem.Value <= 16) soundType = TypeOfSound.def;
                else if(duplicatedItem.Value <= 64) soundType = TypeOfSound.cool;
                else if(duplicatedItem.Value <= 256) soundType = TypeOfSound.cooler;

                onPlayBubbleSound.Invoke(soundType);
                UpdateScore(duplicatedItem.Value);

                if(fxManager != null)
                {
                    fxManager.SpawnPopupText(currentComboCount);
                }

                if(duplicatedItem.Value == maxTile)
                {
                    gameState = GameState.Won;
                    isWin = true;
                }
            }
        }

        yield return new WaitForSeconds(Globals.AnimationDuration);

        foreach(var go in objectsToDestroy)
        {
            if(go != null) Destroy(go);
        }

        foreach(var item in movementDetails)
        {
            if(item.GOToAnimateScale != null)
            {
                var newGoPosition = GetCellPosition(item.NewRow, item.NewColumn);
                var duplicatedItem = matrix[item.NewRow, item.NewColumn];

                GameObject prefab = GetGOBasedOnValue(duplicatedItem.Value);
                var newGO = Instantiate(prefab, newGoPosition, Quaternion.identity) as GameObject;

                newGO.transform.localScale = Vector3.one * 0.01f;
                
                newGO.transform.DOScale(dynamicScale, Globals.AnimationDuration * 2f).SetEase(Ease.OutBack);

                matrix[item.NewRow, item.NewColumn].GO = newGO;
            }
        }

        yield return new WaitForSeconds(Globals.AnimationDuration * 0.5f);

        CreateNewItem(); 
    }

    private void UpdateScore(int toAdd)
    {
        score += toAdd;
        ScoreText.text = "Score: " + score;
        HighScoreText.text = "High Score: " + SaveManager.GetPlayerData().highScore;
    }

    private GameObject GetGOBasedOnValue(int value)
    {
        GameObject newGo = null;
        switch (value)
        {
            case 2: newGo = GO2; break;
            case 4: newGo = GO4; break;
            case 8: newGo = GO8; break;
            case 16: newGo = GO16; break;
            case 32: newGo = GO32; break;
            case 64: newGo = GO64; break;
            case 128: newGo = GO128; break;
            case 256: newGo = GO256; break;
            case 512: newGo = GO512; break;
            case 1024: newGo = GO1024; break;
            case 2048: newGo = GO2048; break;
            case 4096: newGo = GO4096; break;
            case 8192: newGo = GO8192; break;
            case 16384: newGo = GO16384; break;
            default:
                throw new System.Exception("Uknown value:" + value);
        }
        return newGo;
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
    public void Pause(bool pause)
    {
        isPause = pause;
    }
    private Vector3 GetCellPosition(int row, int column)
    {
        float baseStep = 1f + distance; 
        float scaledStep = baseStep * dynamicScale; 

        float offsetX = (Globals.Columns - 1) * scaledStep / 2f;
        float offsetY = (Globals.Rows - 1) * scaledStep / 2f;

        float x = (column * scaledStep) - offsetX;
        float y = (row * scaledStep) - offsetY;

        return this.transform.position + new Vector3(x, y, ZIndex);
    }


    private void CalculateDynamicScale()
    {
        float screenHeight = Camera.main.orthographicSize * 2f;
        float screenWidth = screenHeight * Camera.main.aspect;

        float padding = 0.9f; 
        float availableWidth = screenWidth * padding;
        float availableHeight = screenHeight * padding;

        availableHeight -= 4.3f;

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
        List<Item> allItems = new List<Item>();
        
        for(int i = 0; i < Globals.Rows; i++)
        {
            for(int k = 0; k < Globals.Columns; k++)
            {
                if(matrix[i, k] != null) allItems.Add(matrix[i, k]);
            }
        }

        for(int i = allItems.Count - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            Item temp = allItems[i];
            allItems[i] = allItems[rnd];
            allItems[rnd] = temp;
        }

        float shrinkDuration = 0.35f;   
        float delayBetweenTiles = 0.1f; 

        Sequence mainLoseSequence = DOTween.Sequence();

        for(int i = 0; i < allItems.Count; i++)
        {
            Item item = allItems[i];
            if(item == null) continue;

            Transform itemTransform = item.GO.transform;
            float currentDelay = i * delayBetweenTiles; 

            Sequence tileSequence = DOTween.Sequence();

            tileSequence.AppendInterval(currentDelay) 
                .Append(itemTransform.DOScale(itemTransform.localScale * 1.15f, shrinkDuration * 0.3f).SetEase(Ease.OutQuad))
                .Append(itemTransform.DOScale(Vector3.zero, shrinkDuration * 0.7f).SetEase(Ease.InQuad))
                .OnComplete(() => 
                {
                    if(item != null && item.GO.gameObject != null)
                    {
                        Destroy(item.GO.gameObject);
                    }
                });

            mainLoseSequence.Insert(0, tileSequence);
        }

        mainLoseSequence.OnComplete(() =>
        {
            winLoseEvent.Invoke(GameState.Lose);
        });
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Snake : MonoBehaviour
{
    [Header("Grille")]
    public Collider2D gridArea;
    public int gridWidth = 17;
    public int gridHeight = 15;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highscoreText;
    
    private int score;
    private int highscore;

    private Vector2Int direction = Vector2Int.right;
    private Vector2Int headGridPos;

    [SerializeField] private float moveDelay = 0.25f;
    private float timer;

    [Header("Sprites tête")]
    public Sprite headRight; public Sprite headUp; public Sprite headLeft; public Sprite headDown;

    [Header("Sprites corps")]
    public Sprite bodyHorizontal; public Sprite bodyVertical; public Sprite bodyTopLeft;
    public Sprite bodyTopRight; public Sprite bodyBottomLeft; public Sprite bodyBottomRight;

    [Header("Sprites queue")]
    public Sprite tailUp; public Sprite tailDown; public Sprite tailLeft; public Sprite tailRight;

    [Header("Prefab")]
    public GameObject segmentPrefab;

    private SpriteRenderer headRenderer;
    private List<Transform> segments = new List<Transform>();
    private List<Vector2Int> gridPositions = new List<Vector2Int>();

    private bool activeBonus;
    public float bonusTime = 15.0f;

    private Queue<Vector2Int> inputBuffer = new Queue<Vector2Int>();
    private const int maxBufferSize = 2;

    public GameOverScreen GameOverScreen;

    private bool hasStarted = false;


    void Start()
    {
        headRenderer = GetComponent<SpriteRenderer>();
        headRenderer.sortingOrder = 10;
        score = 0;
        activeBonus = false;

        highscore = SaveManager.LoadData().highscore;

        UpdateScoreUI();
        InitSnake();
    }

    void InitSnake()
    {
        segments.Clear();
        gridPositions.Clear();
        headGridPos = new Vector2Int(gridWidth / 2, gridHeight / 2);
        gridPositions.Add(headGridPos);
        segments.Add(transform);
        transform.position = GridToWorld(headGridPos);
        UpdateHeadSprite();

        int initialBodyLength = 1;
        for (int i = 1; i <= initialBodyLength + 1; i++)
        {
            Vector2Int pos = headGridPos - direction * i;
            gridPositions.Add(pos);
            Sprite sprite = (i == initialBodyLength + 1) ? GetTailSprite() : (direction.x != 0 ? bodyHorizontal : bodyVertical);
            Transform segment = CreateSegment(GridToWorld(pos), sprite);
            segment.GetComponent<SpriteRenderer>().sortingOrder = 9;
            segments.Add(segment);
        }
    }

    Transform CreateSegment(Vector3 position, Sprite sprite)
    {
        GameObject segment = Instantiate(segmentPrefab, position, Quaternion.identity);
        segment.GetComponent<SpriteRenderer>().sprite = sprite;
        return segment.transform;
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        Vector2Int lastQueuedDir = inputBuffer.Count > 0
            ? inputBuffer.ToArray()[inputBuffer.Count - 1]
            : direction;

        bool inputDetected = false;

        if ((keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
            && lastQueuedDir != Vector2Int.down && inputBuffer.Count < maxBufferSize)
        {
            inputBuffer.Enqueue(Vector2Int.up);
            inputDetected = true;
        }
        else if ((keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame)
            && lastQueuedDir != Vector2Int.up && inputBuffer.Count < maxBufferSize)
        {
            inputBuffer.Enqueue(Vector2Int.down);
            inputDetected = true;
        }
        else if ((keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
            && lastQueuedDir != Vector2Int.right && inputBuffer.Count < maxBufferSize)
        {
            inputBuffer.Enqueue(Vector2Int.left);
            inputDetected = true;
        }
        else if ((keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
            && lastQueuedDir != Vector2Int.left && inputBuffer.Count < maxBufferSize)
        {
            inputBuffer.Enqueue(Vector2Int.right);
            inputDetected = true;
        }

        if (inputDetected)
        {
            hasStarted = true;
        }

        if (activeBonus)
        {
            bonusTime -= Time.deltaTime;

            if (bonusTime <= 0.0f)
            {
                timerEnded();
            }
        }

        UpdateHeadSprite();
    }

    void FixedUpdate()
    {
        if (!hasStarted) return;

        timer += Time.fixedDeltaTime;
        if (timer >= moveDelay)
        {
            Move();
            timer = 0f;
        }
    }


    void Move()
    {
        if (inputBuffer.Count > 0)
        {
            direction = inputBuffer.Dequeue();
        }

        Vector2Int newHeadPos = headGridPos + direction;
        if (newHeadPos.x < 0 || newHeadPos.x >= gridWidth || newHeadPos.y < 0 || newHeadPos.y >= gridHeight) { Die("mur"); return; }
        if (gridPositions.Contains(newHeadPos)) { Die("soi-même"); return; }

        gridPositions.Insert(0, newHeadPos);
        gridPositions.RemoveAt(gridPositions.Count - 1);
        headGridPos = newHeadPos;

        for (int i = 0; i < segments.Count; i++) segments[i].position = GridToWorld(gridPositions[i]);
        UpdateBodySprites();
    }

    Vector3 GridToWorld(Vector2Int gridPos)
    {
        Bounds bounds = gridArea.bounds;
        float cellWidth = bounds.size.x / gridWidth;
        float cellHeight = bounds.size.y / gridHeight;
        float x = bounds.min.x + cellWidth * (gridPos.x + 0.5f);
        float y = bounds.min.y + cellHeight * (gridPos.y + 0.5f);
        return new Vector3(x, y, 0f);
    }

    void UpdateHeadSprite()
    {
        if (direction == Vector2Int.right) headRenderer.sprite = headRight;
        else if (direction == Vector2Int.left) headRenderer.sprite = headLeft;
        else if (direction == Vector2Int.up) headRenderer.sprite = headUp;
        else if (direction == Vector2Int.down) headRenderer.sprite = headDown;
    }

    Sprite GetTailSprite()
    {
        if (direction == Vector2Int.right) return tailLeft;
        if (direction == Vector2Int.left) return tailRight;
        if (direction == Vector2Int.up) return tailDown;
        return tailUp;
    }

    void UpdateBodySprites()
    {
        // 1. Gérer uniquement le CORPS (on s'arrête strictement AVANT le dernier segment)
        for (int i = 1; i < segments.Count - 1; i++)
        {
            SpriteRenderer sr = segments[i].GetComponent<SpriteRenderer>();
            Vector2Int prev = gridPositions[i - 1];
            Vector2Int curr = gridPositions[i];
            Vector2Int next = gridPositions[i + 1];

            Vector2Int dirPrev = curr - prev;
            Vector2Int dirNext = next - curr;

            if (dirPrev.x != 0 && dirNext.x != 0) sr.sprite = bodyHorizontal;
            else if (dirPrev.y != 0 && dirNext.y != 0) sr.sprite = bodyVertical;
            else
            {
                // Logique des virages
                if ((dirPrev.x == 1 && dirNext.y == 1) || (dirPrev.y == -1 && dirNext.x == -1)) sr.sprite = bodyTopLeft;
                else if ((dirPrev.x == -1 && dirNext.y == 1) || (dirPrev.y == -1 && dirNext.x == 1)) sr.sprite = bodyTopRight;
                else if ((dirPrev.x == 1 && dirNext.y == -1) || (dirPrev.y == 1 && dirNext.x == -1)) sr.sprite = bodyBottomLeft;
                else sr.sprite = bodyBottomRight;
            }
        }

        // 2. Gérer uniquement la QUEUE (on force le sprite de queue sur le dernier élément)
        int lastIndex = segments.Count - 1;
        SpriteRenderer tailSR = segments[lastIndex].GetComponent<SpriteRenderer>();

        // Calcul de direction entre l'avant-dernier et le dernier
        Vector2Int diff = gridPositions[lastIndex - 1] - gridPositions[lastIndex];

        if (diff.x == 1) tailSR.sprite = tailLeft;
        else if (diff.x == -1) tailSR.sprite = tailRight;
        else if (diff.y == 1) tailSR.sprite = tailDown;
        else if (diff.y == -1) tailSR.sprite = tailUp;
    }

    public bool Occupies(int x, int y)
    {
        return gridPositions.Contains(new Vector2Int(x, y));
    }

    void Die(string reason)
    {
        Debug.Log("GAME OVER (" + reason + ")");
        SaveManager.SaveScore(score);       
        Time.timeScale = 0f;
        GameOverScreen.Setup(score, highscore);
    }

    private void Grow()
    {
        Vector2Int lastGridPos = gridPositions[gridPositions.Count - 1];
        Sprite currentTailSprite = segments[segments.Count - 1].GetComponent<SpriteRenderer>().sprite;
        Transform newSegment = CreateSegment(GridToWorld(lastGridPos), currentTailSprite);

        newSegment.GetComponent<SpriteRenderer>().sortingOrder = 9;
        segments.Add(newSegment);
        gridPositions.Add(lastGridPos);

        score += 1;
        UpdateScoreUI();
    }

    private void OnTriggerEnter2D(Collider2D other) { if (other.CompareTag("Food")) Grow(); }

    void UpdateScoreUI() 
    {
        if (scoreText != null) scoreText.text = "Score: " + score;
        if (highscoreText != null) highscoreText.text = "Best: " + highscore;
    }

    public void IncreaseSpeed()
    {
        activeBonus = true;
        moveDelay = 0.10f;
    }

    public void DecreaseSpeed()
    {
        activeBonus = true;
        moveDelay = 0.5f;
    }

    void timerEnded()
    {
        activeBonus = false;
        bonusTime = 10f;
        moveDelay = 0.25f;
    }
}
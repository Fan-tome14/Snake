using UnityEngine;
using System.Collections;

public class Bonus : MonoBehaviour
{
    public Collider2D gridArea;
    private Snake snake;

    private bool eaten;
    private float spawnTime = 25f;
    private float despawnTime = 10f;
    private float targetTime;
    private bool spawnedBonus;

    private int bonusIndex;
    private SpriteRenderer spriteRenderer;

    public Sprite FastBoostSprite;
    public Sprite SlowBoostSprite;

    public int gridWidth = 17;
    public int gridHeight = 15;

    private void Awake()
    {
        snake = FindFirstObjectByType<Snake>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        //Invoke("settleRandomBonus", 5f);
        targetTime = spawnTime;
        spawnedBonus = false;
    }

    void Update()
    {
        targetTime -= Time.deltaTime;

        if (targetTime <= 0.0f)
        {
            timerEnded();
        }
    }

    public void settleRandomBonus()
    {
        float chance = UnityEngine.Random.value;

        if (chance < 0.6f)
        {
            bonusIndex = 1;
        }
        else
        {
            bonusIndex = 0;
        }

        if (bonusIndex == 1)
        {
            if (FastBoostSprite != null)
            {
                spriteRenderer.sprite = FastBoostSprite;
            }
        }
        else
        {
            if (SlowBoostSprite != null)
            {
                spriteRenderer.sprite = SlowBoostSprite;
            }
        }

        RandomizePosition();
    }


    public void RandomizePosition()
    {
        Bounds bounds = gridArea.bounds;

        float cellWidth = bounds.size.x / gridWidth;
        float cellHeight = bounds.size.y / gridHeight;

        int xIndex;
        int yIndex;

        do
        {
            xIndex = UnityEngine.Random.Range(0, gridWidth);
            yIndex = UnityEngine.Random.Range(0, gridHeight);
        }
        while (snake != null && snake.Occupies(xIndex, yIndex));

        float x = bounds.min.x + cellWidth * (xIndex + 0.5f);
        float y = bounds.min.y + cellHeight * (yIndex + 0.5f);

        transform.position = new Vector2(x, y);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Snake"))
        {
            //eaten = true;
            transform.position = new Vector2(-30, -30);
            targetTime = spawnTime;
            spawnedBonus = false;

            if (bonusIndex == 1)
            {
                snake.IncreaseSpeed();
            }
            else
            {
                snake.DecreaseSpeed();
            }
        }
    }

    private void timerEnded()
    {
        if (spawnedBonus)
        {
            transform.position = new Vector2(-30, -30);
            spawnedBonus = false;
            targetTime = spawnTime;
        }
        else
        {
            settleRandomBonus();
            spawnedBonus = true;
            targetTime = despawnTime;
        }
    }

}
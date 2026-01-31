using UnityEngine;

public class Food : MonoBehaviour
{
    public Collider2D gridArea; 
    private Snake snake;
    private SpriteRenderer spriteRenderer;

    public Sprite Pomme;

    public int gridWidth = 17;
    public int gridHeight = 15;

    private void Awake()
    {
        snake = FindFirstObjectByType<Snake>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (Pomme != null)
        {
            spriteRenderer.sprite = Pomme;
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
        RandomizePosition();
    }
}

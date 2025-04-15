using UnityEngine;
using UnityEngine.UI; 

[RequireComponent(typeof(SpriteRenderer), typeof(Button))]
public class Tile : MonoBehaviour 
{
    [SerializeField] private Button _button;
    private Vector2Int _position; 
    private SpriteRenderer _spriteRenderer;
    private GameController _gameController; 
    
    public Color TileColor { get; private set;}
    
    private void Awake()
    {
        _button.onClick.AddListener(OnClickAction);
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    public void Initialize(GameController controller, Vector2Int pos, Color color)
    {
        _gameController = controller;
        _position = pos;
        SetColor(color);
    }
    
    public void SetColor(Color newColor)
    {
        TileColor = newColor;
        
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = newColor;
        }
    }
    
    private void OnClickAction()
    {
        if (_gameController != null && !_gameController.IsGameOver())
        {
            _gameController.OnTileClicked(this);
        }
    }
}
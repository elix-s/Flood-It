using UnityEngine;
using UnityEngine.UI; 
using System.Collections.Generic;
using Common.SavingSystem;
using Cysharp.Threading.Tasks;
using TMPro;
using VContainer;
using Random = UnityEngine.Random; 

public class GameController : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private TMP_Dropdown _sizeDropdown; 
    [SerializeField] private TMP_Dropdown _colorDropdown;
    [SerializeField] private TMP_Dropdown _movesDropdown;

    [Header("Prefabs and grid")]
    [SerializeField] private GameObject _tilePrefab;
    [SerializeField] private Transform _gridParent; 
    [SerializeField] private float _spacing = 0.1f; 

    [Header("UI elements")]
    [SerializeField] private TMP_Text _movesText; 
    [SerializeField] private TMP_Text _gameOverText; 
    [SerializeField] private Button _restartButton; 

    [Header("Colors")]
    private List<Color> _availableColors = new List<Color>() {
        Color.red, Color.blue, Color.green, Color.yellow, 
        Color.magenta, new Color(1, 0.5f, 0), Color.cyan
    };
    
    private Logger _logger;
    private SavingSystem _savingSystem;
    private AppData _appData;
    
    private int _gridSize;
    private int _numColors;
    private int _maxMoves;
    private int _currentMoves;

    private Tile[,] _grid; 
    private List<Color> _currentLevelColors;
    private bool _isGameOver = false;
    private bool _isDataLoading = false;
    
    [Inject]
    private void Construct(Logger logger, SavingSystem savingSystem)
    {
        _logger = logger;
        _savingSystem = savingSystem;
    }

    private async void Awake()
    {
        _appData = new AppData();
        _restartButton.onClick.AddListener(SetupGame);

        var data = await _savingSystem.LoadDataAsync<AppData>();

        if (data == null)
        {
            _sizeDropdown.value = 1;
            _colorDropdown.value = 1;
            _movesDropdown.value = 1;
        }
        else
        {
            _sizeDropdown.value = data.Size;
            _colorDropdown.value = data.Color;
            _movesDropdown.value = data.Moves;
        }

        _sizeDropdown.onValueChanged.AddListener(OnChangeDropdownValue_Size);
        _colorDropdown.onValueChanged.AddListener(OnChangeDropdownValue_Color);
        _movesDropdown.onValueChanged.AddListener(OnChangeDropdownValue_Moves);
        
        _isDataLoading = true;
    }

    private void OnChangeDropdownValue_Size(int value)
    {
        _appData.Size = value;
        SaveData(_appData);
    }
    
    private void OnChangeDropdownValue_Color(int value)
    {
        _appData.Color = value;
        SaveData(_appData);
    }
    
    private void OnChangeDropdownValue_Moves(int value)
    {
        _appData.Moves = value;
        SaveData(_appData);
    }

    private async void Start()
    {
        await UniTask.WaitUntil(() => _isDataLoading);
        SetupGame(); 
    }

    private void SaveData(AppData data)
    {
        _savingSystem.SaveDataAsync<AppData>(data).Forget();
        SetupGame();
    }

    private void SetupGame()
    {
        _isGameOver = false;
        _currentMoves = 0;
        _gameOverText.text = "";
        _gridSize = GetSizeFromDropdown();
        _numColors = GetColorsFromDropdown();
        _maxMoves = GetMovesFromDropdown();
        
        _currentLevelColors = new List<Color>();
        if (_availableColors.Count < _numColors)
        {
            _logger.LogError("Number of available colors does not match number of colors");
            _numColors = _availableColors.Count;
        }
        
        List<Color> shuffledColors = new List<Color>(_availableColors);
        
        // Simple shuffle:
        for (int i = 0; i < shuffledColors.Count; i++) 
        {
            int randomIndex = Random.Range(i, shuffledColors.Count);
            Color temp = shuffledColors[i];
            shuffledColors[i] = shuffledColors[randomIndex];
            shuffledColors[randomIndex] = temp;
        }
        
        for (int i = 0; i < _numColors; i++)
        {
            _currentLevelColors.Add(shuffledColors[i]);
        }
        
        ClearGrid();
        GenerateGrid();
        UpdateMovesUI();
    }
    
    private int GetColorsFromDropdown()
    {
        if (int.TryParse(_colorDropdown.options[_colorDropdown.value].text, out int result))
        {
            return result;
        }
        
        return 6;
    }

    private int GetMovesFromDropdown()
    {
        if (int.TryParse(_movesDropdown.options[_movesDropdown.value].text, out int result))
        {
            return result;
        }
        
        return 25;
    }

    private int GetSizeFromDropdown()
    {
        switch (_sizeDropdown.value)
        {
            case 0: return 10;
            case 1: return 14;
            case 2: return 18;
            default: return 14;
        }
    }
    
    private void ClearGrid()
    {
        if (_gridParent == null) return;
        foreach (Transform child in _gridParent)
        {
            Destroy(child.gameObject);
        }
        
        _grid = null; 
    }

    private void GenerateGrid()
    {
        _grid = new Tile[_gridSize, _gridSize];
        float totalWidth = _gridSize + (_gridSize - 1) * _spacing;
        float totalHeight = _gridSize + (_gridSize - 1) * _spacing;
        Vector3 startOffset = new Vector3(-totalWidth / 2f + 0.5f, totalHeight / 2f - 0.5f, 0);

        for (int y = 0; y <_gridSize; y++)
        {
            for (int x = 0; x < _gridSize; x++)
            {
                Vector3 position = new Vector3(x * (1 + _spacing), -y * (1 + _spacing), 0) + startOffset;
                var tileGO = Instantiate(_tilePrefab, position, Quaternion.identity, _gridParent);
                tileGO.name = $"Tile_{x}_{y}";

                Tile tileScript = tileGO.GetComponent<Tile>();
                
                if (tileScript != null)
                {
                    Color randomColor = _currentLevelColors[Random.Range(0, _numColors)];
                    tileScript.Initialize(this, new Vector2Int(x, y), randomColor);
                    _grid[x, y] = tileScript;
                }
                else
                {
                    _logger.LogError("Tile prefab is missing the Tile script");
                }
            }
        }
    }

    public void OnTileClicked(Tile clickedTile)
    {
        if (_isGameOver) return;

        Color clickedColor = clickedTile.TileColor;
        Color startTileColor = _grid[0, 0].TileColor; 
        
        if (clickedColor == startTileColor) return;
        
        _currentMoves++;
        
        UpdateMovesUI();
        FloodFillAlgorithm.PerformFloodFill(_grid, new Vector2Int(0, 0), clickedColor);
        CheckGameStatus();
    }
    
    private void CheckGameStatus()
    {
        if (CheckWinCondition())
        {
            EndGame(true);
            return;
        }
        
        if (_currentMoves >= _maxMoves)
        {
            EndGame(false);
            return;
        }
    }

    private bool CheckWinCondition()
    {
        Color targetColor = _grid[0, 0].TileColor; 
        
        for (int y = 0; y < _gridSize; y++)
        {
            for (int x = 0; x < _gridSize; x++)
            {
                if (_grid[x, y].TileColor != targetColor)
                {
                    return false; 
                }
            }
        }
        
        return true; 
    }

    private void EndGame(bool won)
    {
        _isGameOver = true;
        
        if (won)
        {
            _gameOverText.text = "Victory!";
        }
        else
        {
            _gameOverText.text = "Defeat!";
        }
    }

    private void UpdateMovesUI()
    {
        _movesText.text = $"Moves: {_currentMoves} / {_maxMoves}";
    }

    public bool IsGameOver()
    {
        return _isGameOver;
    }
}

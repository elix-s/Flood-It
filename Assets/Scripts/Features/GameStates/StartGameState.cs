using Common.UIService;
using Cysharp.Threading.Tasks;

public class StartGameState : IGameState
{
    private Logger _logger;
    private UIService _uiService;
    
    public StartGameState(Logger logger, UIService uiService)
    {
        _logger = logger;
        _uiService = uiService;
    }

    public async UniTask Enter(StatePayload payload)
    {
        _uiService.ShowLoadingScreen(2000).Forget();
        _uiService.ShowUIPanel("GameState").Forget();
    }

    public void Update()
    {
      
    }

    public async UniTask Exit()
    {
        _uiService.HideUIPanel();
    }
}
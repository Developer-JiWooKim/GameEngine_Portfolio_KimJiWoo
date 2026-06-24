using FischlWorks_FogWar;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private GameTimer _gameTimer;
    public GameTimer GameTimer => _gameTimer;    

    private bool _isGameEnd = false;

    //#TODO:열쇠로 할지, 상호작용으로 할지 미정
    private int _requiredKeyCount = 5;         // 게임 클리어 위해 모아야 되는 열쇠 개수 
    private int _currentCollectedKeyCount = 0; // 현재 모은 키 개수

    public int CurrentCollectedKeyCount => _currentCollectedKeyCount;
    public int RequiredKeyCount => _requiredKeyCount;

    public event System.Action<int, int> OnKeyCollected;     // 열쇠를 모을때마다 발생할 이벤트(효과음, UI업데이트)
    public event System.Action           OnAllKeysCollected; // 모든 열쇠를 모은 시점에 1회 발생할 이벤트(Goal Point 생성)
    
    public event System.Action OnClear;
    public event System.Action OnGameOver;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _gameTimer = new GameTimer();
    }

    private void Update()
    {
        _gameTimer.UpdateTime();
    }

    public void CollectKey()
    {
        if (_currentCollectedKeyCount >= _requiredKeyCount) return;

        _currentCollectedKeyCount++;

        OnKeyCollected?.Invoke(_currentCollectedKeyCount, _requiredKeyCount);

        if(_currentCollectedKeyCount >= _requiredKeyCount)
        {
            OnAllKeysCollected?.Invoke();
        }
    }

    public void Clear()
    {
        if (_isGameEnd) return;

        _isGameEnd = true;

        _gameTimer.StopTimer();

        // 모든 몬스터 타겟 제거
        foreach (var monster in FindObjectsByType<MonsterController>())
        {
            monster.Target = null;
        }           

        OnClear?.Invoke();
    }

    public void GameStart()
    {
        _isGameEnd = false;

        _gameTimer.StartTimer();
    }

    public void GameOver()
    {
        if (_isGameEnd) return;

        _isGameEnd = true;

        _gameTimer.StopTimer();

        OnGameOver?.Invoke();
    }

    public void Replay()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GameEnd()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private GameTimer _gameTimer;
    public GameTimer GameTimer => _gameTimer;    

    private bool _isGameEnd = false;    

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

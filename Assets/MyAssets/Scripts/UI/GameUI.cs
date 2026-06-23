using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("Setup Panel")]
    [SerializeField] private GameObject      _setupPanel;
    [SerializeField] private TMP_InputField  _colsInputField;
    [SerializeField] private TMP_InputField  _rowsInputField;
    [SerializeField] private TMP_InputField  _monsterCountInputField;
    [SerializeField] private TextMeshProUGUI _errorText;
    [SerializeField] private Button          _startButton;   

    [Header("Game Panel")]
    [SerializeField] private GameObject      _inGamePanel;
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private Button          _endButton;
    [SerializeField] private DamageflashUI   _damageflashUI;

    [Header("Result Panel")]
    [SerializeField] private GameObject      _resultPanel;
    [SerializeField] private TextMeshProUGUI _resultText;
    [SerializeField] private TextMeshProUGUI _mazeSizeText;
    [SerializeField] private TextMeshProUGUI _resultTimeText;
    [SerializeField] private Button          _replayButton;
    [SerializeField] private Button          _gameEndButton;

    [Header("참조")]
    [SerializeField] private MazeLayerManager _mazeLayerManager;
    [SerializeField] private UnitSpawner      _unitSpawner;    

    [Header("Fixed Size")]
    [Tooltip("고정 미로 크기/몬스터 수. true면 Setup Panel을 건너뛰고 바로 시작")]
    [SerializeField] private bool _useFixedSize = true;
    [SerializeField] private int _fixedCols = 20;
    [SerializeField] private int _fixedRows = 20;
    [SerializeField] private int _fixedMonsterCnt = 10;

    private PlayerController _player;

    private int _minSize = 5;
    private int _maxSize = 50;

    private int _maxMonsterCnt;

    private string _errorTextString;

    private void Start()
    {
        SubscribeButtonEvent();

        _errorTextString = $"Please enter only positive integers\nMaze Size: Min:5 / Max:50\nMonster Count: Min:0 / Max:";

        if(_useFixedSize)
        {
            StartSetFixedSize();
        }
        else
        {
            SetSetupPanel();
        }
    }

    private void StartSetFixedSize()
    {
        _setupPanel.SetActive(false);

        _mazeLayerManager.SetLayersAndMazeGenerate(_fixedCols, _fixedRows);

        _unitSpawner.SetMonsterCount(_fixedMonsterCnt);
        _unitSpawner.SpawnAll();

        _player = _unitSpawner.Player;

        SetInGamePanel();
    }

    private void SubscribeButtonEvent()
    {
        _startButton.onClick.AddListener(OnStartButtonClicked);
        _endButton.onClick.AddListener(() => GameManager.Instance.GameOver());
        _replayButton.onClick.AddListener(GameManager.Instance.Replay);
        _gameEndButton.onClick.AddListener(GameManager.Instance.GameEnd);
    }

    private void Update()
    {
        if(_inGamePanel.activeSelf)
        {
            _timerText.text = GameManager.Instance.GameTimer.GetFormattedTime();
        }
    }

    /// <summary>
    /// Setup Panel 초기화, 게임 시작 전 미로 크기 설정
    /// </summary>
    private void SetSetupPanel()
    {
        _errorText.gameObject.SetActive(false);

        _colsInputField.contentType         = TMP_InputField.ContentType.IntegerNumber;
        _rowsInputField.contentType         = TMP_InputField.ContentType.IntegerNumber;
        _monsterCountInputField.contentType = TMP_InputField.ContentType.IntegerNumber;

        // 처음 게임 시작 시 -> Setup Panel 활성화, Result 패널 비활성화, Hp Text 비활성화
        _setupPanel.SetActive(true);
        _inGamePanel.SetActive(false);
        _resultPanel.SetActive(false);
    }

    /// <summary>
    /// 시작 버튼 클릭 시 미로 생성 후 게임 시작
    /// </summary>
    private void OnStartButtonClicked()
    {
        _errorText.gameObject.SetActive(false);

        // 빈 값이면 기본값 적용(maze:10X10 / monsterCount:5)
        if (string.IsNullOrEmpty(_colsInputField.text)) _colsInputField.text = "10";
        if (string.IsNullOrEmpty(_rowsInputField.text)) _rowsInputField.text = "10";
        if (string.IsNullOrEmpty(_monsterCountInputField.text)) _monsterCountInputField.text = "5";

        // 입력값 정수 체크
        if (!int.TryParse(_colsInputField.text, out int cols) ||
            !int.TryParse(_rowsInputField.text, out int rows) ||
            !int.TryParse(_monsterCountInputField.text, out int monsterCount))
        {
            _maxMonsterCnt = 0;
            ShowErrorText(_maxMonsterCnt);
            return;
        }

        _maxMonsterCnt = cols * rows - 2; // 최대 몬스터 수는 셀 전체 개수 - 2(시작, 끝 지점)

        // 몬스터 수가 음수거나 최대 수를 넘는지 체크
        if (monsterCount < 0 || monsterCount > _maxMonsterCnt)
        {
            ShowErrorText(_maxMonsterCnt);
            return;
        }

        // 미로 크기 최소/최대 값 사이인지 체크
        if (cols < _minSize || cols > _maxSize ||
            rows < _minSize || rows > _maxSize)
        {
            ShowErrorText(_maxMonsterCnt);
            return;
        }        

        _unitSpawner.SetMonsterCount(monsterCount);
        _unitSpawner.SpawnAll();

        // 유닛들(플레이어 포함) 스폰 후 PlayerController 참조 연결
        _player = _unitSpawner.Player;

        // Setup Panel 닫고 InGameUI 설정하고 게임 시작
        _setupPanel.SetActive(false);

        SetInGamePanel();
    }

    private void ShowErrorText(int count)
    {
        if(count <= 0 ||  count > _maxSize * _maxSize - 2 )
        {
            _errorText.text = _errorTextString + "Col X Row - 2";
        }
        else
        {
            _errorText.text = _errorTextString + $"{count}";
        }
        
        _errorText.gameObject.SetActive(true);
    }

    /// <summary>
    /// 게임 시작 후 HP UI, Result UI 초기화
    /// </summary>
    private void SetInGamePanel()
    {
        _player.OnHPChanged += UpdateHp;
        _player.OnHPChanged += (current, max) => _damageflashUI?.Flash();
        _player.OnDead      += () => GameManager.Instance.GameOver();

        GameManager.Instance.OnClear    += () => ShowResult("CLEAR!!");
        GameManager.Instance.OnGameOver += () => ShowResult("GAME OVER..");

        _inGamePanel.SetActive(true);

        UpdateHp(_player.CurrentHp, _player.MaxHp);

        GameManager.Instance.GameStart();
    }

    /// <summary>
    /// 결과 화면 UI, 결과 UI 띄우면서 플레이어 입력 막기 위해 PlayerInput 컴포넌트 비활성화
    /// </summary>
    private void ShowResult(string message)
    {
        _inGamePanel.SetActive(false);

        _resultText.text = message;
        _mazeSizeText.text   = $"Maze Size: 20 X 20";
        _resultTimeText.text = $"End Time : {GameManager.Instance.GameTimer.GetFormattedTime()}";
        _resultPanel.SetActive(true);
        
        _player.GetComponent<PlayerInputHandler>().enabled = false;
    }

    /// <summary>
    /// 체력 UI 갱신 메소드
    /// </summary>
    private void UpdateHp(int current, int max)
    {
        _hpText.text = $"HP : {current} / {max}";
    }
}

using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private int   _maxHp     = 3;

    private PlayerInput _playerInput;
    private PlayerMove  _playerMove;

    private int _currentHp;

    public int CurrentHp => _currentHp;
    public int MaxHp => _maxHp;

    public event System.Action<int, int> OnHPChanged;  // 체력이 변경됐을 때 (현재, 최대) 이벤트
    public event System.Action OnDead;                 // 플레이어 체력이 0이 되어 죽었을 때 이벤트 

    private void Awake() => Initialize();

    /// <summary>
    /// 초기화 메소드
    /// </summary>
    private void Initialize()
    {
        _playerInput = GetComponent<PlayerInput>();
        _playerMove  = GetComponent<PlayerMove>();

        _currentHp = _maxHp;
    }

    private void Update()
    {
        // 플레이어 키보드 입력 감지
        if (UnityEngine.InputSystem.Keyboard.current is not null)
        {
            _playerInput.InputKeyboardValue();

            // PlayerInput 컴포넌트에서 감지한 플레이어 입력 값(Vector2)을 Vector3로 바꿔서 변수에 저장 
            Vector3 dir = new Vector3(_playerInput.InputVector.x, 0, _playerInput.InputVector.y);

            // PlayerMove 컴포넌트에 방향과 속력을 전달해서 플레이어를 이동
            _playerMove.Move(dir, _moveSpeed);
        }        
    }

    /// <summary>
    /// 몬스터 공격 범위 안에 닿았을 때 호출될 메소드
    /// </summary>
    public void TakeDamage()
    {
        if (_currentHp <= 0) return;

        _currentHp--;

        OnHPChanged?.Invoke(_currentHp, _maxHp);

        if (_currentHp <= 0) OnDead?.Invoke();
    }
}

using FischlWorks_FogWar;
using Unity.AI.Navigation;
using UnityEngine;

public class MazeLayerManager : MonoBehaviour
{    
    public enum LayerType
    {
        Physical,
        Arcane,
    }    

    [Header("Layer Roots")]
    [SerializeField] private GameObject _physicalLayerRoot;
    [SerializeField] private GameObject _arcaneLayerRoot;

    [Header("Maze Generators")]
    [SerializeField] private MazeGenerator _physicalMaze;
    [SerializeField] private MazeGenerator _arcaneMaze;

    [Header("Seeds")] // 서로 다른 시드를 부여 -> 다른 미로 구조를 갖게 설계
    [SerializeField] private int _physicalSeed = 1001;
    [SerializeField] private int _arcaneSeed   = 2002;

    [Header("Layer Switch Check")]
    [SerializeField] private float _overlapCheckRadius = 0.6f; // 전환 시 플레이어 위치에서 벽과의 겹침을 검사할 반경

    [Header("Fog War System")]
    [SerializeField] private csFogWar _fogWarSystem;

    [Header("NavMesh")]
    [SerializeField] private NavMeshSurface _physicalNavMeshSurface;
    [SerializeField] private NavMeshSurface _arcaneNavMeshSurface;

    private static MazeLayerManager _instance = null;
    public static MazeLayerManager Instance => _instance;

    private int _currentWallLayerMask;
    public int CurrentWallLayerMask => _currentWallLayerMask;

    public csFogWar FogWarSystem => _fogWarSystem;

    private int _physicalWallMask;
    private int _arcaneWallMask;

    private PlayerInput _playerInput;

    private LayerType _currentLayer = LayerType.Physical;
    public LayerType CurrentLayer => _currentLayer;

    

    public event System.Action<LayerType> OnLayerChanged;
    public event System.Action            OnLayerSwitchBlocked; // 전환 실패(벽에 끼임) 시 호출, 사운드/전환 불가 UI 혹은 화면 쉐이킹?

    private void Awake()
    {
        if(_instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }        

        _physicalWallMask = LayerMask.GetMask("Wall_Physical");
        _arcaneWallMask   = LayerMask.GetMask("Wall_Arcane");
    }
    private void OnDestroy()
    {
        if(_playerInput != null)
        {
            _playerInput.OnLayerSwitchRequested -= HandleLayerSwitchRequested;
        }
    }

    // #TODO: 삭제 예정 or UI버튼에 미로 바꾸기? 버튼으로 미로 구조 바꿀 수 있는 기능 추가 예정
    public void RandomSeed()
    {
        _physicalSeed = Random.Range(0, 1100);
        _arcaneSeed   = Random.Range(1100, 2200);

        Debug.Log($"_physicalSeed: {_physicalSeed} / _arcaneSeed:{_arcaneSeed}");
    }

    /// <summary>
    /// 미로 생성 후 레이어 설정
    /// </summary>
    public void SetLayersAndMazeGenerate(int cols, int rows)
    {
        // #TODO: 미로 바뀌는지 테스트용 코드
        RandomSeed();

        _physicalMaze.SetSeed(_physicalSeed);
        _physicalMaze.SetSize(cols, rows);
        _physicalMaze.Generate();
        _physicalNavMeshSurface.BuildNavMesh(); // Physical 전용 NavMesh 베이크

        _arcaneMaze.SetSeed(_arcaneSeed);
        _arcaneMaze.SetSize(cols, rows);
        _arcaneMaze.Generate();
        _arcaneNavMeshSurface.BuildNavMesh(); // Arcane 전용 NavMesh 베이크
        _arcaneNavMeshSurface.RemoveData();   // 시작은 Physical이 활성이므로 Arcane 데이터는 일단 빼둠

        _currentLayer = LayerType.Physical;
        _currentWallLayerMask = _physicalWallMask;

        _physicalMaze.SetWallsActiveState(true);
        _arcaneMaze.SetWallsActiveState(false);

        if(_fogWarSystem != null)
        {            
            _fogWarSystem.ScanLevel();

        }
    }

    /// <summary>
    /// 런타임에 스폰된 플레이어의 PlayerInput을 등록, 레이어 전환 이벤트 구독
    /// </summary>
    public void RegisterPlayerInput(PlayerInput playerInput)
    {
        if (_playerInput != null)
        {
            _playerInput.OnLayerSwitchRequested -= HandleLayerSwitchRequested;
        }

        _playerInput = playerInput;
        _playerInput.OnLayerSwitchRequested += HandleLayerSwitchRequested;
    }

    private void HandleLayerSwitchRequested(Vector3 playerPosition)
    {
        TrySwitchLayer(playerPosition);
    }

    public bool TrySwitchLayer(Vector3 playerPosition)
    {
        LayerType targetLayer = _currentLayer == LayerType.Physical ? LayerType.Arcane : LayerType.Physical;

        int targetWallMask = targetLayer == LayerType.Physical ? _physicalWallMask : _arcaneWallMask;

        bool isBlocked = Physics.CheckSphere(playerPosition, _overlapCheckRadius, targetWallMask);

        if(isBlocked)
        {
            OnLayerSwitchBlocked?.Invoke();
            return false;
        }

        SetActiveLayer(targetLayer);
        return true;
    }

    private void SetActiveLayer(LayerType layer)
    {
        _currentLayer = layer;

        bool physicalActive = layer == LayerType.Physical;

        // 현재 활성화된 레이어에 따라 벽 레이어 마스크 변경
        _currentWallLayerMask = physicalActive ? _physicalWallMask : _arcaneWallMask;

        _physicalMaze.SetWallsActiveState(physicalActive);
        _arcaneMaze.SetWallsActiveState(!physicalActive);

        if(physicalActive)
        {
            _physicalNavMeshSurface.AddData();
            _arcaneNavMeshSurface.RemoveData();
        }
        else
        {
            _arcaneNavMeshSurface.AddData();
            _physicalNavMeshSurface.RemoveData();
        }

        // #TODO: 제미니 코드
        if(_fogWarSystem != null)
        {
            _fogWarSystem.ScanLevel();
            _fogWarSystem.shadowcaster.ResetTileVisibility(); // 레이어 전환 시 이전 시야 기록 초기화
        }

        OnLayerChanged?.Invoke(layer);
    }

    /// <summary>
    /// 현재 활성화된 레이어의 MazeGenerator 반환
    /// </summary>
    public MazeGenerator GetActiveMaze()
    {
        return _currentLayer == LayerType.Physical ? _physicalMaze : _arcaneMaze;
    }
}
using System;
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

    private int _physicalWallMask;
    private int _arcaneWallMask;

    private PlayerInput _playerInput;

    private LayerType _currentLayer = LayerType.Physical;
    public LayerType CurrentLayer => _currentLayer;

    public event System.Action<LayerType> OnLayerChanged;
    public event System.Action            OnLayerSwitchBlocked; // 전환 실패(벽에 끼임) 시 호출, 사운드/전환 불가 UI 혹은 화면 쉐이킹?

    private void Awake()
    {
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
        _physicalSeed = UnityEngine.Random.Range(0, 1100);
        _arcaneSeed = UnityEngine.Random.Range(1100, 2200);

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

        _arcaneMaze.SetSeed(_arcaneSeed);
        _arcaneMaze.SetSize(cols, rows);
        _arcaneMaze.Generate();

        _currentLayer = LayerType.Physical;

        _physicalMaze.SetWallsActiveState(true);
        _arcaneMaze.SetWallsActiveState(false);
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

        _physicalMaze.SetWallsActiveState(physicalActive);
        _arcaneMaze.SetWallsActiveState(!physicalActive);

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

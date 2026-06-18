using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    [SerializeField] private MazeLayerManager _mazeLayerManager;

    [SerializeField] private GameObject _monsterPrefab;
    [SerializeField] private GameObject _playerPrefab;

    [SerializeField] private int   _monsterCount = 5;
    [SerializeField] private float _spawnY       = 1f; // 유닛 스폰 y 좌표

    private Vector2Int _playerStartCell; // 플레이어 시작 셀
    private Vector2Int _goalCell;        // 목표 지점 셀

    private GameObject _player;

    private MazeGenerator _activeMaze;

    public PlayerController Player => _player?.GetComponent<PlayerController>();

    // 생성된 몬스터를 List로 관리
    private List<GameObject> _monsters = new List<GameObject>();
    public List<GameObject> Monsters => _monsters;
    
    /// <summary>
    /// 몬스터 수 설정 메소드(UI에서 호출)
    /// </summary>
    public void SetMonsterCount(int count)
    {
        _monsterCount = count;
    }

    /// <summary>
    /// 몬스터들을 미로의 랜덤한 위치에 스폰하는 메소드
    /// </summary>
    public void SpawnAll()
    {
        ClearAll();
        Initialize();
        SpawnPlayer();
        SpawnMonsters();

        // #TODO: 제미니 코드
        if(_mazeLayerManager != null && _mazeLayerManager.FogWarSystem != null)
        {
            FischlWorks_FogWar.csFogWar fogWar = _mazeLayerManager.FogWarSystem;

            // 1. 실시간 생성된 초기 미로 벽을 감지하도록 첫 스캔 실행
            fogWar.ScanLevel();

            // 2. 스폰된 플레이어에게 안개 시스템을 넘겨주며 시야를 켭니다.
            PlayerController pc = Player;
            if (pc != null)
            {
                pc.RegisterToFogSystem(fogWar);
            }
        }
    }

    private void Initialize()
    {
        _activeMaze = _mazeLayerManager.GetActiveMaze();

        _playerStartCell = new Vector2Int(0, 0);
        _goalCell = new Vector2Int(_activeMaze.Cols - 1, _activeMaze.Rows - 1);
    }

    private void SpawnPlayer()
    {
        if (_playerPrefab == null) return;

        Vector3 spawnPos = _activeMaze.GetCell(_playerStartCell.x, _playerStartCell.y).worldCenter;

        spawnPos.y = _spawnY;

        _player = Instantiate(_playerPrefab, spawnPos, Quaternion.identity);

        FollowCamera followCamera = Camera.main.GetComponent<FollowCamera>();
        if(followCamera != null)
        {
            followCamera.Target = _player.transform;
        }        

        PlayerInput playerInput = _player.GetComponent<PlayerInput>();
        if(playerInput != null)
        {
            _mazeLayerManager.RegisterPlayerInput(playerInput);
        }
    }

    private void SpawnMonsters()
    {
        if (_monsterPrefab == null) return;

        List<Cell> candidates = new List<Cell>();

        // 몬스터를 스폰 가능한 셀들 리스트에 추가
        foreach (Cell cell in _activeMaze.AllCells)
        {
            // 플레이어 시작점, 목표 지점에는 몬스터 생성 X
            if (cell.col == _playerStartCell.x && cell.row == _playerStartCell.y) continue;
            if (cell.col == _goalCell.x && cell.row == _goalCell.y) continue;

            candidates.Add(cell);
        }

        // Fisher-Yates 셔플로 랜덤한 위치 보장
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
        }

        // _monsterCount 수만큼 몬스터 스폰
        for (int i = 0; i < _monsterCount; i++)
        {
            // Cell.worldCenter로 몬스터 스폰해서 위치가 벽과 겹치지 않게
            Vector3 spawnPos = candidates[i].worldCenter;
            spawnPos.y = _spawnY;

            GameObject monster = Instantiate(_monsterPrefab, spawnPos, Quaternion.identity);
            MonsterController mc = monster.GetComponent<MonsterController>();
            if (mc != null && _player != null)
            {
                mc.Target = _player.transform;
            }

            // 스폰한 몬스터들을 리스트에 추가
            _monsters.Add(monster);
        }
    }

    /// <summary>
    /// 몬스터 리스트 초기화 메소드
    /// </summary>
    private void ClearAll()
    {
        if(_player != null)
        {
            Destroy(_player);
            _player = null;
        }

        foreach (var mon in _monsters)
        {
            if (mon != null) Destroy(mon);
        }

        _monsters.Clear();
    }
}

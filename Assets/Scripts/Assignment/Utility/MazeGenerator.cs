using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [Header("Gird")]
    [SerializeField] private int    _cols = 10; // 가로
    [SerializeField] private int    _rows = 10; // 세로
    [SerializeField] private float  _cellSize = 5f;    

    [Header("Walls")]
    [SerializeField] private GameObject _wallPrefab;
    [SerializeField] private float      _wallThickness = 0.3f;
    [SerializeField] private float      _wallHeight    = 3f;

    [Header("Goal Point")]
    [SerializeField] private GameObject _goalPointPrefab;

    [Header("Random Seed")]
    [SerializeField] private int _seed = -1;    

    private Cell[,]          _grid;    
    private List<GameObject> _wallObjects = new List<GameObject>();
    private List<Cell>       _allCells    = new List<Cell>();

    private Vector2 worldStart;

    private readonly List<Cell>  _neighborCache = new List<Cell>(4);
    private readonly Stack<Cell> _dfsStack      = new Stack<Cell>();

    public IReadOnlyList<Cell> AllCells => _allCells;
    public int Cols  => _cols;
    public int Rows  => _rows;
    public float CellSize => _cellSize;

    /// <summary>
    /// 미로 사이즈 설정 메소드
    /// </summary>
    public void SetSize(int cols, int rows)
    {
        _cols = cols;
        _rows = rows;

        worldStart = new Vector2(-cols * _cellSize * 0.5f, -rows * _cellSize * 0.5f);
    }

    /// <summary>
    /// 미로 생성 메소드
    /// </summary>
    public void Generate()
    {
        ClearWalls();
        InitGrid();
        RunDFS();
        SpawnWalls();
        SpawnGoalPoint();
    }

    /// <summary>
    /// 목표 지점 오브젝트 스폰 메소드
    /// </summary>
    private void SpawnGoalPoint()
    {
        if (_goalPointPrefab == null) return;

        Cell goalCell = _grid[_cols - 1, _rows - 1];

        Vector3 spawnPos = goalCell.worldCenter;
        spawnPos.y = 1f;

        Instantiate(_goalPointPrefab, spawnPos, Quaternion.identity);
    }

    /// <summary>
    /// Wall이 담긴 리스트 초기화
    /// </summary>
    public void ClearWalls()
    {
        foreach (var wall in _wallObjects)
        {
            if (wall != null)
            {
                DestroyImmediate(wall);
            }
        }

        _wallObjects.Clear();
    }

    /// <summary>
    /// 10(_cols) x 10(_rows) 셀을 생성 후 각각의 센터를 구한 후 리스트에 저장
    /// </summary>
    private void InitGrid()
    {
        _allCells.Clear();

        _grid = new Cell[_cols, _rows];

        for (int r = 0; r < _rows; r++)
        {
            for (int c = 0; c < _cols; c++)
            {
                Vector3 center = new Vector3(worldStart.x + c * _cellSize + _cellSize * 0.5f, 0f, worldStart.y + r * _cellSize + _cellSize * 0.5f);

                _grid[c, r] = new Cell(c, r, center);
                _allCells.Add(_grid[c, r]);
            }
        }
    }

    /// <summary>
    /// 깊이 기반(DFS) 벽 생성할 위치 탐색
    /// </summary>
    private void RunDFS()
    {
        // 매번 다른 맵이 나오도록 시드값 설정
        if (_seed == -1)
        {
            Random.InitState(System.DateTime.Now.Millisecond);
        }
        else
        {
            Random.InitState(_seed);
        }

        _dfsStack.Clear();

        Cell startCell = _grid[Random.Range(0, _cols), Random.Range(0, _rows)];
        startCell.visited = true;
        _dfsStack.Push(startCell);

        while (_dfsStack.Count > 0)
        {
            Cell current = _dfsStack.Peek();
            List<Cell> neighbors = GetUnvisitedNeighbors(current);

            if (neighbors.Count > 0)
            {
                // 이웃한 셀
                Cell next = neighbors[Random.Range(0, neighbors.Count)];

                RemoveWallBetween(current, next);

                next.visited = true;
                _dfsStack.Push(next);
            }
            else
            {
                // 막힌 곳 
                _dfsStack.Pop();
            }
        }
    }

    /// <summary>
    /// 범위 내(Grid)에서 현재 셀 주위에 이웃한 미방문 셀 검사
    /// </summary>
    private List<Cell> GetUnvisitedNeighbors(Cell cell)
    {
        _neighborCache.Clear();

        int c = cell.col;
        int r = cell.row;

        if (r + 1 < _rows && !_grid[c, r + 1].visited) _neighborCache.Add(_grid[c, r + 1]); // north
        if (r - 1 >= 0    && !_grid[c, r - 1].visited) _neighborCache.Add(_grid[c, r - 1]); // south
        if (c + 1 < _cols && !_grid[c + 1, r].visited) _neighborCache.Add(_grid[c + 1, r]); // east
        if (c - 1 >= 0    && !_grid[c - 1, r].visited) _neighborCache.Add(_grid[c - 1, r]); // west

        return _neighborCache;
    }

    /// <summary>
    /// 두 셀 사이의 벽을 제거하는 메소드
    /// </summary>
    private void RemoveWallBetween(Cell a, Cell b)
    {
        // dc, dr로 두 셀의 상대적 위치를 파악해 어느 방향 벽을 제거할지 결정
        int dc = b.col - a.col;
        int dr = b.row - a.row;

        // 두 셀은 벽을 공유하므로 양쪽 셀 모두 해당 방향 벽을 제거
        if      (dr == 1)   { a.northWall = false; b.southWall = false; }    
        else if (dr == -1)  { a.southWall = false; b.northWall = false; }    
        else if (dc == 1)   { a.eastWall  = false; b.westWall  = false; } 
        else if (dc == -1)  { a.westWall  = false; b.eastWall  = false; } 
    }

    /// <summary>
    /// 벽들을 정해진 위치에 생성하는 메소드
    /// </summary>
    private void SpawnWalls()
    {
        if (_wallPrefab == null) return;

        GameObject wallParent = new GameObject("Walls");
        wallParent.transform.SetParent(transform);

        for (int r = 0; r < _rows; r++)
        {
            for (int c = 0; c < _cols; c++)
            {
                Cell cell = _grid[c, r];

                // 셀 중심 월드 좌표
                float cx = cell.worldCenter.x;
                float cz = cell.worldCenter.z;

                if (cell.northWall)
                {
                    Vector3 pos = new Vector3(cx, _wallHeight * 0.5f, cz + _cellSize * 0.5f);
                    SpawnWall(pos, false, wallParent.transform);
                }

                if (cell.eastWall)
                {
                    Vector3 pos = new Vector3(cx + _cellSize * 0.5f, _wallHeight * 0.5f, cz);
                    SpawnWall(pos, true, wallParent.transform);
                }
                // southWall은 r == 0(맨 아래 행)일 때만 처리, 인접한 두 셀이 같은 벽을 중복 생성하지 않도록 하기 위해
                if (r == 0 && cell.southWall)
                {
                    Vector3 pos = new Vector3(cx, _wallHeight * 0.5f, cz - _cellSize * 0.5f);
                    SpawnWall(pos, false, wallParent.transform);
                }
                // westWall은 c == 0 (맨 왼쪽 열)일 때만 처리, 인접한 두 셀이 같은 벽을 중복 생성하지 않도록 하기 위해
                if (c == 0 && cell.westWall)
                {
                    Vector3 pos = new Vector3(cx - _cellSize * 0.5f, _wallHeight * 0.5f, cz);
                    SpawnWall(pos, true, wallParent.transform);
                }
            }
        }
    }

    /// <summary>
    /// 벽 하나 생성, isVertical -> true = Z축 방향 벽, false = X축 방향 벽
    /// </summary>
    private void SpawnWall(Vector3 position, bool isVertical, Transform parent)
    {
        GameObject wall = Instantiate(_wallPrefab, position, Quaternion.identity, parent);

        // 가로 벽: _cellSize 길이, X 방향으로 
        // 세로 벽: _cellSize 길이, Z 방향으로 
        wall.transform.localScale = isVertical ? new Vector3(_wallThickness, _wallHeight, _cellSize) : new Vector3(_cellSize, _wallHeight, _wallThickness);

        _wallObjects.Add(wall);
    }

    /// <summary>
    /// 월드 좌표를 셀 위치로 변환하는 메소드
    /// </summary>
    public Vector2Int WorldToCell(Vector3 worldPos)
    {
        int col = (int)((worldPos.x - worldStart.x) / _cellSize);
        int row = (int)((worldPos.z - worldStart.y) / _cellSize);

        col = Mathf.Clamp(col, 0, _cols - 1);
        row = Mathf.Clamp(row, 0, _rows - 1);

        return new Vector2Int(col, row);
    }

    /// <summary>
    /// 셀 위치를 월드 좌표로 변환하는 메소드
    /// </summary>
    public Vector3 CellToWorld(Vector2Int cell)
    {
        return _grid[cell.x, cell.y].worldCenter;
    }

    /// <summary>
    /// [col, row] 위치에 있는 셀을 반환하는 메소드
    /// </summary>
    public Cell GetCell(int col, int row)
    {
        if (col < 0 || col >= _cols || row < 0 || row >= _rows) return null;

        return _grid[col, row];
    }

    /// <summary>
    /// 에디터에서 그리드 확인용
    /// </summary>
    private void OnDrawGizmos()
    {
        // 월드 범위 테두리
        Gizmos.color = Color.green;
        float totalW = _cols * _cellSize;
        float totalH = _rows * _cellSize;
        Vector3 center = new Vector3(worldStart.x + totalW * 0.5f, 0, worldStart.y + totalH * 0.5f);
        Gizmos.DrawWireCube(center, new Vector3(totalW, 0.1f, totalH));

        // 셀 그리드
        Gizmos.color = new Color(1, 1, 0, 0.2f);
        for (int r = 0; r < _rows; r++)
        {
            for (int c = 0; c < _cols; c++)
            {
                float cx = worldStart.x + c * _cellSize + _cellSize * 0.5f;
                float cz = worldStart.y + r * _cellSize + _cellSize * 0.5f;
                Gizmos.DrawWireCube(new Vector3(cx, 0, cz), new Vector3(_cellSize - 0.1f, 0.1f, _cellSize - 0.1f));
            }
        }
    }
}

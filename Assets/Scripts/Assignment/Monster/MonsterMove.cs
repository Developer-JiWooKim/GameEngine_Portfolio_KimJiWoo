using System.Collections.Generic;
using UnityEngine;

public class MonsterMove : MonoBehaviour
{
    [SerializeField] private float _moveSpeed    = 7f;   
    [SerializeField] private float _rotateSpeed  = 15f;  
    [SerializeField] private float _nodeDistance = 0.5f;

    private List<Vector3> _path             = new List<Vector3>();
    private int           _pathIndex;
    private Vector2Int    _lastGoalCell     = Vector2Int.zero;
    private float         _sphereCastRaduis = 0.5f; 
    private int           _wallLayerMask;

    private void Awake()
    {
        _wallLayerMask = LayerMask.GetMask("Wall");
    }

    /// <summary>
    /// Idle 상태일 때 제자리 회전
    /// </summary>
    public void IdleRotate()
    {
        transform.Rotate(0f, _rotateSpeed * Time.deltaTime, 0f, Space.World);
    }

    /// <summary>
    /// 타겟 위치까지의 최단 거리를 구하는 AStarPathfinder로 Path를 생성
    /// </summary>
    public void RequestPath(Vector3 targetPos)
    {
        if (AStarPathfinder.Instance == null) return;

        Vector2Int goalCell = AStarPathfinder.Instance.WorldToCell(targetPos);

        if (goalCell == _lastGoalCell && _path.Count > 0) return;

        _lastGoalCell = goalCell;

        List<Vector3> newPath = AStarPathfinder.Instance.FindPath(transform.position, targetPos);

        if (newPath != null && newPath.Count > 0)
        {
            _path = newPath;
            _pathIndex = 0;
        }
    }

    /// <summary>
    /// A* 알고리즘으로 생성한 길 초기화
    /// </summary>
    public void ClearPath()
    {
        _path.Clear();
        _pathIndex = 0;
    }

    /// <summary>
    /// 타겟 방향으로 이동
    /// </summary>
    public void MoveToTarget(Vector3 targetPos)
    {
        if (IsPathClear(targetPos))
        {
            // 타겟과 자신 사이에 장애물이 없으면 바로 타겟 방향으로 이동
            ClearPath();
            MoveStraight(targetPos);
        }
        else
        {
            RequestPath(targetPos);

            // 길이 없거나 끝에 도달했으면 타겟 방향으로 직진
            if (_path.Count == 0 || _pathIndex >= _path.Count)
            {
                MoveStraight(targetPos);
                return;
            }

            Vector3 myPos = transform.position;

            Vector3 nodePos = _path[_pathIndex];
            nodePos.y = myPos.y;

            // 목표 노드에 도착 시 다음 노드로 목표 노드를 바꿈
            if ((nodePos - myPos).sqrMagnitude < _nodeDistance * _nodeDistance)
            {
                _pathIndex++;
                return;
            }

            // 정해진 목표 노드의 센터로 이동
            MoveStraight(nodePos);
        }
    }

    /// <summary>
    /// 타겟 방향으로 회전, 공격 범위 안에 있을 때 플레이어를 바라보게 하는 메소드
    /// </summary>
    public void LookAtTarget(Vector3 targetPos)
    {
        Vector3 dir = targetPos - transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude < 0.001f) return;

        RotateToward(dir);
    }

    /// <summary>
    /// 타겟과 자신 사이에 장애물이 있는지 검사, SphereCast로 현재 타겟과 자신 사이에 벽이 있는지 체크
    /// </summary>
    private bool IsPathClear(Vector3 targetPos)
    {
        Vector3 myPos = transform.position;

        Vector3 origin = myPos + Vector3.up * 0.5f;

        Vector3 direction = targetPos - myPos;
        direction.y = 0;

        float distance = direction.magnitude;

        Vector3 dirNormalized = direction / distance;

        return !Physics.SphereCast(origin, _sphereCastRaduis, dirNormalized, out _, distance, _wallLayerMask);
    }

    /// <summary>
    /// 타겟의 방향으로 이동
    /// </summary>
    private void MoveStraight(Vector3 targetPos)
    {
        Vector3 dir = targetPos - transform.position;
        dir.y = 0f;

        float distance = dir.magnitude;

        if (distance < 0.001f) return;

        dir /= distance; // 정규화

        RotateToward(dir);       

        transform.Translate(dir * _moveSpeed * Time.deltaTime, Space.World);
    }

    /// <summary>
    ///  dir 방향으로 부드러운 회전 적용
    /// </summary>
    private void RotateToward(Vector3 dir)
    {
        Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);

        // 부드러운 회전
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotateSpeed);
    }
}

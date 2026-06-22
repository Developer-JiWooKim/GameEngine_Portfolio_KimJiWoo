using UnityEngine;

/// <summary>
/// 플레이어 이동 시 부드럽게 따라가는 컴포넌트
/// </summary>
public class FollowCamera : MonoBehaviour
{   
    [SerializeField] private float _smoothTime = 0.3f; // 높을수록 더 느리게 따라감

    [Header("Quarter View Settings")]
    [SerializeField] private float _height       = 20f;  // 높이 (Y)
    [SerializeField] private float _distanceBack = 18f;  // 뒤로 빠지는 거리 (Z)
    [SerializeField] private float _pitchAngle   = 45f;  // X축 회전 각도

    private Vector3   _velocity = Vector3.zero;
    private Vector3   _cameraOffset;
    private Transform _target;

    public Transform Target
    {
        get => _target;
        set => _target = value;
    }

    private void Awake() => Initiailize();
    private void Initiailize()
    {
        _cameraOffset = new Vector3(0, _height, -_distanceBack);
        transform.rotation = Quaternion.Euler(_pitchAngle, 0f, 0f);
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        SmoothFollowing();
    }

    private void SmoothFollowing()
    {
        Vector3 targetPosition = _target.position + _cameraOffset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref _velocity,
            _smoothTime
        );
    }
}

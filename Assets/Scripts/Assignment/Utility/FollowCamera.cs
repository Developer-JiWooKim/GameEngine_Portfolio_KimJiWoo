using UnityEngine;

/// <summary>
/// 플레이어 이동 시 부드럽게 따라가는 컴포넌트
/// </summary>
public class FollowCamera : MonoBehaviour
{   
    [SerializeField] private float _smoothTime = 0.3f; // 높을수록 더 느리게 따라감

    private Vector3   _velocity     = Vector3.zero;
    private Vector3   _cameraOffset = new Vector3(0, 20f, 0);
    private Transform _target;

    public Transform Target
    {
        get => _target;
        set => _target = value;
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

using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 화면 전체를 덮는 빨간 Image를 피격 시 깜빡이듯 보여주는 컴포넌트
/// </summary>
public class DamageflashUI : MonoBehaviour
{
    [SerializeField] private Image _flashImage;
    [SerializeField] private float _maxAlpha = 0.35f;
    [SerializeField] private float _fadeDuration = 0.4f;

    private bool  _isFlashing;
    private float _flashTimer;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (_flashImage == null) return;

        Color c = _flashImage.color;
        c.a = 0f;
        _flashImage.color = c;

        _isFlashing = false;
    }

    /// <summary>
    /// 화면을 빨갛게 깜빡이게 하는 메소드
    /// </summary>
    public void Flash()
    {
        if (_flashImage == null) return;

        _flashTimer = 0f;

        // 이미 Flash가 진행 중이면 타이머만 리셋, 중복 방지
        if (!_isFlashing)
        {
            _ = PlayFlash();
        }
    }

    private async Awaitable PlayFlash()
    {
        _isFlashing = true;

        Color c = _flashImage.color;

        while (_flashTimer < _fadeDuration)
        {
            c.a = Mathf.Lerp(_maxAlpha, 0f, _flashTimer / _fadeDuration);

            _flashImage.color = c;

            _flashTimer += Time.deltaTime;

            await Awaitable.NextFrameAsync(destroyCancellationToken);
        }

        c.a = 0f;

        _flashImage.color = c;

        _isFlashing = false;
    }
}

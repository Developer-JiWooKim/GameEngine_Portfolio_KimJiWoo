using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    private Vector2 _inputVector     = Vector2.zero;

    public Vector2 InputVector => _inputVector;

    /// <summary>
    /// Tab 키를 눌러 미로 전환을 시도할 때 발생하는 이벤트
    /// </summary>
    public event System.Action<Vector3> OnLayerSwitchRequested;

    /// <summary>
    /// InputSystem
    /// </summary>
    public void InputKeyboardValue()
    {
        float h = 0f;
        float v = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)  h = -1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) h = 1f;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)    v = 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)  v = -1f;

        _inputVector = new Vector2(h, v);

        if(Keyboard.current.tabKey.wasPressedThisFrame)
        {
            OnLayerSwitchRequested?.Invoke(transform.position);
        }
    }
}

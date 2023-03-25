using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;


public class MouseInputProvider : SingletonMonoBehaviour<MouseInputProvider>
{
    [SerializeField] private InputAction pressAction;
    [SerializeField] private InputAction releaseAction;
    private bool _isMouseGettingDown = false;

    [RuntimeInitializeOnLoadMethod]
    private void Init()
    {
        pressAction.Enable();
        releaseAction.Enable();

        pressAction.started += x => OnPress();
        releaseAction.performed += x => OnRelease();
    }
    

    private void OnPress()
    {
        _isMouseGettingDown = true;
    }

    private void OnRelease()
    {
        _isMouseGettingDown = false;
    }

    public Vector2 MousePosition => GetMousePosition();

    private Vector2 GetMousePosition()
    {
        if (Application.isEditor)
        {
            return Mouse.current.position.ReadValue();
        }
        else
        {
            TouchControl touchControl = Touchscreen.current.touches[0];
            return touchControl.position.ReadValue();
        }
    }

    /// <summary>
    /// マウスが押される/タップされるまで待つ
    /// </summary>
    /// <param name="gameCt"></param>
    public async UniTask OnHoldDownAsync(CancellationToken gameCt)
    {
        await UniTask.WaitUntil(() => _isMouseGettingDown, cancellationToken: gameCt);
    }

    /// <summary>
    /// マウスが離される/タップが終わるまで待つ
    /// </summary>
    /// <param name="gameCt"></param>
    public async UniTask OnHoldUpAsync(CancellationToken gameCt)
    {
        await UniTask.WaitWhile(() => _isMouseGettingDown, cancellationToken: gameCt);
    }
}
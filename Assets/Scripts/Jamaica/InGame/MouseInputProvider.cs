using System.Threading;
using Cysharp.Threading.Tasks;
using Jamaica.Share;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Jamaica.InGame
{
    public class MouseInputProvider : SingletonMonoBehaviour<MouseInputProvider>
    {
        [SerializeField] private InputAction pressAction;
        [SerializeField] private InputAction releaseAction;
        public Vector2 mousePosition;
        private bool _isMouseGettingDown = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            Instance.Initialize_Instance();
        }

        private void Initialize_Instance()
        {
            pressAction.Enable();
            releaseAction.Enable();

            pressAction.performed += OnPress;
            releaseAction.performed += OnRelease;
        }

        public void GetMousePosition(InputAction.CallbackContext context)
        {
            mousePosition = context.ReadValue<Vector2>();
        }

        public async UniTask OnHoldDownAsync(CancellationToken gameCt)
        {
            await UniTask.WaitUntil(() => _isMouseGettingDown, cancellationToken: gameCt);
        }

        public async UniTask OnHoldUpAsync(CancellationToken gameCt)
        {
            await UniTask.WaitWhile(() => _isMouseGettingDown, cancellationToken: gameCt);
        }

        public void OnPress(InputAction.CallbackContext context)
        {
            if (context.performed) _isMouseGettingDown = true;
        }

        public void OnRelease(InputAction.CallbackContext context)
        {
            if (context.performed) _isMouseGettingDown = false;
        }
    }
}
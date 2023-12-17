using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Jamaica
{
    public class NumberBox : MonoBehaviour
    {
        [SerializeField] public bool isAnswerBox;
        [SerializeField] private TMP_Text numberText;
        private CancellationToken _destroyCancellationToken;
        public Vector2 initialPosition;

        private void Awake()
        {
            _destroyCancellationToken = this.GetCancellationTokenOnDestroy();
        }

        public async UniTask EndRollAnimationAsync(CancellationToken gameCt)
        {
            var mergedCt = CancellationTokenSource.CreateLinkedTokenSource(gameCt, _destroyCancellationToken).Token;
            await gameObject.transform.DOScale(new Vector2(1.1f, 1.1f), 0.2f).SetEase(Ease.InQuart)
                .ToUniTask(cancellationToken: mergedCt);
            await gameObject.transform.DOScale(new Vector2(1, 1), 0.2f).SetEase(Ease.OutQuart)
                .ToUniTask(cancellationToken: mergedCt);
        }

        public async UniTask MergeAsync(Vector2 destination, CancellationToken gameCt)
        {
            var mergedCt = CancellationTokenSource.CreateLinkedTokenSource(gameCt, _destroyCancellationToken).Token;
            await gameObject.transform.DOMove(destination, 0.5f).ToUniTask(cancellationToken: mergedCt);
            gameObject.SetActive(false);
        }

        public async UniTask MoveToCenterAsync(Vector2 answerBoxPosition, CancellationToken gameCt)
        {
            var mergedCt = CancellationTokenSource.CreateLinkedTokenSource(gameCt, _destroyCancellationToken).Token;
            var sequence = DOTween.Sequence();
            sequence.Append(transform.DOMove(answerBoxPosition, 0.8f));
            sequence.Join(transform.DOScale(new Vector2(2f, 2f), 0.8f));
            await sequence.Play().ToUniTask(cancellationToken: mergedCt);
        }

        public void ShowNumberBox()
        {
            var rt = GetComponent<RectTransform>();
            rt.localPosition = initialPosition;
            rt.localScale = new Vector2(1, 1);
            gameObject.SetActive(true);
        }

        public void SetNumberText(int number)
        {
            numberText.text = number.ToString();
        }
    }
}
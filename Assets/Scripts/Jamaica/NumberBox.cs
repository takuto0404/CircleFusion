using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

namespace Jamaica
{
    public class NumberBox : MonoBehaviour
    {
        /// <summary>
        /// パズル開始時に初期化される座標
        /// </summary>
        public Vector2 initialPosition;
    
        /// <summary>
        /// 数字を表示するテキスト
        /// </summary>
        [SerializeField] private TMP_Text numberText;

        [SerializeField] public bool isAnswerBox;
        private CancellationToken destroyCt;

        private void Awake()
        {
            destroyCt = this.GetCancellationTokenOnDestroy();
        }
    
        /// <summary>
        /// numberTextの数字を更新
        /// </summary>
        /// <param name="number"></param>
        public void SetNumberText(int number)
        {
            numberText.text = number.ToString();
        }

        /// <summary>
        /// シャッフルが終わった後にクイっと大きくなるやつ
        /// </summary>
        /// <param name="gameCt"></param>
        public async UniTask FinishedShuffleAnimationAsync(CancellationToken gameCt)
        {
            var mergedCt = CancellationTokenSource.CreateLinkedTokenSource(gameCt, destroyCt).Token;
            await gameObject.transform.DOScale(new Vector2(1.1f, 1.1f), 0.2f).SetEase(Ease.InQuart).ToUniTask(cancellationToken:mergedCt);
            await gameObject.transform.DOScale(new Vector2(1, 1), 0.2f).SetEase(Ease.OutQuart).ToUniTask(cancellationToken:mergedCt);
        }

        /// <summary>
        /// このNumberBoxを表示させる
        /// </summary>
        public void ShowBox()
        {
            var rt = GetComponent<RectTransform>();
            rt.localPosition = initialPosition;
            rt.localScale = new Vector2(1, 1);
            gameObject.SetActive(true);
        }

        /// <summary>
        /// このNumberBoxにアニメーションをつけながら、合体する先のボックスまで動いて消える
        /// </summary>
        /// <param name="destination">目的地</param>
        /// <param name="gameCt"></param>
        public async UniTask HideBoxAsync(Vector2 destination, CancellationToken gameCt)
        {
            var mergedCt = CancellationTokenSource.CreateLinkedTokenSource(gameCt, destroyCt).Token;
            await gameObject.transform.DOMove(destination, 0.5f).ToUniTask(cancellationToken:mergedCt);
            gameObject.SetActive(false);
        }

        /// <summary>
        /// このボックスが最後まで残ったときに、真ん中の答えのボックスに動いていくアニメーションを行う
        /// </summary>
        /// <param name="answerBoxPosition">目標値のサイコロの場所</param>
        /// <param name="gameCt"></param>
        public async UniTask MoveToEqualAsync(Vector2 answerBoxPosition,CancellationToken gameCt)
        {
            var mergedCt = CancellationTokenSource.CreateLinkedTokenSource(gameCt, destroyCt).Token;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOMove(answerBoxPosition, 0.8f));
            sequence.Join(transform.DOScale(new Vector2(2f, 2f), 0.8f));
            await sequence.Play().ToUniTask(cancellationToken:mergedCt);
        }
    }
}
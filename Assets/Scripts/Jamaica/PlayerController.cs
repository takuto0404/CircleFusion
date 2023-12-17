using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Jamaica
{
    public class PlayerController : SingletonMonoBehaviour<PlayerController>
    {
        /// <summary>
        /// MVPパターンのVに接続する
        /// </summary>
        [SerializeField] private GameUIView gameUIView;


        /// <summary>
        /// パズルごとのプレイヤーの行動・処理を行う非同期メソッド
        /// </summary>
        /// <param name="gameCt"></param>
        public async UniTask PlayerBehavior(CancellationToken gameCt)
        {
            gameCt.ThrowIfCancellationRequested();
            try
            {
                while (true)
                {
                    NumberBox hoveredOne = null;
                    NumberBox hoveredAnotherOne = null;
                    var operatorResult = OperatorMark.None;


                    while (operatorResult == OperatorMark.None)
                    {
                        hoveredAnotherOne = null;
                        while (hoveredAnotherOne == null)
                        {
                            gameUIView.ClearLine();
                            hoveredOne = null;
                            while (hoveredOne == null)
                            {
                                await MouseInputProvider.Instance.OnHoldDownAsync(gameCt);
                                hoveredOne = GetHoveredNumberBox();
                            }

                            var one = hoveredOne;
                            using (this.UpdateAsObservable().Subscribe(_ =>
                                       gameUIView.DrawLine(AlignPosition(one.initialPosition),
                                           MouseInputProvider.Instance.mousePosition)))
                            {
                                await MouseInputProvider.Instance.OnHoldUpAsync(gameCt);
                                hoveredAnotherOne = GetHoveredNumberBox();
                            }
                        }
                        if(hoveredOne == hoveredAnotherOne)continue;

                        gameUIView.DrawLine(AlignPosition(hoveredOne.initialPosition),
                            AlignPosition(hoveredAnotherOne.initialPosition));
                        var canCalculate =
                            GameUIPresenter.Instance.CheckCalculations(hoveredOne, hoveredAnotherOne);
                        var result = await gameUIView.SelectOperatorAsync(canCalculate, gameCt);
                        gameUIView.ClearLine();
                        operatorResult = result;
                    }

                    GameUIPresenter.Instance.Calculation(hoveredOne, hoveredAnotherOne, operatorResult);
                    JamaicaHistory.SetHist(DiceCalculator.GetAllDices(), 
                        GameUIPresenter.Instance.CreateFormulaText(DiceCalculator.FetchCurrentFormula()));
                    GameUIPresenter.Instance.UpdateFormulaText(JamaicaHistory.LastHist().FormulaText);
                }
            }
            catch (OperationCanceledException e)
            {
                return;
            }
        }

        private Vector2 AlignPosition(Vector2 position)
        {
            return new Vector2(position.x + Screen.width / 2f, position.y + Screen.height / 2f);
        }

        /// <summary>
        /// 今マウス上にあるサイコロを返す
        /// </summary>
        /// <returns></returns>
        private NumberBox GetHoveredNumberBox()
        {
            var pointData = new PointerEventData(EventSystem.current);
            var rayResult = new List<RaycastResult>();
            pointData.position = Input.mousePosition;
            EventSystem.current.RaycastAll(pointData, rayResult);
            var box = rayResult.Select(x => x.gameObject.GetComponent<NumberBox>()).Where(x => x != null)
                .Where(x => !x.isAnswerBox);
            var numberBoxes = box as NumberBox[] ?? box.ToArray();
            if (!numberBoxes.Any())
            {
                return null;
            }

            return numberBoxes.First();
        }
    }
}
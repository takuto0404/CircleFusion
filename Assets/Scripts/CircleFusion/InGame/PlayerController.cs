using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Jamaica.Share;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Jamaica.InGame
{
    public class PlayerController : SingletonMonoBehaviour<PlayerController>
    {
        [SerializeField] private GameUIView gameUIView;

        private static Vector2 AlignPosition(Vector2 position)
        {
            return new Vector2(position.x + Screen.width / 2f, position.y + Screen.height / 2f);
        }

        private static NumberBox GetHoveredNumberBox()
        {
            var pointData = new PointerEventData(EventSystem.current);
            var rayResult = new List<RaycastResult>();
            pointData.position = Input.mousePosition;
            EventSystem.current.RaycastAll(pointData, rayResult);
            var box = rayResult.Select(x => x.gameObject.GetComponent<NumberBox>()).Where(x => x != null)
                .Where(x => !x.isAnswerBox);
            var numberBoxes = box as NumberBox[] ?? box.ToArray();
            return !numberBoxes.Any() ? null : numberBoxes.First();
        }
        
        public async UniTask ProcessPlayerActionAsync(CancellationToken gameCt)
        {
            gameCt.ThrowIfCancellationRequested();
            try
            {
                while (true)
                {
                    NumberBox firstNumberBox = null;
                    NumberBox secondNumberBox = null;
                    var selectedOperator = OperatorMark.None;
                    
                    while (selectedOperator == OperatorMark.None)
                    {
                        secondNumberBox = null;
                        while (secondNumberBox == null)
                        {
                            gameUIView.ClearLine();
                            firstNumberBox = null;
                            while (firstNumberBox == null)
                            {
                                await MouseInputProvider.Instance.OnHoldDownAsync(gameCt);
                                firstNumberBox = GetHoveredNumberBox();
                            }

                            var boxInitialPosition = firstNumberBox.initialPosition;
                            using (UniTaskAsyncEnumerable.EveryUpdate().Subscribe(_ =>
                                       gameUIView.DrawLine(AlignPosition(boxInitialPosition),
                                           MouseInputProvider.Instance.mousePosition)))
                            {
                                await MouseInputProvider.Instance.OnHoldUpAsync(gameCt);
                                secondNumberBox = GetHoveredNumberBox();
                            }
                        }
                        
                        if(firstNumberBox == secondNumberBox)continue;

                        gameUIView.DrawLine(AlignPosition(firstNumberBox.initialPosition),
                            AlignPosition(secondNumberBox.initialPosition));
                        var isCalculable =
                            GameUIPresenter.Instance.CheckCalculations(firstNumberBox, secondNumberBox);
                        selectedOperator = await gameUIView.SelectOperatorAsync(isCalculable, gameCt);
                    }
                    gameUIView.ClearLine();
                    GameUIPresenter.Instance.Calculation(firstNumberBox, secondNumberBox, selectedOperator);
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
    }
}
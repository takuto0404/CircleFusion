using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx.Triggers;
using UniRx;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.UIElements;

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
        while (true)
        {
            NumberBox hoveredOne = null;
            NumberBox hoveredAnotherOne = null;
            var operatorResult = OperatorMark.Return;
            
            
            while (operatorResult == OperatorMark.Return)
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
                               gameUIView.DrawLine(AlignPosition(one.initialPosition), MouseInputProvider.Instance.mousePosition)))
                    {
                        Debug.Log(one.initialPosition);
                        await MouseInputProvider.Instance.OnHoldUpAsync(gameCt);
                        hoveredAnotherOne = GetHoveredNumberBox();
                    }
                
                }

                gameUIView.DrawLine(AlignPosition(hoveredOne.initialPosition), AlignPosition(hoveredAnotherOne.initialPosition));
                var canCalculate =
                    GameUIPresenter.Instance.CanCalculate(hoveredOne, hoveredAnotherOne);
                var result = await gameUIView.SelectOperatorsAsync(canCalculate, gameCt);
                gameUIView.ClearLine();
                operatorResult = result;
            }
            GameUIPresenter.Instance.Calculation(hoveredOne,hoveredAnotherOne,operatorResult);
            JamaicaHistory.SetHist(DiceModel.GetDices(),GameUIPresenter.Instance.MakeFormulaText(DiceModel.GetThisTimeFormula()));
            GameUIPresenter.Instance.SetFormulaText(JamaicaHistory.LastHist().FormulaText);
        }
    }

    private Vector2 AlignPosition(Vector2 position)
    {
        return new Vector2(position.x + 960, position.y + 540);
    }

    /// <summary>
    /// 今マウス上にあるサイコロを返す
    /// </summary>
    /// <returns></returns>
    private NumberBox GetHoveredNumberBox()
    {
        PointerEventData pointData = new PointerEventData(EventSystem.current);
        List<RaycastResult> RayResult = new List<RaycastResult>();
        pointData.position = Input.mousePosition;
        EventSystem.current.RaycastAll(pointData, RayResult);
        var box = RayResult.Select(x => x.gameObject.GetComponent<NumberBox>()).Where(x => x != null)
            .Where(x => !x.isAnswerBox);
        if (!box.Any())
        {
            return null;
        }

        return box.First();
    }
}
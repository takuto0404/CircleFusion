using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UniRx;

public class GameUIView : MonoBehaviour
{
    [SerializeField] private Button backStepButton;
    /// <summary>
    /// 答え(計算目標)となる箱
    /// </summary>
    [SerializeField] private NumberBox answerBox;
    
    /// <summary>
    /// 式に利用できるサイコロの箱
    /// </summary>
    [SerializeField] public List<NumberBox> numberBoxes;
    
    /// <summary>
    /// リタイアするときに押すボタン
    /// </summary>
    [SerializeField] private Button retireButton;
    
    /// <summary>
    /// 連続で何回正解したかを示すUIのバックの図形
    /// </summary>
    [SerializeField] private GameObject comboImage;
    
    /// <summary>
    /// ゲーム終了時のリスタートボタン
    /// </summary>
    [SerializeField] private Button restartButton;
    
    /// <summary>
    /// 右上の方に出てくる式の履歴のテキスト
    /// </summary>
    [SerializeField] private TMP_Text formulaText;
    
    /// <summary>
    /// 左上に出てくるスコア表示用テキスト
    /// </summary>
    [SerializeField] private TMP_Text scoreText;
    
    /// <summary>
    /// 左上に出てくるタイム表示用テキスト
    /// </summary>
    [SerializeField] private TMP_Text timeText;
    
    /// <summary>
    /// ゲームクリア時に上から出てくるパネル
    /// </summary>
    [SerializeField] private GameObject gameClearPanel;
    
    /// <summary>
    /// ゲームオーバー時に上から出てくるパネル
    /// </summary>
    [SerializeField] private GameObject gameOverPanel;
    
    /// <summary>
    /// ゲーム終了時の画面に出てくるスコア結果のテキスト
    /// </summary>
    [SerializeField] private TMP_Text scoreResultText;
    
    /// <summary>
    /// ゲーム終了時の画面に出てくるタイム結果のテキスト
    /// </summary>
    [SerializeField] private TMP_Text timeResultText;
    
    /// <summary>
    /// ゲーム終了時の画面に出てくるコンボ結果のテキスト
    /// </summary>
    [SerializeField] private TMP_Text comboResultText;

    [SerializeField] private List<Button> operatorButtons;
    
    /// <summary>
    /// Canvas上で線を引いてくれるクラス
    /// </summary>
    [SerializeField] private UGUILineRenderer drawLine;

    /// <summary>
    /// 演算記号の種類と文字列のディク
    /// </summary>
    private readonly Dictionary<OperatorMark, string> _operatorDic = new ()
    {
        { OperatorMark.Plus, "+" },
        { OperatorMark.Minus, "-" },
        { OperatorMark.Times, "×" },
        { OperatorMark.Devided, "÷" }
    };

    /// <summary>
    /// 右上の式のテキストを更新する
    /// </summary>
    /// <param name="formulas">式の履歴リスト</param>
    public void SetFormulaText(List<Formula> formulas)
    {
        var formulaTexts = formulas.Select(formula =>
            $"{formula.One} {_operatorDic[formula.OperatorMark]} {formula.AnotherOne} = {formula.Answer}\n").ToList();
        var text = "";
        formulaTexts.ForEach(textData => text += textData);

        formulaText.text = text;
    }


    public void DrawLine(Vector2 position1,Vector2 position2)
    {
        var positions = new []{position1, position2};
        drawLine.SetPositions(positions);
    }

    public void PuzzleInit()
    {
        numberBoxes.ForEach(box => box.ShowBox());
    }

    public void ClearLine()
    {
        drawLine.SetPositions(new[]{Vector2.zero,Vector2.zero});
    }

    /// <summary>
    /// タイマーの表示用テキストを更新する
    /// </summary>
    public void SetTimerText(float time)
    {
        timeText.text = time.ToString();
    }

    /// <summary>
    /// 演算記号を順々にアニメーションをつけて表示する
    /// </summary>
    /// <param name="canCalculate">それぞれの演算記号で計算可能か</param>
    /// <param name="gameCt"></param>
    private async UniTask ShowOperatorsAsync(bool[] canCalculate,CancellationToken gameCt)
    {
        for (var i = 0; i < 5; i++)
        {
            operatorButtons[i].gameObject.SetActive(canCalculate[i]);
            operatorButtons[i].transform.localRotation = Quaternion.Euler(0, 0, 20);
            await operatorButtons[i].transform.DORotate(new Vector3(0, 0, 0), 0.1f).ToUniTask(cancellationToken: gameCt);
        }
    }

    /// <summary>
    /// 演算記号を順々にアニメーションをつけて隠す
    /// </summary>
    /// <param name="gameCt"></param>
    private async UniTask HideOperatorsAsync(CancellationToken gameCt)
    {
        var activeOperators = operatorButtons.Select(button => button.gameObject).Where(operatorGameObject => operatorGameObject.activeSelf).ToList();
        foreach (var operatorGameObject in activeOperators)
        {
            operatorGameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
            await operatorGameObject.transform.DORotate(new Vector3(0, 0, 20), 0.1f).ToUniTask(cancellationToken: gameCt);
            operatorGameObject.SetActive(false);
        }
    }

    /// <summary>
    /// どれかの演算記号のボタンが押されるまで待つ
    /// </summary>
    /// <param name="canCalculate"></param>
    /// <param name="gameCt"></param>
    /// <returns>押された結果</returns>
    public async UniTask<OperatorMark> SelectOperatorsAsync(bool[] canCalculate,CancellationToken gameCt)
    {
        await ShowOperatorsAsync(canCalculate,gameCt);
        
        var tasks = operatorButtons.Select(button => button.OnClickAsync(gameCt));
        var result = await UniTask.WhenAny(tasks);

        await HideOperatorsAsync(gameCt);
        return (OperatorMark)result;
    }

    /// <summary>
    /// ゲーム終了時のアニメション
    /// </summary>
    /// <param name="wasGameCleared">ゲームクリアだったらtrue,ゲームオーバーならfalse</param>
    /// <param name="gameCt"></param>
    public async UniTask GameFinishedAnimationAsync(bool wasGameCleared, CancellationToken gameCt)
    {
        scoreResultText.text = $"ポイント:{GameData.Score}";
        RectTransform rt;
        if (wasGameCleared)
        {
            rt = gameClearPanel.GetComponent<RectTransform>();
            gameClearPanel.SetActive(true);
            timeResultText.text = $"じかん:{GameData.Timer.Value}";
        }
        else
        {
            rt = gameOverPanel.GetComponent<RectTransform>();
            gameOverPanel.SetActive(true);
        }
        rt.position = new Vector2(Screen.width / 2, Screen.height * 1.5f);
        await rt.DOMove(new Vector2(Screen.width / 2, Screen.height / 2), 0.8f).ToUniTask(cancellationToken:gameCt);

        if (wasGameCleared)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f),cancellationToken:gameCt);
            comboImage.SetActive(false);
        }
        
        scoreResultText.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);

        if (wasGameCleared)
        {
            comboImage.transform.localScale = new Vector2(1.5f, 1.5f);
            comboResultText.text = GameData.Combo.ToString();
            comboImage.SetActive(true);
            await comboImage.transform.DOScale(new Vector2(1, 1), 0.8f).ToUniTask(cancellationToken:gameCt);
        }

        await restartButton.OnClickAsync(gameCt);
    }
    public async UniTask RetireButtonOnClickAsync(CancellationToken gameCt)
    {
        await retireButton.OnClickAsync(gameCt);
    }

    public IObservable<Unit> ReturnButtonOnClickAsObservable()
    {
        return backStepButton.OnClickAsObservable();
    }

    public async UniTask MoveToEqualAsync(NumberBox numberBox,CancellationToken gameCt)
    {
        await numberBox.MoveToEqualAsync(answerBox.transform.position,gameCt);
    }
}
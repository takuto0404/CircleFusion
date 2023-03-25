using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;
using Unity.Mathematics;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Random = System.Random;
using Slider = UnityEngine.UI.Slider;

public class GameUIView : MonoBehaviour
{
    [SerializeField] private Button backStepButton;
    /// <summary>
    /// 答え(計算目標)となる箱
    /// </summary>
    [SerializeField] public NumberBox answerBox;
    
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

    [SerializeField] private Button settingButton;

    [SerializeField] private GameObject settingPanel;
    [SerializeField] private Slider diceMaxSlider;
    [SerializeField] private Slider diceAmountSlider;
    [SerializeField] private Button settingBackButton;
    [SerializeField] private Button settingRestartButton;

    [SerializeField] private GameObject numberBoxPrefab;
    [SerializeField] private Transform canvasT;
    [SerializeField] private TMP_Text diceMaxValueText;
    [SerializeField] private TMP_Text diceAmountValueText;
    
    /// <summary>
    /// 右上の式のテキストを更新する
    /// </summary>
    /// <param name="formulas">式の履歴リスト</param>
    public void SetFormulaText(string formulaText)
    {
        this.formulaText.text = formulaText;
    }


    public void DrawLine(Vector2 position1,Vector2 position2)
    {
        var positions = new []{position1, position2};
        drawLine.SetPositions(positions);
    }

    public string GetFormulaText()
    {
        return formulaText.text;
    }
    public void PuzzleInit()
    {
        if (numberBoxes.Count != GameInitialData.Instance.numberOfDice)
        { 
            var numberOfDice = GameInitialData.Instance.numberOfDice;
            numberBoxes.ForEach(x => Destroy(x.gameObject));
            numberBoxes.Clear();
            for (int i = 0; i < numberOfDice;i++)
            {
                var let = 310;
                var theta = (360 / numberOfDice) * i;
                Vector2 pos = new Vector2(Mathf.Sin(theta * Mathf.Deg2Rad) * let, Mathf.Cos(theta * Mathf.Deg2Rad) * let);
                if (i > numberOfDice / 2)
                {
                    theta = 360 - theta;
                    pos = new Vector2(-(Mathf.Sin(theta * Mathf.Deg2Rad) * let), Mathf.Cos(theta * Mathf.Deg2Rad) * let);
                }
                
                var scale = 1 - (numberOfDice - 5) * 0.03f;
                var box = Instantiate(numberBoxPrefab,canvasT);
                box.transform.SetAsFirstSibling();
                answerBox.transform.SetAsFirstSibling();
                var rt = box.GetComponent<RectTransform>();
                rt.localScale = new Vector2(scale, scale);
                var numberBox = box.GetComponent<NumberBox>();
                numberBox.initialPosition = pos;
                numberBoxes.Add(numberBox);
            }
        }
        numberBoxes.ForEach(box => box.ShowBox());
        gameClearPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        comboImage.SetActive(false);
        restartButton.gameObject.SetActive(false);
        scoreResultText.gameObject.SetActive(false);
        formulaText.text = "";
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
        timeText.text = $"じかん:{time.ToString()}";
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
    private void HideOperators()
    {
        foreach (var operatorButton in operatorButtons)
        {
            operatorButton.gameObject.SetActive(false);
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
        var operatorCts = new CancellationTokenSource();
        var mergedCts = CancellationTokenSource.CreateLinkedTokenSource(gameCt, operatorCts.Token);
        ShowOperatorsAsync(canCalculate,mergedCts.Token).Forget();
        
        var tasks = operatorButtons.Select(button => button.OnClickAsync(mergedCts.Token));
        var result = await UniTask.WhenAny(tasks);
        
        operatorCts.Cancel();
        HideOperators();
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
        scoreText.text = $"ポイント:{GameData.Score}";
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
        rt.localPosition = new Vector2(0, Screen.height);
        await rt.DOMove(Vector2.zero, 0.8f).ToUniTask(cancellationToken:gameCt);

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

    public IObservable<Unit> BackButtonOnClickAsObservable()
    {
        return backStepButton.OnClickAsObservable();
    }

    public IObservable<Unit> SettingButtonOnClickAsObservable()
    {
        return settingButton.OnClickAsObservable();
    }

    public async UniTask<bool> SettingProgress(CancellationToken gameCt)
    {
        using (diceAmountSlider.OnValueChangedAsObservable()
                   .Subscribe(value => diceAmountValueText.text = value.ToString()))
        {
            using (diceMaxSlider.OnValueChangedAsObservable()
                       .Subscribe(value => diceMaxValueText.text = value.ToString()))
            {
                settingPanel.SetActive(true);
                var task1 = settingBackButton.OnClickAsync(gameCt);
                var task2 = settingRestartButton.OnClickAsync(gameCt);
                var result  = await UniTask.WhenAny(task1, task2);
                if (result == 1)
                {
                    GameInitialData.Instance.diceMaxValue = (int)diceMaxSlider.value;
                    GameInitialData.Instance.numberOfDice = (int)diceAmountSlider.value;
                }
                settingPanel.SetActive(false);
                return result == 1;
            }
        }
        
    }

    public async UniTask MoveToEqualAsync(NumberBox numberBox,CancellationToken gameCt)
    {
        await numberBox.MoveToEqualAsync(answerBox.GetComponent<RectTransform>().position,gameCt);
    }
}
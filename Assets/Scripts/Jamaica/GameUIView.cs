using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Jamaica
{
    public class GameUIView : MonoBehaviour
    {
        [SerializeField] private Button backStepButton;
        [SerializeField] private TMP_Text solutionText;
        [SerializeField] public NumberBox answerBox;
        [SerializeField] private GameObject titlePanel;
        [SerializeField] public List<NumberBox> numberBoxes;
        [SerializeField] private Button retireButton;
        [SerializeField] private GameObject comboImage;
        [SerializeField] private Button restartButton;
        [SerializeField] private TMP_Text formulaText;
        [SerializeField] private GameObject noticePanel;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private GameObject gameClearPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TMP_Text scoreResultText;
        [SerializeField] private TMP_Text timeResultText;
        [SerializeField] private TMP_Text comboResultText;
        [SerializeField] private List<Button> operatorButtons;
        [SerializeField] private UGUILineRenderer drawLine;
        [SerializeField] private Button settingButton;
        [SerializeField] private GameObject settingPanel;
        [SerializeField] private Slider diceMaxSlider;
        [SerializeField] private Slider diceAmountSlider;
        [SerializeField] private Button settingBackButton;
        [SerializeField] private GameObject numberBoxPrefab;
        [SerializeField] private Transform canvasT;
        [SerializeField] private TMP_Text diceMaxValueText;
        [SerializeField] private TMP_Text diceAmountValueText;

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

        public void HideEverything()
        {
            ClearLine();
            HideOperators();
        }
        public void PuzzleInit()
        {
            titlePanel.SetActive(false);
            HideEverything();
            if (numberBoxes.Count != GameInitialData.Instance.diceCount)
            {
                diceAmountSlider.value = GameInitialData.Instance.diceCount;
                diceMaxSlider.value = GameInitialData.Instance.maxDiceFace;

                var numberOfDice = GameInitialData.Instance.diceCount;
                numberBoxes.ForEach(x => Destroy(x.gameObject));
                numberBoxes.Clear();
                for (int i = 0; i < numberOfDice;i++)
                {
                    var let = 310;
                    var theta = 360 / numberOfDice * i;
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

        public async UniTask ShowNotice()
        {
            noticePanel.SetActive(true);
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            noticePanel.SetActive(false);
        }

        /// <summary>
        /// タイマーの表示用テキストを更新する
        /// </summary>
        public void SetTimerText(float time)
        {
            timeText.text = $"じかん:{time:F2}";
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
                timeResultText.text = $"じかん:{GameData.CurrentTime.Value:F2}";
            }
            else
            {
                rt = gameOverPanel.GetComponent<RectTransform>();
                gameOverPanel.SetActive(true);
                solutionText.text = "Solutions:\n";
                var subscribed = new List<int>();
                for (int i = 0; i < 3 && i < GameData.FormulaString.Count; i++)
                {
                    var randomNum = Random.Range(0, GameData.FormulaString.Count);
                    while (subscribed.Contains(randomNum))
                    {
                        randomNum = Random.Range(0, GameData.FormulaString.Count);
                    }
                    solutionText.text += $"{GameData.FormulaString[randomNum]}\n";
                    subscribed.Add(randomNum);
                }
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
                comboResultText.text = GameData.ComboCount.ToString();
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

        public async UniTask SettingProgress(CancellationToken gameCt)
        {
            using (diceAmountSlider.OnValueChangedAsObservable()
                       .Subscribe(value => diceAmountValueText.text = value.ToString(CultureInfo.InvariantCulture)))
            {
                using (diceMaxSlider.OnValueChangedAsObservable()
                           .Subscribe(value => diceMaxValueText.text = value.ToString(CultureInfo.InvariantCulture)))
                {
                    settingPanel.SetActive(true);
                    
                    await settingBackButton.OnClickAsync(gameCt);
                    GameInitialData.Instance.maxDiceFace = (int)diceMaxSlider.value;
                    GameInitialData.Instance.diceCount = (int)diceAmountSlider.value;
                    settingPanel.SetActive(false);
                    await PlayerDataManager.SavePlayerDataAsync(
                        new PlayerData(GameData.Score, GameData.ComboCount, GameInitialData.Instance.diceCount,
                            GameInitialData.Instance.maxDiceFace), gameCt);
                }
            }
        
        }

        public async UniTask MoveToEqualAsync(NumberBox numberBox,CancellationToken gameCt)
        {
            await numberBox.MoveToEqualAsync(answerBox.GetComponent<RectTransform>().position,gameCt);
        }
    }
}
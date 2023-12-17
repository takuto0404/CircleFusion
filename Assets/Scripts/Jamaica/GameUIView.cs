using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Jamaica
{
    public class GameUIView : MonoBehaviour
    {
        [SerializeField] private Button exitSettingsButton;
        [SerializeField] private Button retireButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button undoStepButton;
        [SerializeField] private GameObject comboCounterImage;
        [SerializeField] private GameObject messagePanel;
        [SerializeField] private GameObject gameClearPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject numberBoxPrefab;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject titlePanel;
        [SerializeField] private List<Button> operatorButtons;
        [SerializeField] public List<NumberBox> numberBoxes;
        [SerializeField] public NumberBox answerNumberBox;
        [SerializeField] private Slider diceMaxSlider;
        [SerializeField] private TMP_Text diceMaxText;
        [SerializeField] private Slider diceCountSlider;
        [SerializeField] private TMP_Text diceCountText;
        [SerializeField] private TMP_Text formulaText;
        [SerializeField] private TMP_Text solutionText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text resultScoreText;
        [SerializeField] private TMP_Text resultTimeText;
        [SerializeField] private TMP_Text comboCountText;
        [SerializeField] private Transform canvasTransform;
        [SerializeField] private UGUILineRenderer lineRenderer;
        private const float MessageDisplayDuration = 1f;
        private const int DisplayedSolutionCount = 3;

        public IUniTaskAsyncEnumerable<AsyncUnit> BackButtonOnClickAsAsyncEnumerable()
        {
            return undoStepButton.OnClickAsAsyncEnumerable();
        }

        public void ClearLine()
        {
            lineRenderer.SetPositions(new[] { Vector2.zero, Vector2.zero });
        }

        public void DrawLine(Vector2 startPoint, Vector2 endPoint)
        {
            var positions = new[] { startPoint, endPoint };
            lineRenderer.SetPositions(positions);
        }

        public async UniTask EndGameAnimationAsync(bool isCleared, CancellationToken gameCt)
        {
            resultScoreText.text = $"ポイント:{GameState.Score}";
            scoreText.text = $"ポイント:{GameState.Score}";
            RectTransform rectTransform;
            if (isCleared)
            {
                rectTransform = gameClearPanel.GetComponent<RectTransform>();
                gameClearPanel.SetActive(true);

                resultTimeText.text = $"じかん:{GameState.CurrentTime.Value:F2}";
            }
            else
            {
                rectTransform = gameOverPanel.GetComponent<RectTransform>();
                gameOverPanel.SetActive(true);

                solutionText.text = "Solutions:";

                var randomSolutions = GetRandomElements(GameState.FormulaStrings,
                    Mathf.Min(DisplayedSolutionCount, GameState.FormulaStrings.Count));
                solutionText.text += string.Join("\n", randomSolutions);
            }

            rectTransform.localPosition = new Vector2(0, Screen.height);
            await rectTransform.DOMove(Vector2.zero, 0.8f).ToUniTask(cancellationToken: gameCt);


            resultScoreText.gameObject.SetActive(true);
            restartButton.gameObject.SetActive(true);

            if (isCleared)
            {
                comboCounterImage.SetActive(false);
                comboCounterImage.transform.localScale = new Vector2(1.5f, 1.5f);
                comboCountText.text = GameState.ComboCount.ToString();
                comboCounterImage.SetActive(true);
                await comboCounterImage.transform.DOScale(new Vector2(1, 1), 0.8f).ToUniTask(cancellationToken: gameCt);
            }

            await restartButton.OnClickAsync(gameCt);
        }

        private List<T> GetRandomElements<T>(List<T> sourceList, int count)
        {
            var shuffledList = sourceList.OrderBy(_ => Guid.NewGuid()).ToList();
            var result = shuffledList.Take(count).ToList();
            return result;
        }

        public void HideAll()
        {
            ClearLine();
            HideOperators();
        }

        private void HideOperators()
        {
            foreach (var operatorButton in operatorButtons)
            {
                operatorButton.gameObject.SetActive(false);
            }
        }

        public void InitializePuzzle()
        {
            titlePanel.SetActive(false);
            HideAll();

            var diceCount = GameInitialData.Instance.diceCount;
            diceCountSlider.value = diceCount;
            diceMaxSlider.value = GameInitialData.Instance.maxDiceValue;

            if (numberBoxes.Count != diceCount)
            {
                PlaceNumberBox(diceCount);
            }

            numberBoxes.ForEach(numberBox => numberBox.ShowNumberBox());
            gameClearPanel.SetActive(false);
            gameOverPanel.SetActive(false);
            comboCounterImage.SetActive(false);
            restartButton.gameObject.SetActive(false);
            resultScoreText.gameObject.SetActive(false);
            formulaText.text = "";
        }

        private void PlaceNumberBox(int diceCount)
        {
            numberBoxes.ForEach(numberBox => Destroy(numberBox.gameObject));
            numberBoxes.Clear();
            for (var i = 0; i < diceCount; i++)
            {
                const int let = 310;
                var theta = 360 / diceCount * i;
                var position = new Vector2(Mathf.Sin(theta * Mathf.Deg2Rad) * let,
                    Mathf.Cos(theta * Mathf.Deg2Rad) * let);
                if (i > diceCount / 2)
                {
                    theta = 360 - theta;
                    position = new Vector2(-(Mathf.Sin(theta * Mathf.Deg2Rad) * let),
                        Mathf.Cos(theta * Mathf.Deg2Rad) * let);
                }

                var scale = 1 - (diceCount - 5) * 0.03f;

                var instantiatedNumberBox = Instantiate(numberBoxPrefab, canvasTransform);
                instantiatedNumberBox.transform.SetAsFirstSibling();
                var rectTransform = instantiatedNumberBox.GetComponent<RectTransform>();
                rectTransform.localScale = new Vector2(scale, scale);
                var numberBox = instantiatedNumberBox.GetComponent<NumberBox>();
                numberBox.initialPosition = position;

                numberBoxes.Add(numberBox);
            }

            answerNumberBox.transform.SetAsFirstSibling();
        }

        public async UniTask RetireButtonOnClickAsync(CancellationToken gameCt)
        {
            await retireButton.OnClickAsync(gameCt);
        }

        public void SetTimerText(float time)
        {
            timeText.text = $"じかん:{time:F2}";
        }

        public async UniTask ShowMessageAsync()
        {
            messagePanel.SetActive(true);
            await UniTask.Delay(TimeSpan.FromSeconds(MessageDisplayDuration));
            messagePanel.SetActive(false);
        }

        public void UpdateFormulaText(string formulaString)
        {
            formulaText.text = formulaString;
        }

        public async UniTask<OperatorMark> SelectOperatorAsync(bool[] isCalculable, CancellationToken gameCt)
        {
            var selectOperatorCts = new CancellationTokenSource();
            var mergedCts = CancellationTokenSource.CreateLinkedTokenSource(gameCt, selectOperatorCts.Token);
            ShowOperatorSymbolsAsync(isCalculable, mergedCts.Token).Forget();

            var tasks = operatorButtons.Select(button => button.OnClickAsync(mergedCts.Token));
            var result = await UniTask.WhenAny(tasks);

            selectOperatorCts.Cancel();
            HideOperators();
            return (OperatorMark)result;
        }

        public IUniTaskAsyncEnumerable<AsyncUnit> SettingButtonOnClickAsAsyncEnumerable()
        {
            return settingsButton.OnClickAsAsyncEnumerable();
        }

        private async UniTask ShowOperatorSymbolsAsync(bool[] canCalculate, CancellationToken gameCt)
        {
            for (var i = 0; i < 5; i++)
            {
                operatorButtons[i].gameObject.SetActive(canCalculate[i]);
                operatorButtons[i].transform.localRotation = Quaternion.Euler(0, 0, 20);
                await operatorButtons[i].transform.DORotate(new Vector3(0, 0, 0), 0.1f)
                    .ToUniTask(cancellationToken: gameCt);
            }
        }

        public async UniTask ProcessSettingsAsync(CancellationToken gameCt)
        {
            using (diceCountSlider.OnValueChangedAsAsyncEnumerable().Subscribe(value =>
                       diceCountText.text = value.ToString(CultureInfo.InvariantCulture)))
            {
                using (diceMaxSlider.OnValueChangedAsAsyncEnumerable()
                           .Subscribe(value => diceMaxText.text = value.ToString(CultureInfo.InvariantCulture)))
                {
                    settingsPanel.SetActive(true);

                    await exitSettingsButton.OnClickAsync(gameCt);
                    
                    GameInitialData.Instance.maxDiceValue = (int)diceMaxSlider.value;
                    GameInitialData.Instance.diceCount = (int)diceCountSlider.value;
                    
                    settingsPanel.SetActive(false);
                    
                    await PlayerDataManager.SavePlayerDataAsync(
                        new PlayerData(GameState.Score, GameState.ComboCount, GameInitialData.Instance.diceCount,
                            GameInitialData.Instance.maxDiceValue), gameCt);
                }
            }
        }

        public async UniTask MoveToCenterAsync(NumberBox numberBox, CancellationToken gameCt)
        {
            await numberBox.MoveToCenterAsync(answerNumberBox.GetComponent<RectTransform>().position, gameCt);
        }
    }
}
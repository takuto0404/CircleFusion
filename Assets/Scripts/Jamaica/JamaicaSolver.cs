using System;
using System.Collections.Generic;
using System.Linq;

namespace Jamaica
{
    public class FormulaInfo
    {
        public FormulaInfo(int number,string solutionString,OperatorMark beforeOperatorSymbol)
        {
            Number = number;
            SolutionString = solutionString;
            BeforeOperatorSymbol = beforeOperatorSymbol;
        }
        public int Number;
        public string SolutionString;
        public OperatorMark BeforeOperatorSymbol;
    }
    public static class JamaicaSolver
    {
        private static readonly Dictionary<OperatorMark, string> OperatorDic = new()
        {
            { OperatorMark.Plus, "+" },
            { OperatorMark.Minus, "-" },
            { OperatorMark.Times, "*" },
            { OperatorMark.Devided, "/" },
            { OperatorMark.None , ""}
        };

        private static readonly Stack<FormulaInfo[]> FormulaHist =
            new();

        private static FormulaInfo[] _originalDices;
        private static List<string> _solutions;
        private static int _targetAnswer;

        public static (bool isSolvable, List<string> solutionStrings) SolveJamaica(int targetAnswer, int[] diceNumbers)
        {
            _solutions = new List<string>();
            _originalDices = diceNumbers.Select(diceNumber => new FormulaInfo(diceNumber, diceNumber.ToString(), OperatorMark.None)).ToArray();
            FormulaHist.Push(_originalDices);
            _targetAnswer = targetAnswer;

            Solve();
            return (_solutions.Count > 0, _solutions);
        }

        private static void Solve()
        {
            var lastFormula = FormulaHist.Peek();
            var subscribed1 = new List<int>();
            for (var i = 0; i < _originalDices.Length; i++)
            {
                if (lastFormula[i].Number == -1 || subscribed1.Contains(lastFormula[i].Number)) continue;
                subscribed1.Add(lastFormula[i].Number);
                var subscribed2 = new List<int>();
                for (var l = 0; l < _originalDices.Length; l++)
                {
                    if (i == l || lastFormula[l].Number == -1 || lastFormula[i].Number == -1 ||
                        subscribed2.Contains(lastFormula[i].Number) ||
                        lastFormula[i].Number < lastFormula[l].Number) continue;
                    subscribed2.Add(lastFormula[i].Number);
                    for (var operatorNumber = 0; operatorNumber < 4; operatorNumber++)
                    {
                        var hist = CopyArray(lastFormula);
                        var beforeFirstFormula = hist[i];
                        var beforeSecondFormula = hist[l];

                        var firstFormula = hist[i];
                        var secondFormula = hist[l];
                        var isCalculable = new []
                        {
                            true, firstFormula.Number <= secondFormula.Number, 
                            true,
                            (float)firstFormula.Number / (float)secondFormula.Number % 1 == 0 && secondFormula.Number != 0 &&
                            secondFormula.Number != 1
                        };

                        if (!isCalculable[operatorNumber]) continue;
                        
                        if ((beforeFirstFormula.BeforeOperatorSymbol is OperatorMark.Plus or OperatorMark.Times && beforeFirstFormula.BeforeOperatorSymbol == (OperatorMark)operatorNumber) ||
                            (beforeSecondFormula.BeforeOperatorSymbol is OperatorMark.Plus or OperatorMark.Times &&
                             beforeSecondFormula.BeforeOperatorSymbol == (OperatorMark)operatorNumber))
                        {
                            if (beforeFirstFormula.Number < secondFormula.Number)
                            {
                                continue;
                            }
                        }

                        if (beforeFirstFormula.BeforeOperatorSymbol == OperatorMark.Devided && beforeSecondFormula.BeforeOperatorSymbol == OperatorMark.Times)
                        {
                            continue;
                        }

                        firstFormula.Number = CalculateResult(firstFormula.Number, secondFormula.Number,
                            (OperatorMark)operatorNumber);

                        CreateFormulaText(beforeFirstFormula,beforeSecondFormula,firstFormula,(OperatorMark)operatorNumber);

                        firstFormula.BeforeOperatorSymbol = (OperatorMark)operatorNumber;

                        secondFormula.SolutionString = "";
                        secondFormula.Number = -1;
                        secondFormula.BeforeOperatorSymbol = OperatorMark.None;

                        hist[i] = firstFormula;
                        hist[l] = secondFormula;
                        FormulaHist.Push(hist);

                        var activeNumbers = hist.Where(x => x.Number != -1).ToArray();
                        if (activeNumbers.Length == 1)
                        {
                            if (activeNumbers.First().Number == _targetAnswer)
                            {
                                var text = $"{activeNumbers.First().SolutionString} = {_targetAnswer}";
                                if (_solutions.Contains(text)) continue;
                                _solutions.Add(text);
                            }
                        }

                        Solve();

                        FormulaHist.Pop();
                    }
                }
            }
        }

        private static int CalculateResult(int firstNumber, int secondNumber, OperatorMark operatorMark)
        {
            return operatorMark switch
            {
                OperatorMark.Plus => firstNumber + secondNumber,
                OperatorMark.Minus => firstNumber - secondNumber,
                OperatorMark.Times => firstNumber * secondNumber,
                OperatorMark.Devided => firstNumber / secondNumber,
                _ => -1
            };
        }
        
        private static FormulaInfo[] CopyArray(FormulaInfo[] originalArray)
        {
            var newArray = new FormulaInfo[originalArray.Length];
            Array.Copy(originalArray, newArray, originalArray.Length);
            return newArray;
        }

        private static void CreateFormulaText(FormulaInfo beforeFirstFormula,FormulaInfo beforeSecondFormula,FormulaInfo firstFormula,OperatorMark operatorSymbol)
        {
            if (operatorSymbol is OperatorMark.Times or OperatorMark.Devided)
            {
                firstFormula.SolutionString = beforeFirstFormula.BeforeOperatorSymbol is OperatorMark.Plus or OperatorMark.Minus ? $"({beforeFirstFormula.SolutionString})" : beforeFirstFormula.SolutionString;
                
                firstFormula.SolutionString += OperatorDic[operatorSymbol];
                
                if (beforeSecondFormula.BeforeOperatorSymbol is OperatorMark.Plus or OperatorMark.Minus)
                {
                    firstFormula.SolutionString += $"({beforeSecondFormula.SolutionString})";
                }
                else
                {
                    firstFormula.SolutionString += beforeSecondFormula.SolutionString;
                }
            }
            else
            {
                firstFormula.SolutionString = $"{beforeFirstFormula.SolutionString} {OperatorDic[operatorSymbol]} {beforeSecondFormula.SolutionString}";
            }
        }
    }
}
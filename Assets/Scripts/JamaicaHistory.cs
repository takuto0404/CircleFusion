using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public static class JamaicaHistory
{
    /// <summary>
    /// そのパズル上でどのようにボックスが動かされていったかの履歴
    /// </summary>
    private static List<Hist> _histories;

    public static void PuzzleInit()
    {
        _histories = new List<Hist>();
    }

    public static Hist LastHist()
    {
        return _histories[^1];
    }

    public static void SetInitHist(List<Dice> dices)
    {
        _histories.Add(new Hist(dices,new List<Formula>()));
    }

    public static void SetHist(List<Dice> dices, Formula formula)
    {
        var lastFormulas = _histories[^1].Formula;
        lastFormulas.Add(formula);
        _histories.Add(new Hist(dices,lastFormulas));
    }
    
    public static Hist BackHist()
    {
        if (_histories.Count == 0) return null;
        Debug.Log(_histories.Count);
        _histories.RemoveAt(_histories.Count - 1);
        return _histories[^1];
    }
}
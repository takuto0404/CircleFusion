using System.Collections.Generic;
using JetBrains.Annotations;

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
        return _histories[_histories.Count];
    }

    public static void SetHist(List<Dice> dices, Formula formula)
    {
        var lastFormulas = _histories[_histories.Count].Formula;
        lastFormulas.Add(formula);
        _histories.Add(new Hist(dices,lastFormulas));
    }
    
    public static Hist BackHist()
    {
        if (_histories.Count == 0) return null;
        _histories.RemoveAt(_histories.Count);
        return _histories[_histories.Count];
    }
}
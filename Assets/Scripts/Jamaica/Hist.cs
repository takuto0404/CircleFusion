using System.Collections.Generic;
using System.Linq;

namespace Jamaica
{
    public class DiceInfoClass
    {
        public DiceInfoClass(Dice dice)
        {
            DiceInfo.isActive = dice.IsActive;
            DiceInfo.diceNumber = dice.DiceNumber.Value;
        }
        public (bool isActive, int diceNumber) DiceInfo;
    }
    
    public class Hist
    {
        public Hist(List<Dice> dices,string formulaText)
        {
            Dices = dices.Select(dice => new DiceInfoClass(dice)).ToList();
            FormulaText = formulaText;
        }
        public readonly List<DiceInfoClass> Dices;
        public readonly string FormulaText;
    }
}
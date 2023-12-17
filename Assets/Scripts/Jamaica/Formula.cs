namespace Jamaica
{
    public class Formula
    {
        public Formula(int firstDice ,int secondDice,OperatorMark operatorSymbol,int result)
        {
            FirstDice = firstDice;
            SecondDice = secondDice;
            Result = result;
            OperatorSymbol = operatorSymbol;
        }
        public readonly int FirstDice;
        public readonly int SecondDice;
        public readonly int Result;
        public readonly OperatorMark OperatorSymbol;
    
    }
}
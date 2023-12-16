namespace Jamaica
{
    public class Formula
    {
        public Formula(int firstDice ,int secondDice,OperatorMark operatorSymbol,int result)
        {
            FirstDice = firstDice;
            SecondDice = secondDice;
            OperatorSymbol = operatorSymbol;
            Result = result;
        }
        public readonly int FirstDice;
        public readonly int SecondDice;
        public readonly OperatorMark OperatorSymbol;
        public readonly int Result;
    
    }
}
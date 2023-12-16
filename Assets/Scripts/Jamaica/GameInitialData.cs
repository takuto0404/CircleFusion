using UnityEngine;

namespace Jamaica
{
    public class GameInitialData : SingletonMonoBehaviour<GameInitialData>
    {
        [SerializeField] public int rollTime;
        [SerializeField] public int maxDiceValue;
        [SerializeField] public int diceCount;

        public void SetInformation(int customMaxDiceValue, int customDiceCount)
        {
            maxDiceValue = customMaxDiceValue;
            diceCount = customDiceCount;
        }
    }
}
using CircleFusion.Share;
using UnityEngine;

namespace CircleFusion.InGame
{
    public class GameInitialData : SingletonMonoBehaviour<GameInitialData>
    {
        [SerializeField] public int rollTime;
        public int maxDiceValue;
        public int diceCount;

        public void SetInformation(int customMaxDiceValue, int customDiceCount)
        {
            maxDiceValue = customMaxDiceValue;
            diceCount = customDiceCount;
        }
    }
}
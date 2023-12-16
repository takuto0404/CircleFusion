using UnityEngine;

namespace Jamaica
{
    public class GameInitialData : SingletonMonoBehaviour<GameInitialData>
    {
        [SerializeField] public int rollTime;
        [SerializeField] public int maxDiceFace;
        [SerializeField] public int diceCount;
    }
}
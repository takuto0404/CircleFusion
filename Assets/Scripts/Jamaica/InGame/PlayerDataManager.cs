using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using MemoryPack;
using UnityEngine;

namespace Jamaica.InGame
{
    public abstract class PlayerDataManager
    {
        private const int DefaultDiceCount = 5;
        private const int DefaultDiceMax = 6;
        private const string FileName = "SaveData.txt";
        private static string Path => Application.persistentDataPath;
        public static PlayerData PlayerData;
        
        public static async UniTask SavePlayerDataAsync(PlayerData playerData, CancellationToken gameCt)
        {
            PlayerData = playerData;
            var path = System.IO.Path.Combine(Path, FileName);
            await File.WriteAllBytesAsync(path, MemoryPackSerializer.Serialize(playerData), gameCt).AsUniTask();
        }

        public static async UniTask LoadPlayerDataAsync(CancellationToken gameCt)
        {
            try
            {
                var bytes = await File.ReadAllBytesAsync(System.IO.Path.Combine(Path, FileName), gameCt).AsUniTask();
                PlayerData = MemoryPackSerializer.Deserialize<PlayerData>(bytes);
            }
            catch
            {
                PlayerData = new PlayerData(0, 0, DefaultDiceCount, DefaultDiceMax);
            }
        }
    }
}
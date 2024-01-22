using System;
using System.IO;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using MemoryPack;
using UnityEngine;

namespace CircleFusion.InGame
{
    public abstract class PlayerDataManager
    {
        private const int DefaultDiceCount = 5;
        private const int DefaultDiceMax = 6;
        private const string FileName = "SaveData.txt";
        private static string DataPath => Application.persistentDataPath;
        public static PlayerData PlayerData;
        
        public static void SavePlayerDataAsync(PlayerData playerData, CancellationToken gameCt)
        {
            PlayerData = playerData;
            var path = Path.Combine(DataPath, FileName);
            var data = MemoryPackSerializer.Serialize(playerData).ToString();
            PlayerPrefs.SetString("Data",data);
        }

        public static void LoadPlayerDataAsync(CancellationToken gameCt)
        {
            try
            {
                var path = Path.Combine(DataPath, FileName);
                var bytes = Encoding.GetEncoding("UTF-8").GetBytes(PlayerPrefs.GetString("Data"));
                PlayerData = MemoryPackSerializer.Deserialize<PlayerData>(bytes);
            }
            catch
            {
                PlayerData = new PlayerData(0, 0, DefaultDiceCount, DefaultDiceMax);
            }
        }
    }
}
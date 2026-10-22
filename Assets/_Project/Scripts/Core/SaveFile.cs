using System.IO;
using UnityEngine;

namespace CrimeLife.Core
{
    public static class SaveFile
    {
        private static string FilePath => Path.Combine(Application.persistentDataPath, "save.json");

        public static string Serialize(SaveData data) => JsonUtility.ToJson(data, true);

        public static SaveData Deserialize(string json) => JsonUtility.FromJson<SaveData>(json);

        public static void Write(SaveData data) => File.WriteAllText(FilePath, Serialize(data));

        public static bool TryRead(out SaveData data)
        {
            data = null;
            if (!File.Exists(FilePath)) return false;

            try
            {
                data = Deserialize(File.ReadAllText(FilePath));
            }
            catch (System.Exception)
            {
                // 깨진 저장 파일 때문에 게임이 멈추지 않게 없는 것으로 취급한다
                data = null;
            }
            return data != null && !string.IsNullOrEmpty(data.SceneName);
        }
    }
}

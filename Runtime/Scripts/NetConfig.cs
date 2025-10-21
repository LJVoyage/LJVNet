using UnityEngine;

namespace LJVoyage.LJVNet.Runtime
{
    [CreateAssetMenu(fileName = "NetConfig", menuName = "LJV/Net/Config")]
    public class NetConfig : ScriptableObject
    {
        public string baseUrl = "https://api.example.com/";
        public string token;
        public int timeout = 10;
        public bool logRequest = true;
        public bool logResponse = true;

        private static NetConfig _instance;

        public static NetConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<NetConfig>("NetConfig");
                    if (_instance == null)
                        Debug.LogWarning("⚠️ NetAPIConfig 未找到，请放入 Resources/NetAPIConfig.asset");
                }

                return _instance;
            }
        }
    }
}
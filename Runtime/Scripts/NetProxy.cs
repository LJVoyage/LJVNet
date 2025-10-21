using System.Collections;
using UnityEngine;

namespace LJVoyage.LJVNet.Runtime
{
    public class NetProxy : MonoBehaviour
    {
        private static NetProxy _instance;

        public static NetProxy Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[NetProxy]");
                    _instance = go.AddComponent<NetProxy>();
                    DontDestroyOnLoad(go);
                }

                return _instance;
            }
        }

        /// <summary>
        /// 启动一个网络请求协程
        /// </summary>
        public void RunCoroutine(IEnumerator routine)
        {
            StartCoroutine(routine);
        }

        /// <summary>
        /// 取消协程
        /// </summary>
        public void StopCoroutineSafe(Coroutine c)
        {
            if (c != null)
                StopCoroutine(c);
        }
    }
}
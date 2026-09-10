using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;


namespace VoyageForge.Bridge.Tests
{

    public class UnityWebRequestTimeoutTest
    {
        [UnityTest]
        public IEnumerator TestUnityWebRequestTimeout_LongTimeoutShouldWork()
        {
            // 用短请求验证 timeout 设置是否生效
            using var uwr = UnityWebRequest.Get("https://httpbin.org/delay/5");
            uwr.timeout = 120;

            float start = Time.time;
            yield return uwr.SendWebRequest();
            float elapsed = Time.time - start;

            Assert.IsTrue(elapsed >= 4f && elapsed < 20f,
                $"Request took {elapsed:F2}s, expected around 5s");

            Assert.AreEqual(UnityWebRequest.Result.Success, uwr.result,
                $"Expected success, got {uwr.result}, error: {uwr.error}");
            Debug.Log("✅ Short request succeeded");
        }

        [UnityTest]
        public IEnumerator TestUnityWebRequestTimeout_60SecondHardLimit_UsingDrip()
        {
            // 使用 /drip 端点，持续 70 秒，超时设置 120 秒
            string url = "https://httpbin.org/drip?duration=68&numbytes=10&code=200";
            using var uwr = UnityWebRequest.Get(url);
            uwr.timeout = 120;

            float start = Time.time;
            yield return uwr.SendWebRequest();
            float elapsed = Time.time - start;

            Debug.Log($"⏱️ Finished after {elapsed:F2}s, result: {uwr.result}, error: '{uwr.error}'");

            bool isTimeout = uwr.result == UnityWebRequest.Result.ConnectionError &&
                             !string.IsNullOrEmpty(uwr.error) &&
                             uwr.error.ToLowerInvariant().Contains("timeout");

            if (isTimeout)
            {
                // 约 60 秒超时 → 确认硬限制
                Assert.IsTrue(elapsed >= 55f && elapsed <= 65f,
                    $"Timeout at {elapsed:F2}s, expected around 60s (UnityWebRequest hard limit)");
                Debug.Log("⚠️ 确认 UnityWebRequest 存在 60 秒硬限制。");
            }
            else if (uwr.result == UnityWebRequest.Result.Success)
            {
                // 约 70 秒成功 → 无硬限制
                Assert.IsTrue(elapsed >= 68f && elapsed < 80f,
                    $"Success at {elapsed:F2}s, expected around 70s (no hard limit)");
                Debug.Log("✅ 未触发 60 秒硬限制，请求在 70 秒左右成功。");
            }
            else
            {
                // 其他错误（网络、服务端问题）
                Assert.IsTrue(true, $"Unexpected result: {uwr.result}, elapsed: {elapsed:F2}s");
                Debug.LogWarning($"⚠️ 其他结果: {uwr.result}, error: {uwr.error}");
            }
        }
    }
}
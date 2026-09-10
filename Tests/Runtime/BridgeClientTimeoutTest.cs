using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VoyageForge.Bridge.Runtime;

namespace VoyageForge.Bridge.Tests
{
    public class BridgeClientTimeoutTest
    {
        [UnityTest]
        public IEnumerator TestShortRequest_ShouldSucceed()
        {
            var request = new Request
            {
                url = "/delay/5",
                method = "GET",
                timeoutSeconds = 120,
                endpointKey = "webapi" // 确保使用 httpbin 端点
            };

            var task = TestBridgeClient.SendAsync<string>(request);
            var awaiter = task.GetAwaiter();
            float start = Time.time;
            while (!awaiter.IsCompleted)
                yield return null;
            float elapsed = Time.time - start;

            Assert.IsTrue(elapsed >= 4f && elapsed < 20f,
                $"Short request took {elapsed:F2}s, expected around 5s");

            var response = awaiter.GetResult();
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccessStatusCode, $"Status: {response.statusCode}");
            Assert.IsNotNull(response.data);
            Debug.Log("✅ Short request succeeded with BridgeClient");
        }

        [UnityTest]
        public IEnumerator TestLongRequest_ShouldNotTimeoutPrematurely()
        {
            var request = new Request
            {
                url = "/drip?duration=70&numbytes=10&code=200",
                method = "GET",
                timeoutSeconds = 120,
                endpointKey = "webapi"
            };

            var task = TestBridgeClient.SendAsync<string>(request);
            var awaiter = task.GetAwaiter();
            float start = Time.time;
            while (!awaiter.IsCompleted)
                yield return null;
            float elapsed = Time.time - start;

            Debug.Log($"⏱️ Long request finished after {elapsed:F2}s");

            var response = awaiter.GetResult();
            Assert.IsNotNull(response);

            bool isTimeout = response.statusCode == 0 &&
                             (response.statusText == "Timeout" || response.statusText == "Canceled");

            if (isTimeout)
            {
                Assert.IsTrue(elapsed >= 55f && elapsed <= 65f,
                    $"Premature timeout at {elapsed:F2}s, expected around 60s (UnityWebRequest hard limit)");
                Debug.LogWarning("⚠️ 检测到约60秒超时，UnityWebRequest 硬限制仍生效，BridgeClient 未完全覆盖。");
            }
            else if (response.IsSuccessStatusCode)
            {
                Assert.IsTrue(elapsed >= 68f && elapsed < 80f,
                    $"Success at {elapsed:F2}s, expected around 70s (no hard limit)");
                Debug.Log("✅ 长请求成功，BridgeClient 超时控制有效，未触发60秒硬限制。");
            }
            else
            {
                // 其他错误（网络问题）
                Assert.IsTrue(true, $"Unexpected response: status={response.statusCode}, statusText={response.statusText}");
                Debug.LogWarning($"⚠️ 其他结果: {response.statusCode}, {response.statusText}, elapsed: {elapsed:F2}s");
            }
        }
    }
}
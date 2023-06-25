using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ChatGPTWrapper;
using System.Threading.Tasks;
using System;
using PlasticGui.WorkspaceWindow;
using System.Net;

namespace Reqs {

    public class Requests
    {
        private void SetHeaders(ref UnityWebRequest req, List<(string, string)> headers)
        {
            for (int i = 0; i < headers.Count; i++)
            {
                req.SetRequestHeader(headers[i].Item1, headers[i].Item2);
            }
        }

        public Task<T> GetRequestAsync<T>(string URI, Action<T> Complete, List<(string, string)> Headers = null)
        {
            var TCS = new TaskCompletionSource<T>();

            UnityWebRequest WebRequest = new UnityWebRequest(URI, "GET");
            if (Headers != null) SetHeaders(ref WebRequest, Headers);
            WebRequest.downloadHandler = new DownloadHandlerBuffer();
            WebRequest.disposeUploadHandlerOnDispose = true;
            WebRequest.disposeDownloadHandlerOnDispose = true;

            async void Callback(AsyncOperation AsyncOp)
            {
                await Task.Delay(0);

                switch (WebRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        TCS.SetException(new Exception("Error: " + WebRequest.error));
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        TCS.SetException(new Exception("HTTP Error: " + WebRequest.error));
                        break;
                    case UnityWebRequest.Result.Success:
                        var responseJson = JsonUtility.FromJson<T>(WebRequest.downloadHandler.text);
                        Complete(responseJson);
                        TCS.SetResult(responseJson);
                        break;
                }

                WebRequest.Dispose();
            }

            WebRequest.SendWebRequest().completed += Callback;

            return TCS.Task;
        }

        public IEnumerator GetReq<T>(string uri, System.Action<T> callback, List<(string, string)> headers = null)
        {
            UnityWebRequest webRequest = new UnityWebRequest(uri, "GET");
            if (headers != null) SetHeaders(ref webRequest, headers);
            webRequest.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
            webRequest.disposeUploadHandlerOnDispose = true;
            webRequest.disposeDownloadHandlerOnDispose = true;

            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    var responseJson = JsonUtility.FromJson<T>(webRequest.downloadHandler.text);
                    callback(responseJson);
                    break;
            }

            webRequest.Dispose();
            
        }

        public Task<T> PostRequestAsync<T>(string URI, string JSON, Action<T> Complete, List<(string, string)> Headers = null)
        {
            var TCS = new TaskCompletionSource<T>();

            UnityWebRequest WebRequest = new UnityWebRequest(URI, "POST");
            if (Headers != null) SetHeaders(ref WebRequest, Headers);

            byte[] JSON_ToSend = new System.Text.UTF8Encoding().GetBytes(JSON);
            WebRequest.uploadHandler = new UploadHandlerRaw(JSON_ToSend);
            WebRequest.downloadHandler = new DownloadHandlerBuffer();
            WebRequest.disposeDownloadHandlerOnDispose = true;
            WebRequest.disposeUploadHandlerOnDispose = true;

            async void Callback(AsyncOperation AsyncOp)
            {
                await Task.Delay(0);

#if UNITY_2020_3_OR_NEWER
                switch (WebRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        TCS.SetException(new Exception($"Error: {WebRequest.error}"));
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        TCS.SetException(new Exception($"HTTP Error: {WebRequest.error}"));
                        if (WebRequest.error == "HTTP/1.1 429 Too Many Requests")
                        {
                            Debug.Log("Retrying...");
                            var Result = await PostRequestAsync<T>(URI, JSON, Complete, Headers);
                            TCS.SetResult(Result);
                        }
                        break;
                    case UnityWebRequest.Result.Success:
                        var responseJson = JsonUtility.FromJson<T>(WebRequest.downloadHandler.text);
                        Complete(responseJson);
                        TCS.SetResult(responseJson);
                        break;
                }
#else
                if (!string.IsNullOrWhiteSpace(WebRequest.error))
                {
                    TCS.SetException(new Exception($"Error {WebRequest.responseCode} - {WebRequest.error}"));
                }
                else 
                {
                    var ResponseJSON = JsonUtility.FromJson<T>(WebRequest.downloadHandler.text);
                    TCS.SetResult(ResponseJSON);
                }
#endif

                WebRequest.Dispose();
            }

            WebRequest.SendWebRequest().completed += Callback;

            return TCS.Task;
        }
        
        public IEnumerator PostReq<T>(string uri, string json, System.Action<T> callback, List<(string, string)> headers = null)
        {
            UnityWebRequest webRequest = new UnityWebRequest(uri, "POST");
            if (headers != null) SetHeaders(ref webRequest, headers);

            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            webRequest.disposeDownloadHandlerOnDispose = true;
            webRequest.disposeUploadHandlerOnDispose = true;

            yield return webRequest.SendWebRequest();

#if UNITY_2020_3_OR_NEWER
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("HTTP Error: " + webRequest.error);
                    if (uri.EndsWith("/completions")) {
                      var errJson = JsonUtility.FromJson<ChatGPTResError>(webRequest.downloadHandler.text);
                      Debug.LogError(errJson.error.message);
                      if (webRequest.error == "HTTP/1.1 429 Too Many Requests")
                      {
                          Debug.Log("retrying...");
                          yield return PostReq<T>(uri, json, callback, headers);
                      }
                    }
                    break;
                case UnityWebRequest.Result.Success:
                    var responseJson = JsonUtility.FromJson<T>(webRequest.downloadHandler.text);
                    callback(responseJson);
                    break;
            }
#else
            if(!string.IsNullOrWhiteSpace(webRequest.error))
            {
                Debug.LogError($"Error {webRequest.responseCode} - {webRequest.error}");
                yield break;
            }
            else
            {
                var responseJson = JsonUtility.FromJson<T>(webRequest.downloadHandler.text);
                    callback(responseJson);
            }
#endif

            webRequest.Dispose();
        }
    }
}

using UnityEngine;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace Ginkgo.Net
{
    /// <summary>
    /// 支持证书没部署正确的https
    /// </summary>
    public class WWW2 : CustomYieldInstruction
    {
        public static bool validateCert = false;

        public byte[] bytes
        {
            get
            {
                return m_bytes;
            }
        }

        public string text
        {
            get
            {
                if (m_bytes == null || m_bytes.Length == 0)
                {
                    throw new Exception("No valid data retrieved, can't visit text");
                }
                if (m_text == null)
                {
                    m_text = GeneralUtils.Bytes2Utf8String(bytes);
                }
                return m_text;
            }
        }

        public AssetBundle assetBundle
        {
            get
            {
                if (m_bytes == null || m_bytes.Length == 0)
                {
                    throw new Exception("No valid data retrieved, can't visit assetbundle");
                }
                if (m_assetbundle == null)
                {
                    m_assetbundle = AssetBundle.LoadFromMemory(m_bytes);
                }
                return m_assetbundle;
            }
        }

        public int bytesDownloaded { get; set; }
        public string error { get; set; }

        string url;
        string m_text;
        byte[] m_bytes;
        AssetBundle m_assetbundle;
        int contentLength;

        bool exception = false;

        class AsyncWebState
        {
            public HttpWebRequest webRequest;
            public WebResponse webResponse;
            public Stream readStream;
            public byte[] buffer = new byte[1024 * 1024 * 4];
            public MemoryStream ms = new MemoryStream();

            public void Close()
            {
                ms.Close();
                buffer = null;
            }
        }

        public override bool keepWaiting
        {
            get { return !exception && (contentLength <= 0 || bytesDownloaded < contentLength); }
        }

        public WWW2(string url)
            : this(url, null)
        {

        }

        bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

        public WWW2(string url, Dictionary<string, string> headers)
        {
            this.url = url;
            HttpWebRequest request;
            WebResponse response = null;
            Stream ns = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(url);
                if (url.StartsWith("https") && !validateCert)
                {
                    request.UseDefaultCredentials = true;
                    ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;
                }
                request.Method = "GET";
                if (headers != null)
                {
                    foreach (var h in headers)
                    {
                        request.Headers.Add(h.Key, h.Value);
                    }
                }
                //requestGetCount.Headers.Add("Accept-Encoding", "gzip, deflate");
                AsyncWebState state = new AsyncWebState();
                state.webRequest = request;
                request.BeginGetResponse(AsyncResponseCallBack, state);
            }
            catch (WebException e)
            {
                exception = true;
                error = string.Format("MyWWW-HttpWebRequest exception: {0}, url:{1}", e.Message, url);
                if (response != null)
                {
                    response.Close();
                }
                if (ns != null)
                {
                    ns.Close();
                }
            }
        }

        void AsyncResponseCallBack(IAsyncResult asyncResult)
        {
            AsyncWebState state = asyncResult.AsyncState as AsyncWebState;
            try
            {
                var response = state.webRequest.EndGetResponse(asyncResult);
                contentLength = (int)response.ContentLength;
                var ns = response.GetResponseStream();
                state.webResponse = response;
                state.readStream = ns;
                ns.BeginRead(state.buffer, 0, state.buffer.Length, AsyncReadCallback, state);
            }
            catch (Exception e)
            {
                error = string.Format("MyWWW-HttpWebRequest exception: {0}, url:{1}", e.Message, url);
                exception = true;
                state.Close();
            }

        }

        void AsyncReadCallback(IAsyncResult asyncResult)
        {
            AsyncWebState state = asyncResult.AsyncState as AsyncWebState;
            int readcn = state.readStream.EndRead(asyncResult);
            if (readcn == 0)
            {
                m_bytes = new byte[state.ms.Length];
                state.ms.Seek(0, SeekOrigin.Begin);
                state.ms.Read(m_bytes, 0, m_bytes.Length);
                contentLength = bytesDownloaded;
                state.Close();
                UnityEngine.Assertions.Assert.AreEqual(contentLength, m_bytes.Length, "这两个值应相等");
                state.readStream.Close();
                state.webResponse.Close();
            }
            else if (readcn > 0)
            {
                bytesDownloaded += readcn;
                state.ms.Write(state.buffer, 0, readcn);
                state.readStream.BeginRead(state.buffer, 0, state.buffer.Length, AsyncReadCallback, state);
            }
            else
            {
                throw new InvalidDataException("Down load Error...unkown - 1?");
            }
        }
    }

    internal class InvalidDataException : Exception
    {
        public InvalidDataException()
        {
        }

        public InvalidDataException(string message) : base(message)
        {
        }

        public InvalidDataException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

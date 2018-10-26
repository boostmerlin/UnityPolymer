using System;
using System.Collections;
using UnityEngine;
using Hash = System.Collections.Generic.Dictionary<string, string>;
#if UNITY_5_4_OR_NEWER
using UnityEngine.Networking;
#endif
using UniRx;
using System.IO;

/// <summary>
/// TODO: GetAudioClip GetMovieClip
/// </summary>

namespace Ginkgo
{
    public static class WWWLoader
    {
        public static string UrlRoot { get; set; }
        public static bool CacheAsFile { get; set; }
        static WWWLoader()
        {
            CacheAsFile = false;
            UrlRoot = string.Empty;
        }
        static byte[] decryption(byte[] binary)
        {
            byte[] decrypted = binary;
            return decrypted;
        }

        //public static void SetCurrentCache(Cache cache)
        //{
        //    Caching.currentCacheForWriting = cache;
        //}

        public static IDisposable GetBytes(string relativeUri, Action<byte[]> onNext, Action<Exception> onError = null, Hash headers = null, IProgress<float> progress = null)
        {
            string text = (string.Join("/", new string[] { UrlRoot, relativeUri }));
            IDisposable ret;
#if UNITY_5_4_OR_NEWER
            var www = UnityWebRequest.Get(text);
            if (headers != null)
            {
                foreach (var kv in headers)
                {
                    www.SetRequestHeader(kv.Key, kv.Value);
                }
            }
            var observable = Observable.FromCoroutine<byte[]>((observer, cancelToken) => Fetch(www, observer, progress, cancelToken, (handler) => handler.data));
            ret = observable.Subscribe(onNext, onError);
#else
            ret = ObservableWWW.GetAndGetBytes(text, headers, progress).Subscribe(onNext, onError);
#endif
            return ret;
        }

        static IEnumerator Fetch<T>(UnityWebRequest www, IObserver<T> observer, IProgress<float> reportProgress, CancellationToken cancel, Func<DownloadHandler, T> targetGetFunc)
        {
            using (www)
            {
                var operation = www.SendWebRequest();
                if (reportProgress != null)
                {
                    while (!www.isDone && !cancel.IsCancellationRequested)
                    {
                        try
                        {
                            reportProgress.Report(www.downloadProgress);
                        }
                        catch (Exception ex)
                        {
                            observer.OnError(ex);
                            yield break;
                        }
                        yield return null;
                    }
                }
                else
                {
                    if (!www.isDone)
                    {
                        yield return operation;
                    }
                }

                if (cancel.IsCancellationRequested)
                {
                    yield break;
                }

                if (reportProgress != null)
                {
                    try
                    {
                        reportProgress.Report(www.downloadProgress);
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                        yield break;
                    }
                }

                if (www.isHttpError || www.isNetworkError)
                {
                    observer.OnError(new MyUnityWebException(www.error));
                }
                else
                {
                    observer.OnNext(targetGetFunc(www.downloadHandler));
                    observer.OnCompleted();
                }
            }
        }

        static void checkSaveAsFile(string path, byte[] datas)
        {
            if (CacheAsFile)
            {
                FileInfo fi = new FileInfo(path);
                if (!fi.Directory.Exists)
                {
                    fi.Directory.Create();
                }
                var fs = fi.OpenWrite();
                fs.Write(datas, 0, datas.Length);
            }
        }

        static bool localLoad<T>(string path, IProgress<float> progress, Action<T> onNext, Func<byte[], T> loadFunc)
        {
            if (File.Exists(path))
            {
                T t = loadFunc(File.ReadAllBytes(path));
                onNext(t);
                if (progress != null)
                {
                    progress.Report(1);
                }
                return true;
            }
            return false;
        }

        public static IDisposable GetTexture(string relativeUri, Action<Texture2D> onNext, bool readable = true, Action<Exception> onError = null, Hash headers = null, IProgress<float> progress = null)
        {
            IDisposable dispose = Disposable.Empty;
            string path = Path.Combine(FileUtils.PersistentDataPath, relativeUri);
            if (localLoad<Texture2D>(path, progress, onNext, (bytes) =>
             {
                 Texture2D t = new Texture2D(1, 1);
                 t.LoadImage(bytes);
                 return t;
             }))
            {
                return dispose;
            }

#if UNITY_5_4_OR_NEWER
            string text = (string.Join("/", new string[] { UrlRoot, relativeUri }));
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(text, !readable);
            if (headers != null)
            {
                foreach (var kv in headers)
                {
                    www.SetRequestHeader(kv.Key, kv.Value);
                }
            }
            var observable = Observable.FromCoroutine<Texture2D>((observer, cancelToken) => Fetch(www, observer, progress, cancelToken
                , (handler) =>
                {
                    DownloadHandlerTexture texhandler = handler as DownloadHandlerTexture;
                    if (CacheAsFile)
                    {
                        checkSaveAsFile(path, texhandler.data);
                    }
                    return texhandler.texture;
                }));
            dispose = observable.Subscribe(onNext, onError);
#else
            dispose = GetBytes(relativeUri, (bytes) =>
            {
                if(bytes == null)
                {
                    return;
                }
                checkSaveAsFile(path, bytes);
                Texture2D t = new Texture2D(1, 1);
                t.LoadImage(bytes);
                onNext(t);
            }
            , onError, headers, progress
            );
#endif
            return dispose;
        }


        public static IDisposable GetText(string relativeUri, Action<string> onNext, Action<Exception> onError = null, Hash headers = null, IProgress<float> progress = null)
        {
            IDisposable dispose = Disposable.Empty;
            string path = Path.Combine(FileUtils.PersistentDataPath, relativeUri);
            if (localLoad<string>(path, progress, onNext, (bytes) =>
            {
                string t = GeneralUtils.Bytes2Utf8String(bytes);
                return t;
            }))
            {
                return dispose;
            }
#if UNITY_5_4_OR_NEWER
            string text = (string.Join("/", new string[] { UrlRoot, relativeUri }));
            UnityWebRequest www = UnityWebRequest.Get(text);
            www.downloadHandler = new DownloadHandlerBuffer();
            if (headers != null)
            {
                foreach (var kv in headers)
                {
                    www.SetRequestHeader(kv.Key, kv.Value);
                }
            }
            var observable = Observable.FromCoroutine<string>((observer, cancelToken) => Fetch(www, observer, progress, cancelToken
                , (handler) =>
                {
                    var hhandler = handler as DownloadHandlerBuffer;
                    if (CacheAsFile)
                    {
                        checkSaveAsFile(path, hhandler.data);
                    }
                    return hhandler.text;
                }));
            dispose = observable.Subscribe(onNext, onError);
#else
            dispose = GetBytes(relativeUri, (bytes) =>
            {
                if (bytes == null)
                {
                    return;
                }
                checkSaveAsFile(path, bytes);
                onNext(GeneralUtils.Bytes2Utf8String(bytes));
            }
            , onError, headers, progress
            );
#endif
            return dispose;
        }

        static IEnumerator loadAb(string path, IObserver<AssetBundle> observer, IProgress<float> process)
        {
            var bundleLoadRequest = AssetBundle.LoadFromFileAsync(path);
            yield return bundleLoadRequest;

            var myLoadedAssetBundle = bundleLoadRequest.assetBundle;
            if(process != null)
            {
                process.Report(1);
            }
            if (myLoadedAssetBundle == null)
            {
                observer.OnError(new MyUnityWebException("Failed to load Assetbundle : " + path));
                yield break;
            }
            observer.OnNext(myLoadedAssetBundle);
        }
        public static IObservable<AssetBundle> GetAssetBundle(string relativeUri, Hash headers = null, IProgress<float> progress = null)
        {
            string path = Path.Combine(FileUtils.PersistentDataPath, relativeUri);
            if (File.Exists(path))
            {
                var localObv = Observable.FromCoroutine<AssetBundle>((observer) => { return loadAb(path, observer, progress); });
                return localObv;
            }
#if UNITY_5_4_OR_NEWER
            string text = (string.Join("/", new string[] { UrlRoot, relativeUri }));
            UnityWebRequest www = UnityWebRequest.GetAssetBundle(text);
            var observable = Observable.FromCoroutine<AssetBundle>((observer, cancelToken) => Fetch(www, observer, progress, cancelToken
                , (handler) =>
                {
                    var hhandler = handler as DownloadHandlerAssetBundle;
                    if (CacheAsFile)
                    {
                        checkSaveAsFile(path, hhandler.data);
                    }
                    return hhandler.assetBundle;
                }));
            return observable;
#else
            return Observable.Create<AssetBundle>((observer) =>
            {
                dispose = GetBytes(relativeUri, (bytes) =>
                {
                    if (bytes == null)
                    {
                        return;
                    }
                    checkSaveAsFile(path, bytes);
                    var ab = AssetBundle.LoadFromMemory(bytes);
                    observer.OnNext(ab);
                }
            , observer.OnError, headers, progress
            );
                return dispose;
            });
#endif
        }

        class MyUnityWebException : Exception
        {
            public MyUnityWebException(string msg)
                : base(msg)
            { }
        }

    }
}
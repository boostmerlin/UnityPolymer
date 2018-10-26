using System;
using UnityEngine;
using System.Threading;
using Ginkgo.UI;
using Ginkgo.IOC;
using System.Collections.Generic;
using System.Collections;

namespace Ginkgo
{
    public class MSystem : MonoBehaviour
    {
        public int MainThreadId { get; set; }

        public string RunningPlatName { get; private set; }

        static MSystem _ins;

        static List<IService> services = new List<IService>();

        public static void AddService(IService service)
        {
            if (services.Contains(service)) return;
            services.Add(service);
        }

        public static MSystem Selfie
        {
            get
            {
                Debug.Assert(_ins != null);
                return _ins;
            }
        }

        static IContainer container;
        public static IContainer Container
        {
            get
            {
                if (container == null)
                {
                    container = new Container();
                    container.RegisterInstance(container);
                }
                return container;
            }
        }

        static IEventAggregator eventAggregator;
        public static IEventAggregator EventAggregator
        {
            get { return eventAggregator ?? (eventAggregator = new EventAggregator()); }
            set { eventAggregator = value; }
        }

        #region LogDeviceInfo
        public static void LogDeviceInfo()
        {
            string text = "Ginkgo";
            string text2 = text;
            text = string.Concat(new object[]
            {
            text2,
            " ("
            });
            text += Application.platform.ToString();
            text2 = text;
            text = string.Concat(new object[]
            {
            text2,
            "deviceModel=",
            SystemInfo.deviceModel,
            ";",
            "deviceType=",
            SystemInfo.deviceType,
            ";",
            "deviceUniqueIdentifier=",
            SystemInfo.deviceUniqueIdentifier,
            ";",
            "graphicsDeviceID=",
            SystemInfo.graphicsDeviceID,
            ";",
            "graphicsDeviceName=",
            SystemInfo.graphicsDeviceName,
            ";",
            "graphicsDeviceVendor=",
            SystemInfo.graphicsDeviceVendor,
            ";",
            "graphicsDeviceVendorID=",
            SystemInfo.graphicsDeviceVendorID,
            ";",
            "graphicsDeviceVersion=",
            SystemInfo.graphicsDeviceVersion,
            ";",
            "graphicsMemorySize=",
            SystemInfo.graphicsMemorySize,
            ";",
            "graphicsShaderLevel=",
            SystemInfo.graphicsShaderLevel,
            ";",
            "npotSupport=",
            SystemInfo.npotSupport,
            ";",
            "operatingSystem=",
            SystemInfo.operatingSystem,
            ";",
            "processorCount=",
            SystemInfo.processorCount,
            ";",
            "processorType=",
            SystemInfo.processorType,
            ";",
            "supportedRenderTargetCount=",
            SystemInfo.supportedRenderTargetCount,
            ";",
            "supports3DTextures=",
            SystemInfo.supports3DTextures,
            ";",
            "supportsAccelerometer=",
            SystemInfo.supportsAccelerometer,
            ";",
            "supportsComputeShaders=",
            SystemInfo.supportsComputeShaders,
            ";",
            "supportsGyroscope=",
            SystemInfo.supportsGyroscope,
            ";",
            "supportsImageEffects=",
            SystemInfo.supportsImageEffects,
            ";",
            "supportsInstancing=",
            SystemInfo.supportsInstancing,
            ";",
            "supportsLocationService=",
            SystemInfo.supportsLocationService,
            ";",
            "supportsRenderToCubemap=",
            SystemInfo.supportsRenderToCubemap,
            ";",
            "supportsShadows=",
            SystemInfo.supportsShadows,
            ";",
            "supportsSparseTextures=",
            SystemInfo.supportsSparseTextures,
            ";",
            "supportsVibration=",
            SystemInfo.supportsVibration,
            ";",
            "systemMemorySize=",
            SystemInfo.systemMemorySize,
            ";",
            "SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf)=",
            SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf),
            ";",
            "SupportsRenderTextureFormat(RenderTextureFormat.ARGB4444)=",
            SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB4444),
            ";",
            "SupportsRenderTextureFormat(RenderTextureFormat.Depth)=",
            SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth),
            ";",
            "graphicsDeviceVersion.StartsWith(\"Metal\")=",
            SystemInfo.graphicsDeviceVersion.StartsWith("Metal"),
            ";",
            "currentResolution.width=",
            Screen.currentResolution.width,
            ";",
            "currentResolution.height=",
            Screen.currentResolution.height,
            ";",
            "screen.width=",
            Screen.width,
            ";",
            "screen.height=",
            Screen.height,
            ";",
            "dpi=",
            Screen.dpi,
            ";",
            "Application.persistentPath=",
            Application.persistentDataPath,
            ";",
            "temporaryCachePath=",
            Application.temporaryCachePath,
            ";",
            "streamingAssetsPath=",
            Application.streamingAssetsPath,
            ";",
            });

            text += "genuine? " + Application.genuine;
            Log.Common.Print(true, "userAgent = " + text.Substring(0, text.Length / 2));
            Log.Common.Print(true, "userAgent = " + text.Substring(text.Length / 2));
            Log.Common.Print(true, "Application.dataPath = " + Application.dataPath);
            Log.Common.Print(true, "Application.persistentDataPath = " + Application.persistentDataPath);
            Log.Common.Print(true, "Application.streamingAssetsPath = " + Application.streamingAssetsPath);
            Log.Common.Print(true, "Application.temporaryCachePath = " + Application.temporaryCachePath);
            Log.Common.Print(true, "Environment.CurrentDirectory = " + System.Environment.CurrentDirectory);
        }
        #endregion
        public T AddComponent<T>() where T : Component
        {
            T com = Selfie.gameObject.GetComponent<T>();
            if (!com)
            {
                com = Selfie.gameObject.AddComponent<T>();
            }
            return com;
        }

        void setPlatformName()
        {
            string[] names = { "android", "osx", "win", "webgl", "linux", "ios|iphone" };
            string lower = Application.platform.ToString().ToLower();
            bool set = false;
            for (int i = 0; i < names.Length; i++)
            {
                var text = names[i];
                if (text.Contains("|"))
                {
                    string[] array = text.Split('|');
                    Array.ForEach(array, (s) =>
                    {
                        if (lower.Contains(s))
                        {
                            RunningPlatName = array[0];
                            set = true;
                        }
                    });
                }
                else if (lower.Contains(text))
                {
                    RunningPlatName = text;
                    set = true;
                }
                if (set)
                {
                    break;
                }
            }
        }

        IEnumerator Start()
        {
            foreach(var service in services)
            {
                yield return StartCoroutine(service.SetupAsync());
                service.Setup();
                service.Loaded();
            }
        }

        void Awake()
        {
            _ins = this;
            MainThreadId = Thread.CurrentThread.ManagedThreadId;
            Log.Common.Print("MLIntialize on Main Thread: {0}", MainThreadId);
            WWWLoader.UrlRoot = GinkgoConfig.Selfie.ServerRootPath;
#if USE_FGUI
            Container.Register<IUIManagerService, FGUIService>();
#else

#endif
            bool ok = GUIManager.Selfie.Check();
            if (!ok)
            {
                Log.Common.PrintWarning("GUIManager Not ready.");
            }
#if DEBUG_ML
            ReflectionUtil.LogAssemblyInfo();
            LogDeviceInfo();
#endif
            setPlatformName();
            setUp();
        }

        void setUp()
        {
            AddService(new NetworkService());
        }
    }
}

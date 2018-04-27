using System;
using UnityEngine;
using System.Threading;
using ML.UI;
using ML.IOC;
using System.Collections.Generic;
using System.Collections;

namespace ML
{
    public class MSystem : MonoBehaviour
    {
        public int MainThreadId { get; set; }

        public string RunningPlatName { get; private set; }

        static MSystem _ins;

        static List<IService> services = new List<IService>();

        public static void addService(IService service)
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
            string text = "Moonlight";
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
            Log.ML.Print(true, "userAgent = " + text.Substring(0, text.Length / 2));
            Log.ML.Print(true, "userAgent = " + text.Substring(text.Length / 2));
            Log.ML.Print(true, "Application.dataPath = " + Application.dataPath);
            Log.ML.Print(true, "Application.persistentDataPath = " + Application.persistentDataPath);
            Log.ML.Print(true, "Application.streamingAssetsPath = " + Application.streamingAssetsPath);
            Log.ML.Print(true, "Application.temporaryCachePath = " + Application.temporaryCachePath);
            Log.ML.Print(true, "Environment.CurrentDirectory = " + System.Environment.CurrentDirectory);
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
            Log.ML.Print("MLIntialize on Main Thread: {0}", MainThreadId);
            WWWLoader.UrlRoot = MelinConfig.Selfie.ServerRootPath;
#if USE_UGUI
#else
            Container.Register<IUIManagerService, FGUIService>();
            bool ok = GUIManager.Selfie.Check();
            if(!ok)
            {
                Log.ML.PrintWarning("GUIManager Not ready.");
            }
#endif
#if DEBUG_ML
            LogDeviceInfo();
#endif
            setPlatformName();
            setUp();
        }

        void setUp()
        {
            Entry.SetUp();
        }
    }
}

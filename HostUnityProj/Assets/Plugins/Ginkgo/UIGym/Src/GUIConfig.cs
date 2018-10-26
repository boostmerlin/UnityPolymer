using UnityEngine;
using ScreenMatchMode = FairyGUI.UIContentScaler.ScreenMatchMode;

namespace Ginkgo.UI
{
    public class GUIConfig : ScriptableObject
    {
        [Header("设计分辨率宽：")]
        public int designResoWidth = 1280;
        [Header("设计分辨率高：")]
        public int designResoHeight = 768;

        [Header("屏幕缩放模式：")]
        public ScreenMatchMode screenMatchMode = ScreenMatchMode.MatchWidthOrHeight;

        [Header("相对于Resources目录:")]
        public string LocalUIAssetsPath = "UI";

        [Header("相对于server root: ")]
        public string RemoteUIAssetsRootPath = "UI";

        [Header("自动查找本地UI资源")]
        public bool preloadAllLocalUI = true;

        [Header("预加载本地资源: ")]
        public string[] preloadAssets;

        [Header("启用动画: ")]
        public bool animationEnable = true;

        public bool checkRemoteAssetFirst = false;

        public static GUIConfig Selfie
        {
            get
            {
                var ins = Resources.Load<GUIConfig>("GUIConfig");
                return ins ?? new GUIConfig();
            }
        }

    }
}
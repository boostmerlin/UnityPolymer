namespace Ginkgo.UI
{
    public class UGUIService : IUIManagerService
    {
        public void InitLoadAssets(IUIAssetCtrl assetCtrl)
        {
            var config = GUIConfig.Selfie;
            var assets = config.preloadAssets;
            for (int i = 0; i < assets.Length; i++)
            {
                assetCtrl.LoadLocalPackage(assets[i]);
            }
        }

        public IUIAssetCtrl InitManager()
        {
            var config = GUIConfig.Selfie;

            var cs = URoot.Selfie.CanvasScaler;
            cs.screenMatchMode = config.screenMatchMode;
            var container = MSystem.Container;
            var assetCtrl = new UGUIAssetsCtrl();
            container.RegisterInstance<IUIAssetCtrl>(assetCtrl, false);
            container.Register<IViewCtrl, UGUIViewCtrlDefault>("default");
            return assetCtrl;
        }
    }
}
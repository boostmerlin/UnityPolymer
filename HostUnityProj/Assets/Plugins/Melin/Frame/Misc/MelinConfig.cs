using UnityEngine;

namespace ML
{
    public class MelinConfig : ScriptableObject
    {
        public bool DebugMode = true;

        public string ServerUrl = "http://127.0.0.1";
        public int ServerPort = 8000;
        public string ExternalAssets = "externals";
        public string bundleExt = ".ab";

        // public long MaxCacheSize = 300 * 1024 * 1024;

        public string ServerRootPath
        {
            get
            {
                return string.Format("{0}:{1}/{2}", ServerUrl, ServerPort, ExternalAssets);
            }
        }
        public static MelinConfig Instance
        {
            get
            {
                var ins = Resources.Load<MelinConfig>("MelinConfig");
                return ins ?? new MelinConfig();
            }
        }
    }
}
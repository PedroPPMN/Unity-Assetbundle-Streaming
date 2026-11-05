using UnityEngine;

namespace Portfolio.AssetBundles
{
    public sealed class AssetBundleDemo : MonoBehaviour
    {
        [TextArea]
        [SerializeField] private string sampleManifestJson = "{\n  \"bundles\": [\n    {\n      \"name\": \"props\",\n      \"url\": \"http://127.0.0.1:8000/props\",\n      \"version\": \"1\",\n      \"dependencies\": []\n    }\n  ]\n}";

        private async void Start()
        {
            var manifest = AssetBundleManifestUtility.Parse(sampleManifestJson);
            using var loader = new AssetBundleLoader();
            try
            {
                await loader.LoadWithDependenciesAsync(manifest, "props", new Progress<float>(p =>
                {
                    Debug.Log($"Bundle progress: {p:P0}");
                }));
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Demo expected to fail without a local server: {ex.Message}");
            }
        }
    }
}
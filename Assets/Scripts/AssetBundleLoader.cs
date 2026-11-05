using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Portfolio.AssetBundles
{
    public sealed class AssetBundleLoader : IDisposable
    {
        private readonly Dictionary<string, AssetBundle> _loaded = new();

        public async Task<AssetBundle> LoadFromUrlAsync(string name, string url, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            if (_loaded.TryGetValue(name, out var existing)) return existing;

            using var request = UnityWebRequestAssetBundle.GetAssetBundle(url);
            var op = request.SendWebRequest();
            while (!op.isDone)
            {
                progress?.Report(op.progress);
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new InvalidOperationException($"Failed to download bundle '{name}': {request.error}");
            }

            var bundle = DownloadHandlerAssetBundle.GetContent(request);
            _loaded[name] = bundle;
            progress?.Report(1f);
            return bundle;
        }

        public async Task LoadWithDependenciesAsync(BundleManifest manifest, string rootName, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            var order = new List<string>(AssetBundleManifestUtility.ResolveLoadOrder(manifest, rootName));
            for (var i = 0; i < order.Count; i++)
            {
                var name = order[i];
                BundleEntry entry = null;
                foreach (var b in manifest.bundles)
                {
                    if (b.name == name) { entry = b; break; }
                }

                if (entry == null) continue;
                var local = i;
                await LoadFromUrlAsync(entry.name, entry.url, new Progress<float>(p =>
                {
                    progress?.Report((local + p) / order.Count);
                }), cancellationToken);
            }
        }

        public void Unload(string name, bool unloadAllLoadedObjects = false)
        {
            if (!_loaded.TryGetValue(name, out var bundle)) return;
            bundle.Unload(unloadAllLoadedObjects);
            _loaded.Remove(name);
        }

        public void Dispose()
        {
            foreach (var pair in _loaded)
            {
                pair.Value.Unload(false);
            }
            _loaded.Clear();
        }
    }
}
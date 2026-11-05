using System;
using System.Collections.Generic;
using UnityEngine;

namespace Portfolio.AssetBundles
{
    [Serializable]
    public sealed class BundleManifest
    {
        public BundleEntry[] bundles;
    }

    [Serializable]
    public sealed class BundleEntry
    {
        public string name;
        public string url;
        public string version;
        public string[] dependencies;
    }

    public static class AssetBundleManifestUtility
    {
        public static BundleManifest Parse(string json) => JsonUtility.FromJson<BundleManifest>(json);

        public static IEnumerable<string> ResolveLoadOrder(BundleManifest manifest, string rootName)
        {
            var map = new Dictionary<string, BundleEntry>();
            foreach (var entry in manifest.bundles) map[entry.name] = entry;

            var visited = new HashSet<string>();
            var order = new List<string>();

            void Visit(string name)
            {
                if (!visited.Add(name)) return;
                if (!map.TryGetValue(name, out var entry)) return;
                if (entry.dependencies != null)
                {
                    foreach (var dep in entry.dependencies) Visit(dep);
                }
                order.Add(name);
            }

            Visit(rootName);
            return order;
        }
    }
}
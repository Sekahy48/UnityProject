using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Unity.Services
{
    public class TextureCache
    {
        private static TextureCache _instance;
        public static TextureCache Instance => _instance ??= new TextureCache();

        private readonly Dictionary<string, Texture2D> _cache = new Dictionary<string, Texture2D>();

        public Texture2D Get(string path)
        {
            if (_cache.TryGetValue(path, out Texture2D cached))
                return cached;

            string fullPath = Path.Combine(Application.streamingAssetsPath, path);
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"TextureCache: image not found at {fullPath}");
                return null;
            }

            byte[] bytes = File.ReadAllBytes(fullPath);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);
            _cache[path] = tex;
            return tex;
        }
    }
}

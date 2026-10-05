using System.Security.Cryptography;
using System.Text;

namespace Game;

// In-memory imported-scene/terrain/bounds cache identity; source/import bytes stay unchanged.
internal static class StaticRenderKey
{
    internal const string OptimizerVersion = "landscape-buffer-cache-v1";
    internal static string Create(string content, string importSettings, string engine, string backend, string version = OptimizerVersion)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('\n', content, importSettings, engine, backend, version))));
}

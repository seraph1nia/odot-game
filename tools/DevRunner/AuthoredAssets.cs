using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using Game;

namespace DevRunner;

internal sealed record AuthoredAssetFile(string Path, string SourcePath, string Sha256, long Bytes, int EmbeddedImages, int Materials, string[] AnimationNames);
internal sealed record AuthoredDerivedFile(string Path, string Parent, string ImageName, string Sha256, long Bytes);
internal sealed record AuthoredAssetManifest(int SchemaVersion, string Source, string Revision, string Permission, string PermissionScope, AuthoredAssetFile[] Files, AuthoredDerivedFile[] Derived);
internal sealed record GlbSummary(int Meshes, int Materials, int Images, string[] Clips, IReadOnlyDictionary<string, double> ClipLengths,
    IReadOnlyDictionary<string, double> ClipStarts, string[] Joints, IReadOnlyDictionary<string, string[]> Children);

// Actual distribution bytes and their declarative GLB contracts, not C# source
// text. Engine import/posed attachment checks remain the graphical consumer's job.
internal static class AuthoredAssets
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };
    internal static AuthoredAssetManifest ReadManifest(string directory) => JsonSerializer.Deserialize<AuthoredAssetManifest>(
        File.ReadAllText(System.IO.Path.Combine(directory, "manifest.json")), Options)
        ?? throw new InvalidDataException("Missing authored export manifest.");

    internal static IReadOnlyDictionary<string, GlbSummary> ValidateDistribution(string assetsDirectory)
    {
        string authored = System.IO.Path.Combine(assetsDirectory, "Authored");
        var models = Validate(authored);
        AssetCatalog.InstalledModels(Directory.EnumerateFiles(assetsDirectory, "*", SearchOption.AllDirectories)
            .Select(p => "res://Assets/" + System.IO.Path.GetRelativePath(assetsDirectory, p).Replace(System.IO.Path.DirectorySeparatorChar, '/')));
        return models;
    }
    internal static IReadOnlyDictionary<string, GlbSummary> Validate(string directory)
    {
        AuthoredAssetManifest manifest = ReadManifest(directory);
        if (manifest.SchemaVersion != 1 || manifest.Source != "https://github.com/seraph1nia/odot-game-assets.git"
            || manifest.Revision != AssetCatalog.Revision || manifest.Permission != AssetCatalog.Permission || string.IsNullOrWhiteSpace(manifest.PermissionScope))
            throw new InvalidDataException("Authored source revision or owner permission is not the declared contract.");
        string[] required = AssetCatalog.RequiredPaths.ToArray();
        if (manifest.Files.Length != required.Length || !manifest.Files.Select(f => f.Path).Order(StringComparer.Ordinal).SequenceEqual(required, StringComparer.Ordinal))
            throw new InvalidDataException("Authored manifest does not exactly cover the required presentation inventory.");
        string[] installed = Directory.EnumerateFiles(directory, "*.glb", SearchOption.AllDirectories)
            .Select(p => System.IO.Path.GetRelativePath(directory, p).Replace(System.IO.Path.DirectorySeparatorChar, '/')).Order(StringComparer.Ordinal).ToArray();
        if (!installed.SequenceEqual(required, StringComparer.Ordinal)) throw new InvalidDataException("Authored model distribution contains missing or unregistered exports.");
        var summaries = new Dictionary<string, GlbSummary>(StringComparer.Ordinal);
        var originals = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        foreach (AuthoredAssetFile file in manifest.Files)
        {
            if (System.IO.Path.IsPathRooted(file.Path) || file.Path.Contains('\\') || file.Path.Split('/').Any(s => s is "" or "." or "..")
                || file.SourcePath != "exports/" + file.Path || !file.Path.EndsWith(".glb", StringComparison.Ordinal))
                throw new InvalidDataException("Invalid authored export path.");
            byte[] bytes = File.ReadAllBytes(System.IO.Path.Combine(directory, file.Path));
            if (bytes.LongLength != file.Bytes || Convert.ToHexStringLower(SHA256.HashData(bytes)) != file.Sha256)
                throw new InvalidDataException("Authored export bytes differ from provenance: " + file.Path);
            originals.Add(file.Path, bytes);
            GlbSummary summary = Inspect(bytes);
            if (summary.Images != file.EmbeddedImages || summary.Materials != file.Materials
                || !summary.Clips.SequenceEqual(file.AnimationNames, StringComparer.Ordinal))
                throw new InvalidDataException("Authored export dependency/animation summary differs: " + file.Path);
            summaries.Add(file.Path, summary);
        }
        string[] images = Directory.EnumerateFiles(directory, "*.png", SearchOption.AllDirectories)
            .Select(p => System.IO.Path.GetRelativePath(directory, p).Replace(System.IO.Path.DirectorySeparatorChar, '/')).Order(StringComparer.Ordinal).ToArray();
        if (manifest.Derived is null || manifest.Derived.Length != images.Length
            || !manifest.Derived.Select(d => d.Path).Order(StringComparer.Ordinal).SequenceEqual(images, StringComparer.Ordinal))
            throw new InvalidDataException("Extracted authored map inventory differs from provenance.");
        foreach (AuthoredDerivedFile image in manifest.Derived)
        {
            if (!originals.TryGetValue(image.Parent, out byte[]? glb)
                || image.Path != image.Parent[..^4] + "_" + image.ImageName + ".png")
                throw new InvalidDataException("Invalid extracted authored map parent/path.");
            int jsonLength = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(glb.AsSpan(12)));
            using JsonDocument document = JsonDocument.Parse(glb.AsMemory(20, jsonLength));
            JsonElement root = document.RootElement;
            JsonElement embedded = root.GetProperty("images").EnumerateArray().Single(i => i.GetProperty("name").GetString() == image.ImageName);
            JsonElement view = root.GetProperty("bufferViews")[embedded.GetProperty("bufferView").GetInt32()];
            int offset = view.TryGetProperty("byteOffset", out JsonElement value) ? value.GetInt32() : 0;
            ReadOnlySpan<byte> original = glb.AsSpan(28 + jsonLength + offset, view.GetProperty("byteLength").GetInt32());
            byte[] derived = File.ReadAllBytes(System.IO.Path.Combine(directory, image.Path));
            if (derived.LongLength != image.Bytes || !original.SequenceEqual(derived)
                || Convert.ToHexStringLower(SHA256.HashData(derived)) != image.Sha256)
                throw new InvalidDataException("Extracted map differs from its immutable embedded source: " + image.Path);
        }
        return summaries;
    }

    internal static GlbSummary Inspect(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 28 || BinaryPrimitives.ReadUInt32LittleEndian(bytes) != 0x46546c67
            || BinaryPrimitives.ReadUInt32LittleEndian(bytes[4..]) != 2 || BinaryPrimitives.ReadUInt32LittleEndian(bytes[8..]) != bytes.Length
            || BinaryPrimitives.ReadUInt32LittleEndian(bytes[16..]) != 0x4e4f534a)
            throw new InvalidDataException("Not a complete version-two GLB export.");
        int jsonLength = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(bytes[12..]));
        if (jsonLength <= 0 || jsonLength > bytes.Length - 28) throw new InvalidDataException("Invalid GLB JSON chunk.");
        int binary = 20 + jsonLength;
        if (BinaryPrimitives.ReadUInt32LittleEndian(bytes[(binary + 4)..]) != 0x004e4942
            || BinaryPrimitives.ReadUInt32LittleEndian(bytes[binary..]) != bytes.Length - binary - 8)
            throw new InvalidDataException("Incomplete GLB binary dependency chunk.");
        using JsonDocument document = JsonDocument.Parse(bytes.Slice(20, jsonLength).ToArray());
        JsonElement root = document.RootElement;
        if (root.GetProperty("asset").GetProperty("version").GetString() != "2.0") throw new InvalidDataException("Invalid glTF version.");
        JsonElement[] buffers = Items(root, "buffers"), images = Items(root, "images"), materials = Items(root, "materials"), meshes = Items(root, "meshes"),
            nodes = Items(root, "nodes"), accessors = Items(root, "accessors"), animations = Items(root, "animations");
        if (buffers.Length != 1 || buffers.Any(b => b.TryGetProperty("uri", out _))
            || buffers[0].GetProperty("byteLength").GetInt32() > bytes.Length - binary - 8
            || images.Any(i => i.TryGetProperty("uri", out _) || !i.TryGetProperty("bufferView", out _)))
            throw new InvalidDataException("GLB has external or missing buffer/image dependencies.");
        if (meshes.Length == 0 || materials.Length == 0 || Items(root, "scenes").Length == 0
            || meshes.Any(m => Items(m, "primitives").Length == 0 || Items(m, "primitives").Any(p => !p.TryGetProperty("material", out _))))
            throw new InvalidDataException("GLB lacks scene geometry or authored materials.");
        var lengths = animations.ToDictionary(a => a.GetProperty("name").GetString()!, a => Items(a, "samplers")
            .Max(s => accessors[s.GetProperty("input").GetInt32()].GetProperty("max")[0].GetDouble()), StringComparer.Ordinal);
        var starts = animations.ToDictionary(a => a.GetProperty("name").GetString()!, a => Items(a, "samplers")
            .Min(s => accessors[s.GetProperty("input").GetInt32()].GetProperty("min")[0].GetDouble()), StringComparer.Ordinal);
        string[] joints = Items(root, "skins").SelectMany(s => s.GetProperty("joints").EnumerateArray())
            .Select(i => nodes[i.GetInt32()].GetProperty("name").GetString()!).Distinct(StringComparer.Ordinal).ToArray();
        var children = nodes.Where(n => n.TryGetProperty("name", out _)).ToDictionary(n => n.GetProperty("name").GetString()!,
            n => n.TryGetProperty("children", out JsonElement value) ? value.EnumerateArray().Select(i => nodes[i.GetInt32()].GetProperty("name").GetString()!).ToArray() : [], StringComparer.Ordinal);
        return new(meshes.Length, materials.Length, images.Length, animations.Select(a => a.GetProperty("name").GetString()!).ToArray(), lengths, starts, joints, children);
    }
    private static JsonElement[] Items(JsonElement element, string property) => element.TryGetProperty(property, out JsonElement value) ? value.EnumerateArray().ToArray() : [];
}

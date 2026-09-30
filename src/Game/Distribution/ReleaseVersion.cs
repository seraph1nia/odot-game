using System.Globalization;
using System.Text.RegularExpressions;

namespace Game.Distribution;

// Distribution precedence is independent of the gameplay wire protocol.
public sealed partial record ReleaseVersion
{
    private ReleaseVersion(string value, int major, int minor, int patch, string prerelease)
    {
        Value = value; Major = major; Minor = minor; Patch = patch; Prerelease = prerelease;
    }

    public string Value { get; }
    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }
    public string Prerelease { get; }
    public bool IsPreview => Prerelease.Length != 0;
    public string Channel => IsPreview ? "preview" : "stable";
    public string NumericVersion => string.Create(CultureInfo.InvariantCulture, $"{Major}.{Minor}.{Patch}.0");
    public string Tag => "v" + Value;

    // The assembly metadata limit is UInt16.MaxValue - 1. Limit text too so
    // hostile API tags cannot cause unbounded regex or prerelease processing.
    public static bool TryParse(string? value, out ReleaseVersion? version)
    {
        version = null;
        if (value is null || value.Length > 200) return false;
        Match match = Syntax().Match(value);
        if (!match.Success) return false;
        var numbers = new int[3];
        for (int i = 0; i < numbers.Length; i++)
            if (!int.TryParse(match.Groups[i + 1].Value, NumberStyles.None, CultureInfo.InvariantCulture, out numbers[i])
                || numbers[i] > 65534) return false;
        string prerelease = match.Groups[4].Value;
        foreach (string identifier in prerelease.Split('.'))
            if (IsNumeric(identifier) && identifier.Length > 1 && identifier[0] == '0') return false;
        version = new(value, numbers[0], numbers[1], numbers[2], prerelease);
        return true;
    }

    public static ReleaseVersion FromTag(string? tag)
    {
        if (tag is null || !tag.StartsWith('v') || !TryParse(tag[1..], out ReleaseVersion? version))
            throw new ArgumentException("Release tag must be vMAJOR.MINOR.PATCH[-prerelease][+metadata], with numeric components 0..65534 and at most 200 version characters.", nameof(tag));
        return version!;
    }

    public bool CanUpdateTo(ReleaseVersion candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        return (IsPreview || !candidate.IsPreview) && ComparePrecedence(candidate) < 0;
    }

    public int ComparePrecedence(ReleaseVersion other)
    {
        ArgumentNullException.ThrowIfNull(other);
        foreach ((int left, int right) in new[] { (Major, other.Major), (Minor, other.Minor), (Patch, other.Patch) })
            if (left != right) return left.CompareTo(right);
        if (!IsPreview || !other.IsPreview) return IsPreview.CompareTo(other.IsPreview) * -1;
        string[] a = Prerelease.Split('.'), b = other.Prerelease.Split('.');
        for (int i = 0; i < Math.Min(a.Length, b.Length); i++)
        {
            bool an = IsNumeric(a[i]), bn = IsNumeric(b[i]);
            int result = an != bn ? an ? -1 : 1
                : an && a[i].Length != b[i].Length ? a[i].Length.CompareTo(b[i].Length)
                : string.Compare(a[i], b[i], StringComparison.Ordinal);
            if (result != 0) return result;
        }
        return a.Length.CompareTo(b.Length);
    }

    private static bool IsNumeric(string value) => value.Length > 0 && value.All(char.IsAsciiDigit);

    [GeneratedRegex(@"\A(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)(?:-([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?(?:\+[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?\z", RegexOptions.CultureInvariant | RegexOptions.NonBacktracking)]
    private static partial Regex Syntax();
}

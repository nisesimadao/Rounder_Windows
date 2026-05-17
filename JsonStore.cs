using System.IO;
using System.Text.Json;

namespace Rounder.Windows;

public static class JsonStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public static string ConfigDirectory { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rounder");

    public static AppSettings LoadSettings()
    {
        Directory.CreateDirectory(ConfigDirectory);
        var path = Path.Combine(ConfigDirectory, "settings.json");
        if (!File.Exists(path))
        {
            return new AppSettings();
        }

        try
        {
            var settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path), Options) ?? new AppSettings();
            NormalizeSettings(settings);
            return settings;
        }
        catch
        {
            return new AppSettings();
        }
    }

    public static void SaveSettings(AppSettings settings)
    {
        Directory.CreateDirectory(ConfigDirectory);
        var path = Path.Combine(ConfigDirectory, "settings.json");
        File.WriteAllText(path, JsonSerializer.Serialize(settings, Options));
    }

    public static List<CornerPreset> LoadPresets()
    {
        Directory.CreateDirectory(ConfigDirectory);
        var path = Path.Combine(ConfigDirectory, "presets.json");
        if (!File.Exists(path))
        {
            var presets = CreateDefaultPresets();
            SavePresets(presets);
            return presets;
        }

        try
        {
            var presets = JsonSerializer.Deserialize<List<CornerPreset>>(File.ReadAllText(path), Options) ?? [];
            if (presets.Count == 0)
            {
                presets = CreateDefaultPresets();
                SavePresets(presets);
            }

            NormalizePresets(presets);
            return presets;
        }
        catch
        {
            return CreateDefaultPresets();
        }
    }

    public static void SavePresets(List<CornerPreset> presets)
    {
        Directory.CreateDirectory(ConfigDirectory);
        var path = Path.Combine(ConfigDirectory, "presets.json");
        File.WriteAllText(path, JsonSerializer.Serialize(presets, Options));
    }

    public static List<CornerPreset> ReadPresetFile(string path)
    {
        return JsonSerializer.Deserialize<List<CornerPreset>>(File.ReadAllText(path), Options) ?? [];
    }

    public static void WritePresetFile(string path, IEnumerable<CornerPreset> presets)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(presets, Options));
    }

    private static List<CornerPreset> CreateDefaultPresets()
    {
        return
        [
            new CornerPreset { Name = "All Corners" },
            new CornerPreset { Name = "Top Only", BottomLeftEnabled = false, BottomRightEnabled = false },
            new CornerPreset { Name = "Bottom Only", TopLeftEnabled = false, TopRightEnabled = false },
            new CornerPreset { Name = "Left Only", TopRightEnabled = false, BottomRightEnabled = false },
            new CornerPreset { Name = "Right Only", TopLeftEnabled = false, BottomLeftEnabled = false },
            new CornerPreset { Name = "None", TopLeftEnabled = false, TopRightEnabled = false, BottomLeftEnabled = false, BottomRightEnabled = false }
        ];
    }

    private static void NormalizeSettings(AppSettings settings)
    {
        if (settings.CornerColor.A == 0)
        {
            settings.CornerColor = System.Drawing.Color.Black;
        }
    }

    private static void NormalizePresets(IEnumerable<CornerPreset> presets)
    {
        foreach (var preset in presets)
        {
            if (preset.CornerColor.A == 0)
            {
                preset.CornerColor = System.Drawing.Color.Black;
            }
        }
    }
}

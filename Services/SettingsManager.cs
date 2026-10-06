using System.Globalization;
using System.Text.RegularExpressions;
using WukongBenchmarkRunner.Models;

namespace WukongBenchmarkRunner.Services;

public sealed class SettingsManager
{
    private readonly string _configPath;

    public SettingsManager(string configPath)
    {
        _configPath = configPath;
    }

    public void Apply(BenchmarkProfile profile)
    {
        if (!File.Exists(_configPath))
            throw new FileNotFoundException("GameUserSettings.ini не найден.", _configPath);

        var backupPath = _configPath + ".bak";
        if (!File.Exists(backupPath))
            File.Copy(_configPath, backupPath);

        var text = File.ReadAllText(_configPath);

        var desiredWidth = (int)(profile.ResolutionWidth * profile.ResolutionQuality / 100.0);
        var desiredHeight = (int)(profile.ResolutionHeight * profile.ResolutionQuality / 100.0);

        text = ReplaceLine(text, "ResolutionSizeX", profile.ResolutionWidth.ToString());
        text = ReplaceLine(text, "ResolutionSizeY", profile.ResolutionHeight.ToString());
        text = ReplaceLine(text, "LastUserConfirmedResolutionSizeX", profile.ResolutionWidth.ToString());
        text = ReplaceLine(text, "LastUserConfirmedResolutionSizeY", profile.ResolutionHeight.ToString());

        text = ReplaceLine(text, "DesiredScreenWidth", desiredWidth.ToString());
        text = ReplaceLine(text, "DesiredScreenHeight", desiredHeight.ToString());
        text = ReplaceLine(text, "LastUserConfirmedDesiredScreenWidth", desiredWidth.ToString());
        text = ReplaceLine(text, "LastUserConfirmedDesiredScreenHeight", desiredHeight.ToString());

        text = ReplaceLine(
            text,
            "sg.ResolutionQuality",
            profile.ResolutionQuality.ToString(CultureInfo.InvariantCulture));

        text = ReplaceLine(text, "bUseVSync", "False");
        text = ReplaceLine(text, "bUseDynamicResolution", "False");
        text = ReplaceLine(text, "FrameRateLimit", "0.000000");

        text = ReplaceUiValue(text, "QualityLevel", profile.QualityLevel.ToString());
        text = ReplaceUiValue(text, "ViewDistance", profile.ViewDistance.ToString());
        text = ReplaceUiValue(text, "AntiAliasing", profile.AntiAliasing.ToString());
        text = ReplaceUiValue(text, "PostProcessing", profile.PostProcessing.ToString());
        text = ReplaceUiValue(text, "ShadowQuality", profile.ShadowQuality.ToString());
        text = ReplaceUiValue(text, "TextureQuality", profile.TextureQuality.ToString());
        text = ReplaceUiValue(text, "FxQuality", profile.FxQuality.ToString());
        text = ReplaceUiValue(text, "MaterialQuality", profile.MaterialQuality.ToString());
        text = ReplaceUiValue(text, "VegetationQuality", profile.VegetationQuality.ToString());
        text = ReplaceUiValue(text, "GlobalIllumination", profile.GlobalIllumination.ToString());
        text = ReplaceUiValue(text, "ReflectionQuality", profile.ReflectionQuality.ToString());

        text = ReplaceUiValue(text, "InsertFrame", profile.FrameGeneration ? "1" : "0");
        text = ReplaceUiValue(text, "Rtx", profile.RayTracing ? "1" : "0");
        text = ReplaceUiValue(text, "RtxLevel", profile.RayTracingLevel.ToString());
        text = ReplaceUiValue(text, "Vsync", "0");
        text = ReplaceUiValue(text, "LockFrameRate", "0");
        text = ReplaceUiValue(text, "ImageQuality", desiredHeight.ToString());

        text = ReplaceLine(
            text,
            "r.RayTracing.EnableInGame",
            profile.RayTracing ? "True" : "False");

        text = ReplaceLine(text, "sg.ViewDistanceQuality", ToScalability(profile.ViewDistance).ToString());
        text = ReplaceLine(text, "sg.AntiAliasingQuality", ToScalability(profile.AntiAliasing).ToString());
        text = ReplaceLine(text, "sg.ShadowQuality", ToScalability(profile.ShadowQuality).ToString());
        text = ReplaceLine(text, "sg.GlobalIlluminationQuality", ToScalability(profile.GlobalIllumination).ToString());
        text = ReplaceLine(text, "sg.ReflectionQuality", ToScalability(profile.ReflectionQuality).ToString());
        text = ReplaceLine(text, "sg.PostProcessQuality", ToScalability(profile.PostProcessing).ToString());
        text = ReplaceLine(text, "sg.TextureQuality", ToScalability(profile.TextureQuality).ToString());
        text = ReplaceLine(text, "sg.EffectsQuality", ToScalability(profile.FxQuality).ToString());
        text = ReplaceLine(text, "sg.FoliageQuality", ToScalability(profile.VegetationQuality).ToString());
        text = ReplaceLine(text, "sg.ShadingQuality", ToScalability(profile.MaterialQuality).ToString());

        text = ReplaceLine(text, "GSStreamingPoolSize", profile.StreamingPoolSize.ToString());

        File.WriteAllText(_configPath, text);
    }

    private static int ToScalability(int uiValue) => uiValue - 1;

    private static string ReplaceLine(string input, string key, string value)
    {
        var pattern = $@"(?m)^{Regex.Escape(key)}=.*$";

        if (!Regex.IsMatch(input, pattern))
            throw new InvalidOperationException($"Параметр '{key}' не найден в конфиге.");

        return Regex.Replace(input, pattern, $"{key}={value}");
    }

    private static string ReplaceUiValue(string input, string key, string value)
    {
        var pattern = $@"\(""{Regex.Escape(key)}"",\s*""[^""]*""\)";

        if (!Regex.IsMatch(input, pattern))
            throw new InvalidOperationException($"UI-параметр '{key}' не найден в конфиге.");

        return Regex.Replace(input, pattern, $@"(""{key}"", ""{value}"")");
    }
}

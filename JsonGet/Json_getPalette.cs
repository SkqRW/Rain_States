using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace JsonGet;

public class PaletteInfo
{
    public List<int> palette { get; set; }
    public List<float> time { get; set; }


}

public static class PaletteManager
{
    public static Dictionary<string, PaletteInfo> Palettes { get; private set; }
    private static FileSystemWatcher _JSONwatcher;
    private static string _jsonPath;

    //TO DO: change the Pdebug for a BepInEx Logger 
    public static void LoadPalettes()
    {
        try
        {
            if (_jsonPath == null)
            {
                string dllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string modFolder = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(dllPath), ".."));
                _jsonPath = Path.Combine(modFolder, "palettes", "NightCycle.json");
                SetupWatcherJson();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[MyMod] Error al construir la ruta del archivo JSON: {ex.Message}");
            return;
        }

        if (!File.Exists(_jsonPath))
        {
            Debug.LogError($"[MyMod] No se encontró nightCycle.json en: {_jsonPath}");
            return;
        }

        try
        {
            string json = File.ReadAllText(_jsonPath);
            Palettes = JsonConvert.DeserializeObject<Dictionary<string, PaletteInfo>>(json);
            BepInEx.Logging.Logger.CreateLogSource("Palette").LogInfo($"Palette correctly load from {_jsonPath}");
            foreach (var kvp in Palettes)
            {
                string nombre = kvp.Key;
                var info = kvp.Value;
                string paleta = string.Join(", ", info.palette);
                string tiempos = string.Join(", ", info.time);
                BepInEx.Logging.Logger.CreateLogSource("Palette").LogInfo($"Region: {nombre} | Palette: [{paleta}] | Time: [{tiempos}]");
                PDEBUG.Log($"Region: {nombre} | Palette: [{paleta}] | Time: [{tiempos}]");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[MyMod] Error al leer nightCycle.json: {ex.Message}");
        }
    }

    private static void SetupWatcherJson()
    {
        if (_JSONwatcher != null)
            return;

        var dir = Path.GetDirectoryName(_jsonPath);
        var file = Path.GetFileName(_jsonPath);

        _JSONwatcher = new FileSystemWatcher(dir, file)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
        };
        _JSONwatcher.Changed += (s, e) =>
        {
            //Sleep savestly
            System.Threading.Thread.Sleep(100);
            LoadPalettes();
        };
        _JSONwatcher.EnableRaisingEvents = true;
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PDebug = Plugin.DevTools;

namespace Plugin;

public class PaletteInfo
{
    public List<int> palette { get; set; }
    public List<float> time { get; set; }
}

public static class PaletteManager
{
    public static Dictionary<string, PaletteInfo> Palettes { get; private set; }

    //TO DO: change the Pdebug for a BepInEx Logger 
    public static void LoadPalettes()
    {
        string jsonPath;
        try
        {
            string dllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string modFolder = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(dllPath), ".."));
            jsonPath = Path.Combine(modFolder, "palettes", "NightCycle.json");
        }catch (Exception ex)
        {
            Debug.LogError($"[MyMod] Error al construir la ruta del archivo JSON: {ex.Message}");
            return;
        }

        if (!File.Exists(jsonPath))
        {
            Debug.LogError($"[MyMod] No se encontró nightCycle.json en: {jsonPath}");
            return;
        }

        try
        {
            string json = File.ReadAllText(jsonPath);
            Palettes = JsonConvert.DeserializeObject<Dictionary<string, PaletteInfo>>(json);
            BepInEx.Logging.Logger.CreateLogSource("Palette").LogInfo($"Palette correctly load from {jsonPath}");
            foreach (var kvp in Palettes)
            {
                string nombre = kvp.Key;
                var info = kvp.Value;
                string paleta = string.Join(", ", info.palette);
                string tiempos = string.Join(", ", info.time);
                BepInEx.Logging.Logger.CreateLogSource("Palette").LogInfo($"Region: {nombre} | Palette: [{paleta}] | Time: [{tiempos}]");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[MyMod] Error al leer nightCycle.json: {ex.Message}");
        }
    }
}

﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Logging;
using Newtonsoft.Json;
using RoomChange;
using UnityEngine;

namespace JsonGet;


public static class PaletteManager
{
    private static readonly Dictionary<string, FileSystemWatcher> _watchers = new Dictionary<string, FileSystemWatcher>();
    private static readonly ConcurrentQueue<string> _reloadQueue = new ConcurrentQueue<string>();
    private static readonly Dictionary<string, DateTime> _lastLoadTimes = new Dictionary<string, DateTime>();

    private static ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("Rain States PaletteManager");
    
    private const string PALETTE_FOLDER = "palettes";
    private const string PALETTE_FILE_PATTERN = "RainStates*.json";

    /// <summary>
    /// Evento que se dispara cuando se recarga una paleta
    /// </summary>
    public static event EventHandler<PaletteReloadedEventArgs> PaletteReloaded;

    /// <summary>
    /// Evento que se dispara cuando falla la carga de una paleta
    /// </summary>
    public static event EventHandler<PaletteLoadErrorEventArgs> LoadFailed;

    /// <summary>
    /// Inicializa el sistema de paletas, escaneando todos los mods activos
    /// </summary>
    public static void LoadPalettes()
    {
        log.LogWarning("PaletteManager already initialized. Reloading...");
        PaletteInfo.Palettes.Clear();
        ScanAllActiveMods();

        log.LogInfo($"Loaded {PaletteInfo.Palettes.Count} total palette entries from all mods");
        
        // Log all loaded palettes for debugging
        log.LogDebug("Loaded Palettes:");
        foreach (var kvp in PaletteInfo.Palettes)
        {
            string regionName = kvp.Key;
            var info = kvp.Value;
            string paletteStr = (info.TerrainPalette != null ? string.Join(", ", info.BasePalette) : "N/A");
            string timeStr = (info.BaseTime != null ? string.Join(", ", info.BaseTime) : "N/A");
            string terrainStr = (info.TerrainPalette != null ? string.Join(", ", info.TerrainPalette) : "N/A");
            string terrainTimeStr = (info.TerrainTime != null ? string.Join(", ", info.TerrainTime) : "N/A");
            log.LogInfo($"Region: {regionName} | Palette: [{paletteStr}] | Time: [{timeStr}] | Terrain: [{terrainStr}] | TerrainTime: [{terrainTimeStr}]");
        }
    }

    /// <summary>
    /// Scan all active mods for palette files
    /// </summary>
    private static void ScanAllActiveMods()
    {
        if (ModManager.ActiveMods == null || ModManager.ActiveMods.Count == 0)
        {
            log.LogWarning("No active mods found");
            return;
        }

        foreach (var mod in ModManager.ActiveMods)
        {
            try
            {
                ScanModForPalettes(mod);
            }
            catch (Exception ex)
            {
                log.LogError($"Error scanning mod {mod.id}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Escanea un mod específico buscando archivos de paletas
    /// </summary>
    private static void ScanModForPalettes(ModManager.Mod mod)
    {
        // Verificar en las diferentes carpetas de versiones
        List<string> foldersToCheck = new List<string>();

        // Prioridad: TargetedPath > NewestPath > Path (default)
        if (mod.hasTargetedVersionFolder)
        {
            string targetedPalettePath = Path.Combine(mod.TargetedPath, PALETTE_FOLDER);
            if (Directory.Exists(targetedPalettePath))
            {
                foldersToCheck.Add(targetedPalettePath);
            }
        }
        
        if (mod.hasNewestFolder)
        {
            string newestPalettePath = Path.Combine(mod.NewestPath, PALETTE_FOLDER);
            if (Directory.Exists(newestPalettePath))
            {
                foldersToCheck.Add(newestPalettePath);
            }
        }

        string defaultPalettePath = Path.Combine(mod.path, PALETTE_FOLDER);
        if (Directory.Exists(defaultPalettePath))
        {
            foldersToCheck.Add(defaultPalettePath);
        }

        // Procesar cada carpeta encontrada
        foreach (var folder in foldersToCheck)
        {
            ScanDirectory(folder, mod);
        }
    }

    /// <summary>
    /// Escanea un directorio específico buscando archivos JSON de paletas
    /// </summary>
    private static void ScanDirectory(string directory, ModManager.Mod mod)
    {
        if (!Directory.Exists(directory))
            return;

        try
        {
            var jsonFiles = Directory.GetFiles(directory, PALETTE_FILE_PATTERN, SearchOption.TopDirectoryOnly);

            foreach (var file in jsonFiles)
            {
                if (IsMostRecent(file, mod))
                {
                    TryLoadPaletteFile(file, mod.id);
                }
            }

            // Configurar watcher para esta carpeta
            SetupWatcherForDirectory(directory);
        }
        catch (Exception ex)
        {
            log.LogError($"Error scanning directory {directory}: {ex.Message}");
        }
    }

    /// <summary>
    /// Intenta cargar un archivo de paletas y maneja errores
    /// </summary>
    private static void TryLoadPaletteFile(string filePath, string modId)
    {
        try
        {
            LoadPaletteFile(filePath, modId);
        }
        catch (Exception ex)
        {
            string errorMessage = $"Error loading palette file from {filePath}: {ex.Message}";
            log.LogError(errorMessage);
            LoadFailed?.Invoke(null, new PaletteLoadErrorEventArgs(errorMessage, ex, filePath));
        }
    }

    /// <summary>
    /// Carga un archivo de paletas y lo añade al diccionario global
    /// </summary>
    private static void LoadPaletteFile(string filePath, string modId)
    {
        if (!File.Exists(filePath))
            return;

        string json = File.ReadAllText(filePath);
        var palettes = JsonConvert.DeserializeObject<Dictionary<string, RoomChange.PaletteData>>(json);

        if (palettes == null)
        {
            throw new Exception("Failed to deserialize palette file");
        }

        // Añadir o actualizar paletas en el diccionario global
        foreach (var kvp in palettes)
        {
            string regionKey = kvp.Key;
            
            // Si ya existe, loguear que se está sobrescribiendo
            if (PaletteInfo.Palettes.ContainsKey(regionKey))
            {
                log.LogWarning(
                    $"Overwriting palette for region '{regionKey}' with version from mod '{modId}'"
                );
            }

            PaletteInfo.Palettes[regionKey] = kvp.Value;
        }

        // Actualizar tiempo de última carga
        _lastLoadTimes[filePath] = DateTime.Now;

        log.LogInfo(
            $"Loaded {palettes.Count} palette(s) from {Path.GetFileName(filePath)} (mod: {modId})"
        );
    }

    /// <summary>
    /// Configura un FileSystemWatcher para un directorio
    /// </summary>
    private static void SetupWatcherForDirectory(string directory)
    {
        if (_watchers.ContainsKey(directory))
            return;

        try
        {
            var watcher = new FileSystemWatcher(directory, "*.json")
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName
            };

            watcher.Changed += OnFileChanged;
            watcher.Created += OnFileChanged;
            watcher.Renamed += OnFileRenamed;
            watcher.EnableRaisingEvents = true;

            _watchers[directory] = watcher;

            log.LogDebug($"Watching directory: {directory}");
        }
        catch (Exception ex)
        {
            log.LogError($"Failed to setup watcher for {directory}: {ex.Message}");
        }
    }

    /// <summary>
    /// Maneja cambios en archivos
    /// </summary>
    private static void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (Path.GetFileName(e.FullPath).StartsWith("RainState") && e.FullPath.EndsWith(".json"))
        {
            // Verificar si es la versión más reciente
            if (IsMostRecentFile(e.FullPath))
            {
                _reloadQueue.Enqueue(e.FullPath);
            }
        }
    }

    /// <summary>
    /// Maneja renombrado de archivos
    /// </summary>
    private static void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        OnFileChanged(sender, e);
    }

    /// <summary>
    /// Recarga todos los archivos que han sido modificados
    /// </summary>
    public static void ReloadChangedFiles()
    {
        while (_reloadQueue.TryDequeue(out string filePath))
        {
            // Pequeña espera para asegurar que el archivo esté completamente escrito
            System.Threading.Thread.Sleep(100);

            try
            {
                // Encontrar el mod al que pertenece este archivo
                string modId = GetModIdForFile(filePath);
                
                LoadPaletteFile(filePath, modId);
                
                // Disparar evento de recarga
                PaletteReloaded?.Invoke(null, new PaletteReloadedEventArgs(filePath, modId));

                log.LogInfo($"Reloaded palette file: {Path.GetFileName(filePath)}");
            }
            catch (Exception ex)
            {
                string errorMessage = $"Failed to reload palette file {filePath}: {ex.Message}";
                log.LogError(errorMessage);
                LoadFailed?.Invoke(null, new PaletteLoadErrorEventArgs(errorMessage, ex, filePath));
            }
        }
    }

    /// <summary>
    /// Determina si un archivo es la versión más reciente en su mod
    /// </summary>
    private static bool IsMostRecent(string path, ModManager.Mod mod)
    {
        string fullPath = Path.GetFullPath(path);
        string targetedPath = mod.hasTargetedVersionFolder ? Path.GetFullPath(mod.TargetedPath) + Path.DirectorySeparatorChar : null;
        string newestPath = mod.hasNewestFolder ? Path.GetFullPath(mod.NewestPath) + Path.DirectorySeparatorChar : null;
        string defaultPath = Path.GetFullPath(mod.path) + Path.DirectorySeparatorChar;

        if (targetedPath != null && fullPath.StartsWith(targetedPath, StringComparison.InvariantCultureIgnoreCase))
        {
            // Archivo está en la carpeta targeted (mayor prioridad)
            return true;
        }
        else if (newestPath != null && fullPath.StartsWith(newestPath, StringComparison.InvariantCultureIgnoreCase))
        {
            // Archivo está en la carpeta newest
            string relativePath = fullPath.Substring(newestPath.Length);
            return targetedPath == null || !File.Exists(Path.Combine(mod.TargetedPath, relativePath));
        }
        else if (fullPath.StartsWith(defaultPath, StringComparison.InvariantCultureIgnoreCase))
        {
            // Archivo está en la carpeta raíz
            string relativePath = fullPath.Substring(defaultPath.Length);
            return (targetedPath == null || !File.Exists(Path.Combine(mod.TargetedPath, relativePath)))
                && (newestPath == null || !File.Exists(Path.Combine(mod.NewestPath, relativePath)));
        }

        return true;
    }

    /// <summary>
    /// Determina si un archivo es la versión más reciente entre todos los mods
    /// </summary>
    private static bool IsMostRecentFile(string path)
    {
        string fullPath = Path.GetFullPath(path);
        
        foreach (var mod in ModManager.ActiveMods)
        {
            string targetedPath = mod.hasTargetedVersionFolder ? Path.GetFullPath(mod.TargetedPath) + Path.DirectorySeparatorChar : null;
            string newestPath = mod.hasNewestFolder ? Path.GetFullPath(mod.NewestPath) + Path.DirectorySeparatorChar : null;
            string defaultPath = Path.GetFullPath(mod.path) + Path.DirectorySeparatorChar;

            if ((targetedPath != null && fullPath.StartsWith(targetedPath, StringComparison.InvariantCultureIgnoreCase)) ||
                (newestPath != null && fullPath.StartsWith(newestPath, StringComparison.InvariantCultureIgnoreCase)) ||
                fullPath.StartsWith(defaultPath, StringComparison.InvariantCultureIgnoreCase))
            {
                return IsMostRecent(path, mod);
            }
        }

        return true;
    }

    /// <summary>
    /// Obtiene el ID del mod al que pertenece un archivo
    /// </summary>
    private static string GetModIdForFile(string filePath)
    {
        string fullPath = Path.GetFullPath(filePath);

        foreach (var mod in ModManager.ActiveMods)
        {
            string modPath = Path.GetFullPath(mod.path) + Path.DirectorySeparatorChar;
            if (fullPath.StartsWith(modPath, StringComparison.InvariantCultureIgnoreCase))
            {
                return mod.id;
            }
        }

        return "Unknown";
    }

    /// <summary>
    /// Limpia todos los watchers y libera recursos
    /// </summary>
    public static void Cleanup()
    {
        foreach (var watcher in _watchers.Values)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }

        _watchers.Clear();
        while (_reloadQueue.TryDequeue(out _)) ;
        _lastLoadTimes.Clear();

        log.LogInfo("PaletteManager cleaned up");
    }

    /// <summary>
    /// Argumentos del evento de recarga de paleta
    /// </summary>
    public class PaletteReloadedEventArgs : EventArgs
    {
        public string FilePath { get; }
        public string ModId { get; }

        public PaletteReloadedEventArgs(string filePath, string modId)
        {
            FilePath = filePath;
            ModId = modId;
        }
    }

    /// <summary>
    /// Argumentos del evento de error de carga
    /// </summary>
    public class PaletteLoadErrorEventArgs : EventArgs
    {
        public string ErrorMessage { get; }
        public Exception Exception { get; }
        public string FilePath { get; }

        public PaletteLoadErrorEventArgs(string errorMessage, Exception exception, string filePath)
        {
            ErrorMessage = errorMessage;
            Exception = exception;
            FilePath = filePath;
        }
    }
}
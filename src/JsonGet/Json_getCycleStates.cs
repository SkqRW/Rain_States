using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Logging;
using Newtonsoft.Json;
using RoomChange;
using UnityEngine;

namespace JsonGet;

/// <summary>
/// Manages loading cycle configurations from the RainState folder
/// </summary>
public static class CycleStateManager
{
    private static ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("Rain States CycleManager");
    
    private const string CYCLE_FOLDER = "RainState";
    private const string JSON_EXTENSION = ".json";

    /// <summary>
    /// Initializes the cycle state system, scanning all active mods
    /// </summary>
    public static void LoadCycleStates()
    {
        log.LogInfo("Starting CycleStateManager initialization...");
        PaletteInfo.CycleConfigurations.Clear();
        
        // Temporary dictionary to store files before mapping indices
        var tempConfigurations = new Dictionary<string, System.Collections.Generic.SortedDictionary<int, RoomChange.PaletteData>>();
        
        ScanAllActiveMods(tempConfigurations);
        
        // Map indices to sequence [0, n-1]
        MapIndicesToSequential(tempConfigurations);

        log.LogInfo($"Loaded {PaletteInfo.CycleConfigurations.Count} regions/rooms with cycle configurations from all mods");
        
        // Log loaded configurations for debugging
        foreach (var kvp in PaletteInfo.CycleConfigurations)
        {
            string regionName = kvp.Key;
            var configs = kvp.Value;
            int cycleCount = configs.Count(c => c != null);
            log.LogInfo($"Region/Room: {regionName} | {cycleCount} cycle configuration(s) loaded");
            
            for (int i = 0; i < configs.Count; i++)
            {
                if (configs[i] != null)
                {
                    string paletteDetails = "[";
                    foreach (int c in configs[i].BasePalette){
                        paletteDetails += c + " ";
                    }
                    paletteDetails += "]";
                    log.LogDebug($"  - Cycle Index: {i:D2} | {paletteDetails}");
                }
            }
        }
    }

    /// <summary>
    /// Scans all active mods looking for cycle configurations
    /// </summary>
    private static void ScanAllActiveMods(Dictionary<string, System.Collections.Generic.SortedDictionary<int, RoomChange.PaletteData>> tempConfigurations)
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
                ScanModForCycleStates(mod, tempConfigurations);
            }
            catch (Exception ex)
            {
                log.LogError($"Error scanning mod {mod.id}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Scans a specific mod looking for cycle configurations
    /// </summary>
    private static void ScanModForCycleStates(ModManager.Mod mod, Dictionary<string, System.Collections.Generic.SortedDictionary<int, RoomChange.PaletteData>> tempConfigurations)
    {
        string cycleFolderPath = null;

        // Priority: TargetedPath > NewestPath > Path (default)
        // Only the first folder found is processed
        if (mod.hasTargetedVersionFolder)
        {
            string targetedCyclePath = Path.Combine(mod.TargetedPath, CYCLE_FOLDER);
            if (Directory.Exists(targetedCyclePath))
            {
                cycleFolderPath = targetedCyclePath;
            }
        }
        else if (mod.hasNewestFolder)
        {
            string newestCyclePath = Path.Combine(mod.NewestPath, CYCLE_FOLDER);
            if (Directory.Exists(newestCyclePath))
            {
                cycleFolderPath = newestCyclePath;
            }
        } 
        else
        {
            string defaultCyclePath = Path.Combine(mod.path, CYCLE_FOLDER);
            if (Directory.Exists(defaultCyclePath))
            {
                cycleFolderPath = defaultCyclePath;
            }
        }

        // Process only the highest priority folder
        if (cycleFolderPath != null)
        {
            ScanCycleDirectory(cycleFolderPath, mod, tempConfigurations);
        }
    }

    /// <summary>
    /// Scans a RainState directory looking for region folders
    /// </summary>
    private static void ScanCycleDirectory(string cycleDirectory, ModManager.Mod mod, Dictionary<string, System.Collections.Generic.SortedDictionary<int, RoomChange.PaletteData>> tempConfigurations)
    {
        if (!Directory.Exists(cycleDirectory))
            return;

        try
        {
            // Get all folders inside RainState (each one is a region)
            var regionFolders = Directory.GetDirectories(cycleDirectory);

            foreach (var regionFolder in regionFolders)
            {
                string regionName = Path.GetFileName(regionFolder);
                ProcessRegionFolder(regionFolder, regionName, mod, tempConfigurations);
            }
        }
        catch (Exception ex)
        {
            log.LogError($"Error scanning cycle directory {cycleDirectory}: {ex.Message}");
        }
    }

    /// <summary>
    /// Processes a region folder looking for cycle configuration files
    /// </summary>
    private static void ProcessRegionFolder(string regionFolder, string regionName, ModManager.Mod mod, Dictionary<string, System.Collections.Generic.SortedDictionary<int, RoomChange.PaletteData>> tempConfigurations)
    {
        try
        {
            // Get cycle configuration JSON files in the region's root folder
            var cycleFiles = Directory.GetFiles(regionFolder, "*" + JSON_EXTENSION, SearchOption.TopDirectoryOnly);
            
            foreach (var file in cycleFiles)
            {
                LoadCycleConfigFile(file, regionName, null, mod.id, tempConfigurations);
            }

            // Look for specific room folders
            var roomFolders = Directory.GetDirectories(regionFolder);
            
            foreach (var roomFolder in roomFolders)
            {
                string roomName = Path.GetFileName(roomFolder);
                
                // Get JSON files inside the room folder
                var roomCycleFiles = Directory.GetFiles(roomFolder, "*" + JSON_EXTENSION, SearchOption.TopDirectoryOnly);
                
                foreach (var file in roomCycleFiles)
                {
                    LoadCycleConfigFile(file, regionName, roomName, mod.id, tempConfigurations);
                }
            }
        }
        catch (Exception ex)
        {
            log.LogError($"Error processing region folder {regionFolder}: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads a cycle configuration file
    /// </summary>
    private static void LoadCycleConfigFile(string filePath, string regionName, string roomName, string modId, Dictionary<string, System.Collections.Generic.SortedDictionary<int, RoomChange.PaletteData>> tempConfigurations)
    {
        try
        {
            if (!File.Exists(filePath))
                return;

            string fileName = Path.GetFileNameWithoutExtension(filePath);
            
            // Extract the configuration ID from the filename (e.g., "01_day" -> 1)
            int configIndex = ExtractConfigIndex(fileName);
            
            if (configIndex < 0)
            {
                log.LogWarning($"File {fileName} does not have a valid numeric ID in the first 2 characters. Skipping...");
                return;
            }

            // Read the JSON file
            string json = File.ReadAllText(filePath);
            var paletteData = JsonConvert.DeserializeObject<RoomChange.PaletteData>(json);

            if (paletteData == null)
            {
                log.LogError($"Failed to deserialize cycle config file: {filePath}");
                return;
            }

            // Determine the dictionary key (specific room or region)
            string dictionaryKey = string.IsNullOrEmpty(roomName) ? regionName : roomName;

            // Initialize the sorted dictionary if it doesn't exist
            if (!tempConfigurations.ContainsKey(dictionaryKey))
            {
                tempConfigurations[dictionaryKey] = new System.Collections.Generic.SortedDictionary<int, RoomChange.PaletteData>();
            }

            // Check if a configuration already exists at that index
            if (tempConfigurations[dictionaryKey].ContainsKey(configIndex))
            {
                log.LogWarning(
                    $"Overwriting cycle config at file index {configIndex} for '{dictionaryKey}' with version from mod '{modId}'"
                );
            }

            // Save the palette with its original file index
            tempConfigurations[dictionaryKey][configIndex] = paletteData;

            string scope = string.IsNullOrEmpty(roomName) ? "region" : $"room ({roomName})";
            log.LogInfo(
                $"Loaded cycle config from file index {configIndex:D2} for {scope} '{regionName}' from mod '{modId}'"
            );
        }
        catch (Exception ex)
        {
            log.LogError($"Error loading cycle config file {filePath}: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// Extracts the configuration index from the first 2 characters of the filename
    /// </summary>
    private static int ExtractConfigIndex(string fileName)
    {
        if (string.IsNullOrEmpty(fileName) || fileName.Length < 2)
            return -1;

        // Extract the first 2 characters
        string indexPart = fileName.Substring(0, 2);

        // Verify it's numeric
        if (int.TryParse(indexPart, out int index))
            return index;

        return -1;
    }

    /// <summary>
    /// Maps file indices to a sequential [0, n-1] sequence
    /// </summary>
    private static void MapIndicesToSequential(Dictionary<string, System.Collections.Generic.SortedDictionary<int, RoomChange.PaletteData>> tempConfigurations)
    {
        foreach (var kvp in tempConfigurations)
        {
            string key = kvp.Key;
            var sortedConfigs = kvp.Value;

            // Create sequential list mapping the indices
            var sequentialList = new List<RoomChange.PaletteData>();
            var fileIndexToSequentialIndex = new Dictionary<int, int>();

            int sequentialIndex = 0;
            foreach (var config in sortedConfigs)
            {
                int fileIndex = config.Key;
                var paletteData = config.Value;

                sequentialList.Add(paletteData);
                fileIndexToSequentialIndex[fileIndex] = sequentialIndex;

                log.LogDebug($"Mapped '{key}' file index {fileIndex:D2} -> sequential index {sequentialIndex}");
                sequentialIndex++;
            }

            // Save the sequential list in the final dictionary
            PaletteInfo.CycleConfigurations[key] = sequentialList;
        }
    }
}

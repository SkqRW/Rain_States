using System;
using System.Linq;
using UnityEngine;


namespace RoomChange;

public partial class PaletteDrive
{
    private static RoomChange.PaletteData activeRegionPalette;
    private static float actualTime;
    private static bool DEBUG = true;

    public static void Terminate()
    {
        On.RoomCamera.UpdateDayNightPalette -= UpdateRainStatePaletteRoom;
    }
    
    public static void Init()
    {
        On.RoomCamera.UpdateDayNightPalette += UpdateRainStatePaletteRoom;
    }

    /// <summary>
    /// Method that updates the palette if found
    /// </summary>
    /// <param name="orig"></param>
    /// <param name="self"></param>
    private static void UpdateRainStatePaletteRoom(On.RoomCamera.orig_UpdateDayNightPalette orig, RoomCamera self)
    {
        if (self == null || self.room == null)
        {
            orig(self);
            return;
        }

        string roomKey = "";
        if (!PaletteInfo.IsRegionPaletteAvailable(self.room, ref roomKey))
        {
            if (DEBUG)
            {
                PDEBUG.Log("Palette Config not available, using original method.");
            }
            orig(self);
            return;
        }

        // Update rain cycle length (needed for time calculations)
        // Not sure if camera changes during gameplay (probably with dev tools though)
        // But I'm going to put this update here 
        PaletteInfo.SetRainCycleLength(self.room.world.rainCycle.cycleLength);

        activeRegionPalette = PaletteInfo.Palettes[roomKey];
        actualTime = self.room.world.rainCycle.timer;

        // Get the main camera (only apply to first player camera)
        // TODO: Will this be compatible with split screen? Handle later
        RoomCamera camera = self.room.game.cameras[0];

        // Apply base palette (required)
        ApplyBasePalette(camera, activeRegionPalette, actualTime);

        PDEBUG.Log("Applied Base Palette.");

        // Apply effect palettes if available
        if (activeRegionPalette.EffectALength > 0)
        {
            ApplyEffectAPalette(camera, activeRegionPalette, actualTime);
        }

        PDEBUG.Log("Applied Effect A Palette.");

        if (activeRegionPalette.EffectBLength > 0)
        {
            ApplyEffectBPalette(camera, activeRegionPalette, actualTime);
        }

        PDEBUG.Log("Applied Effect B Palette.");

        // TODO: Add terrain palette support
        // if (activeRegionPalette.TerrainLength > 0)
        // {
        //     ApplyTerrainPalette(camera, activeRegionPalette, actualTime);
        // }

        // Custom Debug
        if (self.room.game.devToolsActive && DEBUG)
        {
            // PDEBUG.Log($"Region: {self.room.world.region.name}, {self.room.abstractRoom.name} | Time: {actualTime}");
        }
    }

    /// <summary>
    /// Apply base palette with blending between two palette indices
    /// </summary>
    private static void ApplyBasePalette(RoomCamera camera, PaletteData data, float currentTime)
    {
        var sequence = PaletteInfo.GetBasePaletteSequence(data);
        var interval = PaletteInfo.CalculateIntervals(currentTime, sequence);

        if (interval.IsLastPalette)
        {
            if (DEBUG)
            {
                PDEBUG.Log($"No more palettes to apply. Using last palette: {sequence.Palettes[interval.CurrentIndex]}");
            }
            camera.ChangeMainPalette(sequence.Palettes[interval.CurrentIndex]);
            return;
        }

        PaintRoom.ChangeBothPalettes(
            camera, 
            sequence.Palettes[interval.PrevIndex], 
            sequence.Palettes[interval.NextIndex], 
            interval.BlendFactor
        );
    }

    /// <summary>
    /// Apply Effect A palette
    /// </summary>
    private static void ApplyEffectAPalette(RoomCamera camera, PaletteData data, float currentTime)
    {
        var sequence = PaletteInfo.GetEffectAPaletteSequence(data);
        if (!sequence.IsValid()) return;

        var interval = PaletteInfo.CalculateIntervals(currentTime, sequence);

        // For effect palettes, we typically apply the current palette directly
        // You can modify this to support blending if needed
        PaintRoom.ChangeEffectAPalette(camera, sequence.Palettes[interval.PrevIndex]);

        if (DEBUG)
        {
            PDEBUG.Log($"Applying Effect A Palette: index {interval.PrevIndex} (palette {sequence.Palettes[interval.PrevIndex]}) | blend: {interval.BlendFactor:F2}");
        }
    }

    /// <summary>
    /// Apply Effect B palette
    /// </summary>
    private static void ApplyEffectBPalette(RoomCamera camera, PaletteData data, float currentTime)
    {
        var sequence = PaletteInfo.GetEffectBPaletteSequence(data);
        if (!sequence.IsValid()) return;

        var interval = PaletteInfo.CalculateIntervals(currentTime, sequence);

        PaintRoom.ChangeEffectBPalette(camera, sequence.Palettes[interval.PrevIndex]);

        if (DEBUG)
        {
            PDEBUG.Log($"Applying Effect B Palette: index {interval.PrevIndex} (palette {sequence.Palettes[interval.PrevIndex]}) | blend: {interval.BlendFactor:F2}");
        }
    }

    /// <summary>
    /// Apply Terrain palette
    /// </summary>
    private static void ApplyTerrainPalette(RoomCamera camera, PaletteData data, float currentTime)
    {
        var sequence = PaletteInfo.GetTerrainPaletteSequence(data);
        if (!sequence.IsValid()) return;

        var interval = PaletteInfo.CalculateIntervals(currentTime, sequence);

        // TODO: Implement terrain palette application
        // This will require understanding how terrain palettes work in the game

        if (DEBUG)
        {
            PDEBUG.Log($"Applying Terrain Palette: {sequence.Palettes[interval.PrevIndex]} | blend: {interval.BlendFactor:F2}");
        }
    }
}

/// <summary>
/// Static class containing methods to apply palette changes to room cameras
/// </summary>
public static class PaintRoom
{
    private static bool DEBUG = true;

    /// <summary>
    /// Change both palettes with blending for smooth transitions
    /// </summary>
    public static void ChangeBothPalettes(RoomCamera camera, int prevPalette, int nextPalette, float blend)
    {
        camera.ChangeBothPalettes(prevPalette, nextPalette, blend);
    }

    /// <summary>
    /// Change Effect A palette by modifying the fade texture
    /// </summary>
    public static void ChangeEffectAPalette(RoomCamera camera, int paletteIndex)
    {
        Texture2D texture = camera.fadeTexA;
        
        // Get the effect colors from the all effects texture (2x2 pixel block)
        Color[] effectColors = RoomCamera.allEffectColorsTexture.GetPixels(paletteIndex * 2, 0, 2, 2, 0);
        
        if (DEBUG)
        {
            string colorDebug = "";
            foreach (Color c in effectColors)
            {
                colorDebug += $"R:{c.r * 255:F0}, G:{c.g * 255:F0}, B:{c.b * 255:F0} | ";
            }
            PDEBUG.Log($"Effect A Palette [{paletteIndex}]: {colorDebug}");
        }
        
        // Apply the effect colors to the fade texture (effect color is a 2x2 block)
        texture.SetPixels(30, 4, 2, 2, effectColors, 0);
        texture.SetPixels(30, 12, 2, 2, effectColors, 0);

        camera.ApplyFade();
    }

    /// <summary>
    /// Change Effect B palette (similar to Effect A)
    /// </summary>
    public static void ChangeEffectBPalette(RoomCamera camera, int paletteIndex)
    {
        Texture2D texture = camera.fadeTexB;
        
        // Get the effect colors from the all effects texture (2x2 pixel block)
        Color[] effectColors = RoomCamera.allEffectColorsTexture.GetPixels(paletteIndex * 2, 0, 2, 2, 0);
        
        if (DEBUG)
        {
            string colorDebug = "";
            foreach (Color c in effectColors)
            {
                colorDebug += $"R:{c.r * 255:F0}, G:{c.g * 255:F0}, B:{c.b * 255:F0} | ";
            }
            PDEBUG.Log($"Effect B Palette [{paletteIndex}]: {colorDebug}");
        }
        
        // Apply the effect colors to the fade texture
        texture.SetPixels(30, 2, 2, 2, effectColors, 0);
        texture.SetPixels(30, 10, 2, 2, effectColors, 0);

        camera.ApplyFade();
    }

    /// <summary>
    /// Change main palette without blending
    /// </summary>
    public static void ChangeMainPalette(RoomCamera camera, int paletteIndex)
    {
        camera.ChangeMainPalette(paletteIndex);
    }
}

using System;
using System.Linq;
using UnityEngine;


namespace RoomChange;

public partial class PaletteDrive
{
    private static RoomChange.PaletteData activeRegionPalette;
    private static float actualTime;
    private static bool DEBUG = true;

    public static bool activateEffectFade = false;

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
        activateEffectFade = false;

        // Get the main camera (only apply to first player camera)
        // TODO: Will this be compatible with split screen? Handle later
        RoomCamera camera = self.room.game.cameras[0];

        if (activeRegionPalette.BaseLength > 0)
        {
            ApplyBasePalette(camera, activeRegionPalette, actualTime);
            PDEBUG.Log("Applied Base Palette.");
        }

        // Apply effect palettes if available
        if (activeRegionPalette.EffectALength > 0)
        {
            ApplyEffectAPalette(camera, activeRegionPalette, actualTime);
            PDEBUG.Log("Applied Effect A Palette.");
        }


        if (activeRegionPalette.EffectBLength > 0)
        {
            ApplyEffectBPalette(camera, activeRegionPalette, actualTime);
            PDEBUG.Log("Applied Effect B Palette.");
        }

        // TODO: Add terrain palette support
        // if (activeRegionPalette.TerrainLength > 0)
        // {
        //     ApplyTerrainPalette(camera, activeRegionPalette, actualTime);
        // }


        if (activateEffectFade)
        {
            camera.ApplyFade();
        }

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
        PaintRoom.ChangeEffectAPalette(camera, sequence.Palettes[interval.PrevIndex], sequence.Palettes[interval.NextIndex], interval.BlendFactor);

        if (DEBUG)
        {
            PDEBUG.Log($"Applying Effect A Palette: index {interval.PrevIndex} (palette {sequence.Palettes[interval.PrevIndex]}) and palette {sequence.Palettes[interval.NextIndex]} | blend: {interval.BlendFactor:F2}");
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

        // Apply effect B with blending between two colors
        PaintRoom.ChangeEffectBPalette(
            camera, 
            sequence.Palettes[interval.PrevIndex], 
            sequence.Palettes[interval.NextIndex], 
            interval.BlendFactor
        );

        if (DEBUG)
        {
            PDEBUG.Log($"Applying Effect B Palette: index {interval.PrevIndex} (palette {sequence.Palettes[interval.PrevIndex]}) and palette {sequence.Palettes[interval.NextIndex]} | blend: {interval.BlendFactor:F2}");
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
    /// Convert hexadecimal color string to Unity Color
    /// Supports formats: "#RRGGBB", "RRGGBB", "#RGB", "RGB"
    /// </summary>
    public static Color HexToColor(string hex)
    {
        // Remove # if present
        if (hex.StartsWith("#"))
        {
            hex = hex.Substring(1);
        }

        // Expand short format (RGB -> RRGGBB)
        if (hex.Length == 3)
        {
            hex = string.Format("{0}{0}{1}{1}{2}{2}", hex[0], hex[1], hex[2]);
        }

        // Parse hex string
        if (hex.Length == 6)
        {
            try
            {
                int r = Convert.ToInt32(hex.Substring(0, 2), 16);
                int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                int b = Convert.ToInt32(hex.Substring(4, 2), 16);
                
                return new Color(r / 255f, g / 255f, b / 255f, 1f);
            }
            catch (Exception e)
            {
                PDEBUG.Log($"Failed to parse hex color '{hex}': {e.Message}. Using white as fallback.");
                return Color.white;
            }
        }

        PDEBUG.Log($"Invalid hex color format '{hex}'. Expected format: #RRGGBB or RRGGBB. Using white as fallback.");
        return Color.white;
    }

    /// <summary>
    /// Change both palettes with blending for smooth transitions
    /// </summary>
    public static void ChangeBothPalettes(RoomCamera camera, int prevPalette, int nextPalette, float blend)
    {
        camera.ChangeBothPalettes(prevPalette, nextPalette, blend);
    }

    /// <summary>
    /// Change Effect A palette with color blending between two custom colors
    /// </summary>
    public static void ChangeEffectAPalette(RoomCamera camera, string prevColorHex, string nextColorHex, float blendFactor)
    {
        Texture2D textureA = camera.fadeTexA;
        Texture2D textureB = camera.fadeTexB;

        // Parse hex colors
        Color prevColor = HexToColor(prevColorHex);
        Color nextColor = HexToColor(nextColorHex);

        // Blend between the two colors
        Color blendedColor = Color.Lerp(prevColor, nextColor, blendFactor);

        // Create 2x2 color array with the blended color
        Color[] effectColors = new Color[4]
        {
            blendedColor, blendedColor,
            blendedColor, blendedColor
        };

        if (DEBUG)
        {
            PDEBUG.Log($"Effect A: Blending [{prevColorHex}] -> [{nextColorHex}] at {blendFactor:F2} = RGB({blendedColor.r * 255:F0}, {blendedColor.g * 255:F0}, {blendedColor.b * 255:F0})");
        }

        // Apply the effect colors to the fade texture (effect color is a 2x2 block)
        textureA.SetPixels(30, 4, 2, 2, effectColors, 0);
        textureA.SetPixels(30, 12, 2, 2, effectColors, 0);
        textureB.SetPixels(30, 4, 2, 2, effectColors, 0);
        textureB.SetPixels(30, 12, 2, 2, effectColors, 0);
        PaletteDrive.activateEffectFade = true;
    }

    /// <summary>
    /// Change Effect B palette with color blending between two custom colors
    /// </summary>
    public static void ChangeEffectBPalette(RoomCamera camera, string prevColorHex, string nextColorHex, float blendFactor)
    {
        Texture2D textureA = camera.fadeTexA;
        Texture2D textureB = camera.fadeTexB;
        
        // Parse hex colors
        Color prevColor = HexToColor(prevColorHex);
        Color nextColor = HexToColor(nextColorHex);
        
        // Blend between the two colors
        Color blendedColor = Color.Lerp(prevColor, nextColor, blendFactor);

        // Create 2x2 color array with the blended color
        Color[] effectColors = new Color[4]
        {
            blendedColor, blendedColor,
            blendedColor, blendedColor
        };
        
        if (DEBUG)
        {
            PDEBUG.Log($"Effect B: Blending [{prevColorHex}] -> [{nextColorHex}] at {blendFactor:F2} = RGB({blendedColor.r * 255:F0}, {blendedColor.g * 255:F0}, {blendedColor.b * 255:F0})");
        }
        
        // Apply the effect colors to the fade texture
        textureA.SetPixels(30, 2, 2, 2, effectColors, 0);
        textureA.SetPixels(30, 10, 2, 2, effectColors, 0);
        textureB.SetPixels(30, 2, 2, 2, effectColors, 0);
        textureB.SetPixels(30, 10, 2, 2, effectColors, 0);

        PaletteDrive.activateEffectFade = true;
    }

    /// <summary>
    /// Change main palette without blending
    /// </summary>
    public static void ChangeMainPalette(RoomCamera camera, int paletteIndex)
    {
        camera.ChangeMainPalette(paletteIndex);
    }
}

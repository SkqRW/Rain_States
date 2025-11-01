using System;
using System.Linq;
using UnityEngine;


namespace RoomChange;

public partial class PaletteDrive
{
    private static int paletteIndex = 1;
    private static RoomChange.PaletteData activeRegionPalette;
    private static float nextPaletteTime;
    private static float lastPaletteTime;
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
    /// Method that update the palette if found
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

        string room = "";
        if (!PaletteInfo.IsRegionPaletteAvailable(self.room, ref room))
        {
            if (DEBUG)
            {
                PDEBUG.Log("Palette Config not available, using original method.");
            }
            orig(self);
            return;
        }

        // Don't sure if there any moment camera will change in game (probably with dev tools through)
        // But i'm gonna put that update here 
        PaletteInfo.SetRainCycleLength(self.room.world.rainCycle.cycleLength);


        activeRegionPalette = PaletteInfo.Palettes[room];
        PaletteInfo.CalculatePaletteIntervals(actualTime, activeRegionPalette, ref paletteIndex, ref lastPaletteTime, ref nextPaletteTime);



        if (paletteIndex >= activeRegionPalette.BaseLength)
        {
            if (DEBUG)
            {
                PDEBUG.Log("No more palettes to apply for region: " + self.room.world.region.name);
                self.room.game.cameras[0].ChangeMainPalette(activeRegionPalette.BasePalette[activeRegionPalette.BaseLength - 1]);
            }
            return;
        }


        //Only can take values from [0, 1]
        // If have more than 1, just change to the next palette
        actualTime = self.room.world.rainCycle.timer;
        float paletteBlend = Transitions.Linear.GetBlend(actualTime, lastPaletteTime, nextPaletteTime);

        int prevIndex = Math.Max(paletteIndex, 0);
        int nextIndex = Math.Min(paletteIndex + 1, activeRegionPalette.BasePalette.Count - 1);

        //Only apply to 1 player camera, will be compact with scroll screen?, later will care
        PaintRoom.ChangeBothPalettes(self.room.game.cameras[0], activeRegionPalette.BasePalette[prevIndex], activeRegionPalette.BasePalette[nextIndex], paletteBlend);



        //Custom Debug
        if (self.room.game.devToolsActive)
        {
            //PDEBUG.Log($"Region: {self.room.world.region.name}, {self.room.abstractRoom.name} | paletteIndex: [{prevIndex} - {nextIndex}] -> {paletteIndex} | The percent of blend is  %{paletteBlend * 100}:  ");
            //PDEBUG.Log($"Last Palette Time: {lastPaletteTime}, Next Palette Time: {nextPaletteTime}, Actual Time: {actualTime}");
        }

        if (activeRegionPalette.EffectALength > 0)
        {
            PaletteInfo.CalculatePaletteEffectIntervals(actualTime, activeRegionPalette, ref paletteIndex, ref lastPaletteTime, ref nextPaletteTime);
            int prevEffectIndex = Math.Max(paletteIndex, 0);
            int nextEffectIndex = Math.Min(paletteIndex + 1, activeRegionPalette.EffectAPalette.Count - 1);
            PaintRoom.ChangeEffectAPalette(self.room.game.cameras[0], activeRegionPalette.EffectAPalette[prevEffectIndex]);
            PDEBUG.Log($"Changing Effect A Palette from index {prevEffectIndex} to {nextEffectIndex} with blend %{paletteBlend * 100} | index {activeRegionPalette.EffectAPalette[prevEffectIndex]}");

        }


    }
}

public static class PaintRoom
{
    public static void ChangeBothPalettes(RoomCamera camera, int prevPalette, int nextPalette, float blend)
    {
        // Painful easy...
        camera.ChangeBothPalettes(prevPalette, nextPalette, blend);
    }

    public static void ChangeEffectAPalette(RoomCamera camera, int A)
    {
        Texture2D texture = camera.fadeTexA;
        Color[] AA = RoomCamera.allEffectColorsTexture.GetPixels(A * 2, 0, 2, 2, 0);
        string colors = "";
        foreach (Color c in AA)
        {
            colors += $"R:{c.r * 255}, G:{c.g * 255}, B:{c.b * 255} | ";
            //PDEBUG.Log($"Effect A Color: R:{c.r * 255}, G:{c.g * 255}, B:{c.b * 255}");
        }
        PDEBUG.Log($"Changing Effect A Palette to index: {A} and color {colors} ");
        // Seem the effect color is a 2x2 block
        texture.SetPixels(30, 4, 2, 2, AA, 0);
        texture.SetPixels(30, 12, 2, 2, AA, 0);

        camera.ApplyFade();
    }

}
using System.Linq;
using JsonGet;
using UnityEngine;

namespace RoomChange;

public partial class PaletteDrive
{
    private static int paletteIndex = 1;
    private static int totalPalettes;

    private static JsonGet.PaletteInfo activeRegionPalette;
    private static string currentRegionName;
    private static int rainCycleLength;
    private static float nextPaletteTime;
    private static float lastPaletteTime;
    private static float actualTime;
    private static bool devMode = true;

    public static void Terminate()
    {
        On.RoomCamera.UpdateDayNightPalette -= RoomCamera_UpdateDayNightPalette;
    }
    public static void Init()
    {
        On.RoomCamera.UpdateDayNightPalette += RoomCamera_UpdateDayNightPalette;
    }

    private static void RoomCamera_UpdateDayNightPalette(On.RoomCamera.orig_UpdateDayNightPalette orig, RoomCamera self)
    {
        //TO DO: move this later into a room ctor
        rainCycleLength = self.room.world.rainCycle.cycleLength;

        if (!IsRegionPaletteAvailable(self)) { 
            orig(self); 
            return; 
        }


        if (paletteIndex >= totalPalettes)
        {
            PDEBUG.Log("No more palettes to apply for region: " + self.room.world.region.name);
            self.room.game.cameras[0].ChangeMainPalette(activeRegionPalette.palette[totalPalettes-1]);
            return;
        }

        //Only can take values from [0, 1]
        actualTime = self.room.world.rainCycle.timer;
        float paletteBlend = RateChanges.Linear(actualTime, nextPaletteTime, lastPaletteTime);

        //Yeah, I know this is a bit hacky, but it works for now
        //Later see how to overrise the main palette and fade palette to the custom ones
        self.room.game.cameras[0].ChangeBothPalettes(activeRegionPalette.palette[paletteIndex - 1], activeRegionPalette.palette[paletteIndex], paletteBlend);

        //Custom Debug
        if(self.room.game.devToolsActive)
        {
            PDEBUG.Log($"Region: {self.room.world.region.name}, {self.room.abstractRoom.name} | paletteIndex: [{paletteIndex-1} - {paletteIndex}] | The percent of blend is  %{paletteBlend*100}: ");
        }

        if (paletteBlend > 1)
        {
            paletteIndex++;
            RefreshPaletteInterval(paletteIndex);
        }
    }

    private static bool IsRegionPaletteAvailable(RoomCamera self)
    {
        if (self == null || self.room == null) return false;
        Region region = self.room.world.region;
        bool IsspecificRoom = false;

        if (true)
        {
            // Especific room
            if (JsonGet.PaletteManager.Palettes.ContainsKey(self.room.abstractRoom.name))
            {
                IsspecificRoom = true;
            }
            // Region name
            if (!IsspecificRoom && !JsonGet.PaletteManager.Palettes.ContainsKey(region.name))
            {
                PDEBUG.Log($"NOT FOUND | No palettes found for region: {region.name}");
                return false;
            }

            string room = IsspecificRoom ? self.room.abstractRoom.name : region.name;
            currentRegionName = room;
            activeRegionPalette = JsonGet.PaletteManager.Palettes[room];
            totalPalettes = activeRegionPalette.palette.Count;

            if (totalPalettes == 0)
            {
                PDEBUG.Log($"Palette not found for {room}");
                return false;
            }

            if (totalPalettes == 1)
            {
                PDEBUG.Log($"ONLY ONE PALETTE | No need to cycle palettes for region: {room}");
                paletteIndex = totalPalettes; //Skip the update
                return false;
            }
            RefreshPaletteInterval(1);
            PDEBUG.Log($"[{IsspecificRoom}] Made a refresh in the region {currentRegionName}, now actual is {nextPaletteTime} and prev is {lastPaletteTime}");
            PDEBUG.Log($"The cycle time is {rainCycleLength} and actualTime are {actualTime}");
        }
        return true;
    }
}

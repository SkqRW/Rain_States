using UnityEngine;

namespace RoomChange;

public class PaletteDrive
{
    public static void Terminate()
    {
        On.RoomCamera.UpdateDayNightPalette -= RoomCamera_UpdateDayNightPalette;
    }
    public static void Init()
    {
        On.RoomCamera.UpdateDayNightPalette += RoomCamera_UpdateDayNightPalette;
    }
    private static int paletteIndex = 1;
    private static int totalPalettes = 0;

    private static JsonGet.PaletteInfo activeRegionPalette;
    private static string regionName;
    private static int rainCycleLength;


    private static void RoomCamera_UpdateDayNightPalette(On.RoomCamera.orig_UpdateDayNightPalette orig, RoomCamera self)
    {
        if (!IsRegionValid(self)) { 
            orig(self); 
            return; 
        }


        if (paletteIndex >= totalPalettes)
        {
            PDEBUG.Log("No more palettes to apply for region: " + self.room.world.region.name);
            self.room.game.cameras[0].ChangeMainPalette(activeRegionPalette.palette[paletteIndex - 1]);
            return;
        }

        //move this later into a room ctor
        rainCycleLength = self.room.world.rainCycle.cycleLength;
        

        float time = rainCycleLength * activeRegionPalette.time[paletteIndex];
        float pretime = rainCycleLength * activeRegionPalette.time[paletteIndex-1];
        float delta = RateChanges.Linear(self.room.world.rainCycle.timer, time, pretime);

        //Yeah, I know this is a bit hacky, but it works for now
        //Later see how to overrise the main palette and fade palette to the custom ones
        self.room.game.cameras[0].ChangeBothPalettes(activeRegionPalette.palette[paletteIndex - 1], activeRegionPalette.palette[paletteIndex], delta);

        //Custom Debug
        if(Input.GetKey(KeyCode.D))
        {
            PDEBUG.Log($"Region: {self.room.world.region.name} | paletteIndex: [{paletteIndex-1} - {paletteIndex}] | {delta}: ");
        }

        if (delta >= 1)
        {
            paletteIndex++;
        }
    }

    private static bool IsRegionValid(RoomCamera self)
    {
        Region region = self.room.world.region;
        if (region == null) return false;

        if (regionName == null || regionName != region.name)
        {
            if (!JsonGet.PaletteManager.Palettes.ContainsKey(region.name))
            {
                PDEBUG.Log("NOT FOUND | No palettes found for region: " + region.name);
                return false;
            }

            regionName = region.name;
            activeRegionPalette = JsonGet.PaletteManager.Palettes[region.name];
            totalPalettes = activeRegionPalette.palette.Count;
        }

        return true;
    }
}



public static class RateChanges
{
    //Relative path in A to B
    public static float Linear(float now, float time, float pretime)
    {
        float delta = (now - pretime) / (time - pretime);
        //PDEBUG.Log($"now: {now}, time: {time}, pretime: {pretime}, delta: {delta}");
        return delta;
    }
}


using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PDebug = Plugin.DevTools;

namespace Plugin;

public class PaletteDrive
{
    public static void Terminate()
    {
        On.RoomCamera.Update -= RoomCamera_Update;
    }
    public static void Init()
    {
        On.RoomCamera.Update += RoomCamera_Update;
    }
    private static int idx = 1;
    private static bool first = true;

    private static void RoomCamera_Update(On.RoomCamera.orig_Update orig, RoomCamera self)
    {
        orig(self);

        PDebug.Log("RoomCamera Update called in, rain cycle: + " + self.room.world.rainCycle.timer + " - " + self.room.world.rainCycle.TimeUntilRain + " : " + self.room.world.rainCycle.cycleLength);
        if (self.room.world.region == null)
        {
            return;
        }
        if (!PaletteManager.Palettes.ContainsKey(self.room.world.region.name))
        {
            PDebug.Log("No palettes found for region: " + self.room.world.region.name);
            return;
        }
        if(idx >= PaletteManager.Palettes[self.room.world.region.name].palette.Count)
        {
            PDebug.Log("No more palettes to apply for region: " + self.room.world.region.name);
            return;
        }

        


        int len = self.room.world.rainCycle.cycleLength;
        

        var paletteInfo = PaletteManager.Palettes[self.room.world.region.name];
        if (first)
        {
            self.room.game.cameras[0].ChangeMainPalette(paletteInfo.palette[idx-1]);
            first = false;
            return;
        }

        float time = len * paletteInfo.time[idx];
        float pretime = len * paletteInfo.time[idx-1];
        float delta = (self.room.world.rainCycle.timer-pretime) /(time-pretime);
        self.room.game.cameras[0].ChangeFadePalette(paletteInfo.palette[idx], delta);
        PDebug.Log("YESS :# Loading palettes for region: " + self.room.world.region.name + " | idx: " + idx);
        PDebug.Log($"now: {self.room.world.rainCycle.timer}, time: {time}, pretime: {pretime}, delta: {delta}");

        if (delta >= 1)
        {
            idx++;
            first = true;
        }


    }
}
using System;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using RWCustom;
using BepInEx;
using PDebug = UnityEngine.Debug;
using System.Drawing;
using System.Diagnostics;
using RKMFP = RegionKit.API.MoreFadePalettes;



namespace Plugin;

public partial class DevTools
{
    private void Terminate()
    {

    }
    public static void Init()
    {
        On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
        On.Player.Update += Player_Update;
    }
    public static bool notify = true;
    public static int devTimer = 0;
    public static int pal = 0;

    private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);
        if (notify)
        {
            PDebug.Log("Init dev tools palette");
            notify = false;
        }

        devTimer++;
        if (devTimer < 0)
        {
            return;
        }

        if (Input.GetKey(KeyCode.P))
        {
            PDebug.Log("Start testing dinamic palette");

            pal = ((pal + 1) % 3);
            RainCycle r = self.room.world.rainCycle;

            var screenCount = self.room.cameraPositions.Length;
            PDebug.Log($"change palette to {pal + 1000}");
            self.room.roomSettings.pal = pal + 1000;

            PDebug.Log($"see the changes :3");
            self.room.game.cameras[0].ChangeMainPalette(pal + 1000);
            //RKMFP.SetExtraFadePalette(self.room.roomSettings, 0, newFade);

            SetDevTimer(1);
        }
    }

    private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        sLeaser.sprites[2].color = UnityEngine.Color.red;
    }
    //Regresive count in seconds
    private static void SetDevTimer(int seconds)
    {
        devTimer = -seconds * 40;
    }



    public static void Log(string message)
    {
        UnityEngine.Debug.Log($"[Palette] {message}");
    }
}

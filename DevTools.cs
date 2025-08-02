using System;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using RWCustom;
using BepInEx;
using Debug = UnityEngine.Debug;
using System.Drawing;



namespace Plugin;

public partial class DevTools
{
    public static void Init()
    {
        On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
    }

    private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        sLeaser.sprites[2].color = UnityEngine.Color.red;
    }

    private void Terminate()
    {

    }

    public static void Log(string message)
    {
        UnityEngine.Debug.Log($"[Palette] {message}");
    }
}

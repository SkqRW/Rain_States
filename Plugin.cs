using System.Security;
using System.Security.Permissions;
using BepInEx;

#pragma warning disable CS0618

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace Plugin;

[BepInPlugin(ID, NAME, VER)]
public partial class Plugin : BaseUnityPlugin 
{
    public const string ID = "skeq.rainstates";
    public const string NAME = "Rain States";
    public const string VER = "0.0.2";
    private void OnEnable()
    {
        JsonGet.PaletteManager.LoadPalettes();
        RoomChange.PaletteDrive.Init();
        DevTools.Init();
        //On.RainWorld.OnModsInit += RainWorldOnOnModsInit;
    }

    private void OnDisable()
    {
    }

    /* Do this when the mod is finish?
    private bool IsInit;
    private void RainWorldOnOnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        if (IsInit) return;

        try
        {
            IsInit = true;

            //Your hooks go here
            
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
    */

}

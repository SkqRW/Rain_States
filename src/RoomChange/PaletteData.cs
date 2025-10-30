using System.Collections.Generic;

namespace RoomChange;

public class PaletteData
{
    public List<int> palette { get; set; }
    public List<float> time { get; set; }
}

public static class PaletteInfo
{
    public static Dictionary<string, RoomChange.PaletteData> Palettes = new Dictionary<string, RoomChange.PaletteData>();

    
    public static bool IsRegionPaletteAvailable(Room self)
    {
        if (PaletteInfo.Palettes == null)
        {
            PDEBUG.Log("Palettes no cargadas aún.");
            return false;
        }
        Region region = self.world.region;
        bool IsspecificRoom = false;


        // Especific room
        if (PaletteInfo.Palettes.ContainsKey(self.abstractRoom.name))
        {
            IsspecificRoom = true;
        }

        // Region name
        if (!IsspecificRoom && !PaletteInfo.Palettes.ContainsKey(region.name))
        {
            PDEBUG.Log($"NOT FOUND | No palettes found for region: {region.name}");
            return false;
        }

        string room = IsspecificRoom ? self.abstractRoom.name : region.name;
        PaletteDrive.currentRegionName = room;

        return PaletteDrive.applychange(room, IsspecificRoom);
    }

}

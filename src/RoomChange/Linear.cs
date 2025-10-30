using UnityEngine;

namespace RoomChange;

    /// <summary>
    /// Linear palette transition (default fallback).
    /// </summary>
    /// 


public static class RateChanges
{
    const float epsilon = 0.0001f;

    //Relative path in A to B
    public static float Linear(float now, float time, float pretime)
    {
        if (Mathf.Abs(time - pretime) < epsilon)
        {
            PDEBUG.Log("Division by zero in RateChanges.Linear");
            return 0f;
        }

        float delta = (now - pretime) / (time - pretime);
        PDEBUG.Log($"Actual Time: {now}, nextPaletteTime: {time}, prevPaletteTime: {pretime}, paletteBlend: %{delta * 100}");
        return delta;
    }
}

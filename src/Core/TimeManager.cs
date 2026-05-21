using Godot;
using System;
using System.Collections.Generic;

namespace Game.Core;

public partial class TimeManager : Node
{
    public static TimeManager Instance { get; private set; } = null!;
    
    // The raw, un-offset "Global Time" (0.0 to 1.0)
    public float GlobalTime { get; private set; }
    [Export] public float TimeScale { get; set; } = 1.0f; 

    private readonly Dictionary<string, float> _timeZoneOffsets = new()
    {
        { "GRAND_FORKS", -5.0f }, { "LONDON", 0.0f }, { "TOKYO", 9.0f }, { "NYC", -4.0f },
        { "PRIPYAT", 2.0f }, { "LA", -7.0f }, { "MEXICO_CITY", -6.0f }, { "PARIS", 1.0f },
        { "BERLIN", 1.0f }, { "MOSCOW", 3.0f }, { "BEIJING", 8.0f }, { "MUMBAI", 5.5f },
        { "SYDNEY", 11.0f }, { "RIO", -3.0f }, { "CAIRO", 2.0f }, { "CAPE_TOWN", 2.0f }
    };

    public override void _Ready() 
    {
        Instance = this;
        // Global time is just raw UTC
        DateTime now = DateTime.UtcNow;
        GlobalTime = (float)(now.TimeOfDay.TotalSeconds / 86400.0);
    }

    public void Tick(float delta)
    {
        // 86400 seconds in a day. Adjust by TimeScale.
        GlobalTime += (float)(delta * (TimeScale / 86400.0));
        if (GlobalTime > 1.0f) GlobalTime -= 1.0f;
    }

    public float GetLocalTime(string locationKey)
    {
        float offset = _timeZoneOffsets.GetValueOrDefault(locationKey, 0.0f);
        // Calculate the local time by adding the offset
        float localTime = GlobalTime + (offset / 24.0f);
        return (localTime % 1.0f + 1.0f) % 1.0f;
    }
    public override void _EnterTree() => Instance = this;
}
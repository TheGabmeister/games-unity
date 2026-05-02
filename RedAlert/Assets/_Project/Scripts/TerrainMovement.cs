public static class TerrainMovement
{
    // [TerrainType, LocomotionType] → speed multiplier (0 = impassable)
    private static readonly float[,] SpeedTable =
    {
        //           Foot  Tracked Wheeled Float
        /* Clear */ { 0.9f, 0.8f,  0.6f,   0f   },
        /* Road  */ { 1.0f, 1.0f,  1.0f,   0f   },
        /* Rough */ { 0.8f, 0.7f,  0.4f,   0f   },
        /* Sand  */ { 0.8f, 0.7f,  0.4f,   0f   },
        /* Water */ { 0f,   0f,    0f,     1.0f  },
        /* Ore   */ { 0.9f, 0.7f,  0.5f,   0f   },
        /* Gems  */ { 0.9f, 0.7f,  0.5f,   0f   },
    };

    public static float GetSpeedMultiplier(LocomotionType locomotion, TerrainType terrain)
    {
        return SpeedTable[(int)terrain, (int)locomotion];
    }

    public static bool IsPassable(LocomotionType locomotion, TerrainType terrain)
    {
        return SpeedTable[(int)terrain, (int)locomotion] > 0f;
    }
}

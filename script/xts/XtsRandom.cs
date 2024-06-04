using System;

public static partial class Xts
{
    static Random _rnd = new Random();

    public static float RndNextFloat() => (float)_rnd.NextDouble();

    public static bool RndNextBool() => _rnd.NextDouble() < 0.5;
}

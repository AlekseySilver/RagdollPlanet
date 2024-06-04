using Godot;
using System;

public struct AAnimKey: IComparable<AAnimKey>
{
    public enum EType
    {
        RELAX = 0, DIRECT, LIMIT, LIMB
    }

    public EType Type;
    public float Time;
    public Vector3 Value;
    public float Rate;


    public void json_append(System.Text.StringBuilder builder)
    {
        builder.Append("{\"e\":");
        builder.Append((int)Type);
        builder.Append(",\"t\":");
        builder.Append(Time.ToString3());
        builder.Append(",\"x\":");
        builder.Append(Value.x.ToString3());
        builder.Append(",\"y\":");
        builder.Append(Value.y.ToString3());
        builder.Append(",\"z\":");
        builder.Append(Value.z.ToString3());
        builder.Append(",\"r\":");
        builder.Append(Rate.ToString3());
        builder.Append("},");
    }

    public int CompareTo(object obj)
    {
        if (obj is AAnimKey key)
            return Time.CompareTo(key.Time);
        return 1;
    }

    public int CompareTo(AAnimKey other)
    {
        return Time.CompareTo(other.Time);
    }
} // AAnimKey

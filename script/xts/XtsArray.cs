using System.Collections.Generic;
using System.Linq;

public static partial class Xts
{
    public static string S(this Godot.Collections.Dictionary d, string key)
    {
        if (d.Contains(key))
            return d[key].ToString();
        return null;
    }
    public static int I(this Godot.Collections.Dictionary d, string key)
    {
        int.TryParse(d.S(key), out var i);
        return i;
    }
    public static float F(this Godot.Collections.Dictionary d, string key)
    {
        float.TryParse(d.S(key), out var i);
        return i;
    }
    public static Godot.Collections.Array Array(this Godot.Collections.Dictionary d, string key)
    {
        if (d.Contains(key))
            return d[key] as Godot.Collections.Array;
        return null;
    }

    public static void SetLength<T>(ref T[] array, int newLength) where T : new()
    {
        SetLength(ref array, newLength, new T());
    } // SetLength

    public static void SetLength<T>(ref T[] array, int newLength, T newValue)
    {
        if (newLength <= 0)
        {
            array = null;
            return;
        }
        int old = array != null ? array.Length : 0;
        if (old == newLength)
            return;

        var list = array != null ? array.ToList() : new List<T>();
        if (newLength > old)
        {
            for (int i = old; i < newLength; ++i)
                list.Add(newValue);
        }
        else
        {
            list.RemoveRange(newLength, list.Count - newLength);
        }

        array = list.ToArray();
    } // SetLength
}

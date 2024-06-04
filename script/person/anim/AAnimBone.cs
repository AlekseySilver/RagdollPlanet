using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public class AAnimBone
{
    public ABone Bone { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; private set; }

    public AAnimKey[] Keys { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; private set; }
    int _nextKey2PlayId = 0;

    /// <summary>
    /// Reset to anim Start
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset()
    {
        _nextKey2PlayId = 0;
    }

    /// <summary>
    /// advance and apply Keys
    /// </summary>
    public void UpdateChangeKey(float currentTime)
    {
        while (_nextKey2PlayId < Keys.Length)
        {
            var k = Keys[_nextKey2PlayId];
            if (k.Time > currentTime)
                break;
            ApplyKey(k);
            ++_nextKey2PlayId;
        }
    } // UpdateChangeKey

    /// <summary>
    /// go to Time
    /// </summary>
    public void GoToTime(float currentTime)
    {
        for (_nextKey2PlayId = 0; _nextKey2PlayId < Keys.Length; ++_nextKey2PlayId)
        {
            if (Keys[_nextKey2PlayId].Time > currentTime)
            {
                break;
            }
        }
        if (_nextKey2PlayId > 0)
            ApplyKey(Keys[_nextKey2PlayId - 1]); // play Prev key if have
    } // GotoTime

    public void ApplyKey(AAnimKey key)
    {
        if (Bone is ABone1 bone1)
        {
            switch (key.Type)
            {
                case AAnimKey.EType.DIRECT:
                    bone1.StartDirectByParentSpace(key.Value, key.Rate);
                    break;
                case AAnimKey.EType.LIMB:
                    var value = key.Value;
                    var rate = key.Rate;
                    if (value.x >= 0.0f)
                        bone1.StartLimit(value.x, rate);
                    if (value.y >= 0.0f)
                        bone1.Parent1.StartLimit(value.y, rate);
                    if (value.z >= 0.0f)
                        bone1.Parent1.Parent1.StartLimit(value.z, rate);
                    break;
                case AAnimKey.EType.LIMIT:
                    bone1.StartLimit(key.Value.x, key.Rate);
                    break;
                default:
                    bone1.Relax();
                    break;
            }
        }
        else
        {
            var b3 = Bone as ABone3;
            switch (key.Type)
            {
                case AAnimKey.EType.DIRECT:
                    b3.SetTargetOffset(new Basis(key.Value * Xts.deg2rad));
                    break;
                default:
                    b3.Relax();
                    break;
            }
        }
    } // ApplyKey

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ApplyFirstKey()
    {
        if (Keys.Length > 0)
            ApplyKey(Keys[0]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AAnimBone Create(ABone bone)
    {
        var s = new AAnimBone
        {
            Bone = bone,
            Keys = new AAnimKey[0]
        };
        return s;
    }

    public void AddKey(AAnimKey key)
    {
        RemoveKey(key.Time);

        var list = Keys.ToList();
        list.Add(key);
        Keys = list.ToArray();

        SortKeys();
    }

    public int GetKeyId(float time)
    {
        for (int i = 0; i < Keys.Length; ++i)
            if (Xts.IsBetween(Keys[i].Time, time - Xts.SMALL_FLOAT, time + Xts.SMALL_FLOAT))
                return i;
        return -1;
    }

    public bool GetKey(float time, out AAnimKey key)
    {
        int id = GetKeyId(time);
        if (id < 0)
        {
            key = new AAnimKey();
            return false;
        }
        key = Keys[id];
        return true;
    }

    public void RemoveKey(int i)
    {
        var list = Keys.ToList();
        list.RemoveAt(i);
        Keys = list.ToArray();
    }
    public void RemoveKey(float time)
    {
        var list = Keys.ToList();
        list.RemoveAll(k => Xts.IsBetween(k.Time, time - Xts.SMALL_FLOAT, time + Xts.SMALL_FLOAT));
        Keys = list.ToArray();
    }

    bool NeedSort()
    {
        float prev = -1f;
        for (int i = 0; i < Keys.Length; ++i)
        {
            float t = Keys[i].Time;
            if (t < prev)
                return true;
            prev = t;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool SortKeys()
    {
        if (NeedSort())
        {
            Array.Sort(Keys);
            return true;
        }
        return false;
    }

    public void JsonAppend(System.Text.StringBuilder builder)
    {
        if (Keys.Length == 0)
            return;
        builder.Append("{\"b\":\"");
        builder.Append(Bone.Name);
        builder.Append("\",\"a\":[");
        for (int i = 0; i < Keys.Length; ++i)
        {
            Keys[i].json_append(builder);
        }
        --builder.Length; // Remove last ,
        builder.Append("]},");
    }
}

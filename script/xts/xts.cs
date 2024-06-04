using Godot;
using System;
using System.Runtime.CompilerServices;

public static partial class Xts
{
    public const int GROUND_LAYER_ID = 0;   // 2^0 = 1 - groupnd from which can jump
    public const uint GROUND_LAYER_VALUE = 1u;

    public const int PERSON_LAYER_ID = 1;   // 2^1 = 2 - persons
    public const uint PERSON_LAYER_VALUE = 2u;


    public const int NPC_LAYER_ID = 2;    // 2^2 = 4 - NPC
    public const uint NPC_LAYER_VALUE = 4u;

    public const int INTERACTIVE_LAYER_ID = 3;    // 2^3 = 8 - interactive objects
    public const uint INTERACTIVE_LAYER_VALUE = 8u;

    public const int GRAB_LAYER_ID = 4;    // 2^4 = 16 - bars
    public const uint GRAB_LAYER_VALUE = 16u;

    // 2^5 = 32 - enemy 2

    public const int ENEMY_LAYER_ID = 6;    // 2^6 = 64 - enemy 1
    public const uint ENEMY_LAYER_VALUE = 64u;

    public const int PLAYER_LAYER_ID = 7;   // 2^7 = 128 - player
    public const uint PLAYER_LAYER_VALUE = 128u;

    public const float SMALL_FLOAT = .000001f;
    public const float SIN05 = 0.08715574274765f;
    public const float SIN10 = 0.17364817766693f;
    public const float SIN15 = 0.25881904510252f;
    public const float SIN20 = 0.34202014332567f;
    public const float SIN30 = 0.5f;
    /// <summary>
    /// sqrt(2) / 2
    /// </summary>
    public const float SIN45 = 0.707106781186548f;
    public const float SIN50 = 0.766044443118978f;
    public const float SIN60 = 0.866025403784439f;
    public const float SIN65 = 0.90630778703665f;
    public const float SIN75 = 0.965925826289f;
    public const float SIN80 = 0.984807753012208f;
    public const float SIN85 = 0.9961946980917455f;

    public const float deg2rad = 0.017453292519943f;
    public const float rad2deg = 57.2957795130823f;

    /// <summary>
    /// sqrt(2)
    /// </summary>
    public const float SQRT2 = 1.414213562373095f;
    public static bool IsBetween(int value, int min, int max) => value >= min && value <= max;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotBetween(float value, float min, float max) => value < min || value > max;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBetween(float value, float min, float max) => !IsNotBetween(value, min, max);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Clamp(float value, float min, float max) => value < min ? min : value > max ? max : value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Between(int value, int min, int max) => value < min ? min : value > max ? max : value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Between(float value, float min, float max) => value < min ? min : value > max ? max : value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Between(Vector2 value, float min, float max)
    {
        value.x = Between(value.x, min, max);
        value.y = Between(value.y, min, max);
        return value;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Between(Vector2 value, in Vector2 min, in Vector2 max)
    {
        value.x = Between(value.x, min.x, max.x);
        value.y = Between(value.y, min.y, max.y);
        return value;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Between(Vector3 value, float min, float max)
    {
        value.x = Between(value.x, min, max);
        value.y = Between(value.y, min, max);
        value.z = Between(value.z, min, max);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SmoothStep(float edge0, float edge1, float x)
    {
        float t = Between((x - edge0) / (edge1 - edge0), 0.0f, 1f);
        return t * t * (3f - 2f * t);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsIn(int value, int in1, int in2) => value == in1 || value == in2;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsIn(int value, params int[] ina)
    {
        foreach (var i in ina)
            if (value == i)
                return true;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsIn<T>(T value, T in1, T in2) => value.Equals(in1) || value.Equals(in2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsIn<T>(T value, T in1, T in2, T in3) => value.Equals(in1) || value.Equals(in2) || value.Equals(in3);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsIn<T>(T value, T in1, T in2, T in3, T in4) => IsIn(value, in1, in2) || IsIn(value, in3, in4);
    public static bool IsIn<T>(T value, params T[] ina)
    {
        foreach (var i in ina)
            if (value.Equals(i))
                return true;
        return false;
    }

    /// <summary>
    /// makes an angle between -180 and +180
    /// </summary>
    public static float ClearAngle(float angle)
    {
        if (IsNotBetween(angle, -180.0f, 180.0f))
        {
            angle %= 360.0f;
            if (IsNotBetween(angle, -180.0f, 180.0f))
                angle += angle < -180.0f ? 360.0f : -360.0f;
        }
        return angle;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Min(int a, int b) => a < b ? a : b;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Max(int a, int b) => a > b ? a : b;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Min(float a, float b) => a < b ? a : b;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Max(float a, float b) => a > b ? a : b;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Max(float a, float b, float c) => Max(Max(a, b), c);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Abs(int a) => a < 0 ? -a : a;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Abs(float a) => a < 0.0f ? -a : a;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    public static Vector3 Abs(Vector3 a) => new Vector3(Abs(a.x), Abs(a.y), Abs(a.z));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    public static void Abs(ref int a)
    {
        if (a < 0)
            a = -a;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Abs(ref float a)
    {
        if (a < 0.0f)
            a = -a;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Abs(ref Vector3 v)
    {
        Abs(ref v.x);
        Abs(ref v.y);
        Abs(ref v.z);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float AbsClamp(float v, float a, float b) => a < b ? Clamp(v, a, b) : Clamp(v, b, a);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Avg(float a, float b) => (a + b) * 0.5f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Avg(Vector3 a, Vector3 b) => new Vector3(Avg(a.x, b.x), Avg(a.y, b.y), Avg(a.z, b.z));

    /// <summary>
    /// Value = (Scale - ScaleMin) / (ScaleMax - ScaleMin) * (ValueMax - ValueMin) + ValueMin
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Lerp(float ValueMin, float ValueMax, float ScaleMin, float ScaleMax, float Scale)
        => Lerp(ValueMin, ValueMax, (Scale - ScaleMin) / (ScaleMax - ScaleMin));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Lerp(float a, float b, float t) => (b - a) * t + a;
    public static float LerpClamped(float a, float b, float t) => t <= 0.0f ? a : t >= 1f ? b : Lerp(a, b, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Lerp(in Vector2 a, in Vector2 b, float t) => (b - a) * t + a;
    public static Vector2 LerpClamped(in Vector2 a, in Vector2 b, float t) => t <= 0.0f ? a : t >= 1f ? b : Lerp(a, b, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Lerp(in Vector3 a, in Vector3 b, float t) => (b - a) * t + a;
    public static Vector3 LerpClamped(in Vector3 a, in Vector3 b, float t) => t <= 0.0f ? a : t >= 1f ? b : Lerp(a, b, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quat Lerp(in Quat a, in Quat b, float t) => (b - a) * t + a;
    public static Quat LerpClamped(in Quat a, in Quat b, float t) => t <= 0.0f ? a : t >= 1f ? b : Lerp(a, b, t);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color Lerp(in Color a, in Color b, float t) => (b - a) * t + a;

    // elementwise Lerp
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 LerpEl(in Vector3 a, in Vector3 b, in Vector3 t)
    {
        return new Vector3(Lerp(a.x, b.x, t.x), Lerp(a.y, b.y, t.y), Lerp(a.z, b.z, t.z));
    }

    public static Quat LerpEl(in Quat a, in Quat b, float t)
    {
        return new Quat(Lerp(a.x, b.x, t), Lerp(a.y, b.y, t), Lerp(a.z, b.z, t), Lerp(a.w, b.w, t));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Divide(this Vector3 a, in Vector3 b)
    {
        return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
    }

    public static Vector3 XY0(this Vector2 v) => new Vector3(v.x, v.y, 0.0f);
    public static Vector3 X0Y(this Vector2 v) => v.XAY(0.0f);
    public static Vector3 XAY(this Vector2 v, float A) => new Vector3(v.x, A, v.y);
    public static Vector3 XYA(this Vector2 v, float A) => new Vector3(v.x, v.y, A);
    public static Vector2 XZ(this Vector3 v) => new Vector2(v.x, v.z);
    public static Vector2 XY(this Vector3 v) => new Vector2(v.x, v.y);
    public static Vector2 ZZ(this Vector3 v) => new Vector2(v.z, v.z);
    public static Vector2 XX(this float v) => new Vector2(v, v);

    // the vector product of 2D vectors
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Cross2D(in Vector2 a, in Vector2 b) => a.x * b.y - a.y * b.x;

    public static Vector3 Truncate(Vector3 v, float maxLen)
    {
        Truncate(ref v, maxLen);
        return v;
    } // Truncate
    public static void Truncate(ref Vector3 v, float maxLen)
    {
        float len = v.LengthSquared();
        if (len > maxLen * maxLen)
        {
            len = Mathf.Sqrt(len);
            len = maxLen / len;
            v *= len;
        }
    }

    /// <summary>
    /// normalization of a vector with a slowing radius
    /// </summary>
    /// <param Name="v">vector</param>
    /// <param Name="slowingRadius">slowing radius</param>
    /// <param Name="maxMagnitude">maximum vector length</param>
    public static void SlowingNormalize(ref Vector3 v, float slowingRadius, float maxMagnitude)
    {
        float distance = v.Length();
        if (distance < slowingRadius) // Inside the slowing area
            v *= maxMagnitude / slowingRadius;
        else // Outside the slowing area.
            v *= maxMagnitude / distance;
    }

    /// <summary>
    /// the speed required to rotate the axis to the target direction
    /// </summary>
    /// <param Name="src">the axis in world coordinates</param>
    /// <param Name="tgt">the direction in world coordinates</param>
    /// <returns>angular velocity with a length from 0 to 1</returns>
    public static Vector3 AngularVelocity2Direction(in Vector3 src, in Vector3 tgt)
    {
        var torque = src.Cross(tgt);
        if (src.Dot(tgt) < 0.0f) // if src and tgt are directed in different directions
            torque = torque.Normalized();
        return torque;
    }

    // properties of quaternions
    // - - the inverse quaternion
    // q1 = q2 * q3   // * - consecutive turns
    // q2 = q1 * -q3 -- true
    // -q1 = -q3 * -q2 -- true
    // q3 = -(-q1 * q2) -- true

    // * - consecutive turns are turns in local coordinates,
    // this means that the object was first rotated to q2, then turned to q3 in local coordinates

    /// Sum = TermFirst * TermSecond
    /// Sum - the result of two consecutive turns
    /// TermFirst - 1st turn
    /// TermSecond - 2nd turn
    /// returns 1st turn
    public static Quat TermFirst(in Quat Sum, in Quat TermSecond)
    {
        return Multiply(Sum, TermSecond.Inverse());
    }

    /// Sum = TermFirst * TermSecond
    /// Sum - the result of two consecutive turns
    /// TermFirst - 1st turn
    /// TermSecond - 2nd turn
    /// returns 2nd turn
    public static Quat TermSecond(Quat Sum, in Quat TermFirst)
    {
        Sum = Sum.Inverse();
        Sum = Multiply(Sum, TermFirst);
        return Sum.Inverse();
    }

    public static Quat Multiply(in Quat a, in Quat b)
    {
        return new Quat(
            a.w * b.z + a.z * b.w + a.x * b.y - a.y * b.x,
            a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z,
            a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y,
            a.w * b.y + a.y * b.w + a.z * b.x - a.x * b.z

        );
    }


    /// Sum = TermFirst * TermSecond
    /// Sum - the result of two consecutive turns
    /// TermFirst - 1st turn
    /// TermSecond - 2nd turn
    /// returns 1st turn
    public static Basis TermFirst(in Basis Sum, in Basis TermSecond)
    {
        return Sum * TermSecond.Transposed();
    }

    /// Sum = TermFirst * TermSecond
    /// Sum - the result of two consecutive turns
    /// TermFirst - 1st turn
    /// TermSecond - 2nd turn
    /// returns 2nd turn
    public static Basis TermSecond(Basis Sum, in Basis TermFirst)
    {
        Sum = Sum.Transposed();
        Sum *= TermFirst;
        return Sum.Transposed();
    }

    // bitwise operations
    /// <summary>
    /// setting the bit to N value
    /// </summary>
    public static void SetBit(ref byte intValue, int bitId, bool bitValue)
    {
        int b = intValue;
        SetBit(ref b, bitId, bitValue);
        intValue = (byte)b;
    }

    /// <summary>
    /// setting the bit to N value
    /// </summary>
    public static void SetBit(ref int intValue, int bitId, bool bitValue)
    {
        int mask = 1 << bitId;
        if (bitValue)
            intValue |= mask;
        else
            intValue &= ~mask;
    }
    public static int SetBit(int intValue, int bitId, bool bitValue)
    {
        SetBit(ref intValue, bitId, bitValue);
        return intValue;
    }
    /// getting a bit in the N value
    public static bool GetBit(int intValue, int bitId)
    {
        int mask = 1 << bitId;
        mask &= intValue;
        return mask != 0;
    }

    /// number of set bits
    public static int BitCount(int intValue)
    {
        int count = 0;
        while (intValue != 0)
        {
            if ((intValue & 0x1) == 0x1)
                ++count;
            intValue >>= 1;
        }
        return count;
    }

    /// setting the bit to N value
    public static void SetBit(ref uint intValue, int bitId, bool bitValue)
    {
        uint mask = 1u << bitId;
        if (bitValue)
            intValue |= mask;
        else
            intValue &= ~mask;
    }
    public static uint SetBit(uint intValue, int bitId, bool bitValue)
    {
        SetBit(ref intValue, bitId, bitValue);
        return intValue;
    }
    /// getting a bit in the N value
    public static bool GetBit(uint intValue, int bitId)
    {
        uint mask = 1u << bitId;
        mask &= intValue;
        return mask != 0u;
    }

    /// кол-во установленных битов
    public static int BitCount(uint intValue)
    {
        int count = 0;
        while (intValue != 0)
        {
            if ((intValue & 0x1) == 0x1)
                ++count;
            intValue >>= 1;
        }
        return count;
    }

    public static bool AlwaysTrue() => true;
    public static bool AlwaysFalse() => false;

    /// <summary>
    /// the fraction of the speed to move by the delta value (soft approach along the cubic root)
    /// </summary>
    /// <param Name="delta">required offset</param>
    /// <param Name="max_inv_delta">the value is the inverse of the maximum displacement, above which the maximum speed operates</param>
    /// <returns>the value of the displacement velocity (from 0 (at delta == 0) to +/-1 (at |delta|>= max_delta))</returns>
    public static float SmoothOut(float delta, float maxDeltaInv)
    {
        float x = Between(delta * maxDeltaInv, -1.0f, 1.0f);
        if (x > 0.0f)
            return (float)Math.Pow(x, 1.0d / 3.0d);
        return -(float)Math.Pow(-x, 1.0d / 3.0d);
    }

    public static Vector3 SmoothLerp(Vector3 a, Vector3 b, float maxDeltaInv)
    {
        var d = b - a;
        if (d.x < 0.0f)
            d.x = -d.x;
        if (d.y < 0.0f)
            d.y = -d.y;
        if (d.z < 0.0f)
            d.z = -d.z;

        return LerpEl(a, b, new Vector3(SmoothOut(d.x, maxDeltaInv), SmoothOut(d.y, maxDeltaInv), SmoothOut(d.z, maxDeltaInv)));
    }
    public static Vector3 smooth_out(Vector3 delta, Vector3 max_delta_inv)
    {
        return new Vector3(SmoothOut(delta.x, max_delta_inv.x), SmoothOut(delta.y, max_delta_inv.y), SmoothOut(delta.z, max_delta_inv.z));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Reverse(ref Vector2 a)
    {
        a.x = -a.x;
        a.y = -a.y;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Reverse(ref Vector3 a)
    {
        a.x = -a.x;
        a.y = -a.y;
        a.z = -a.z;
    }

    /// <summary>
    /// new provisions towards the target
    /// </summary>
    /// <param Name="Source">starting position</param>
    /// <param Name="Target">target position</param>
    /// <param Name="shiftDist">the amount of movement along the path. it is obtained by multiplying the speed by the time</param>
    /// <returns>new starting position</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ApproachLinear(in Vector3 source, in Vector3 target, float shiftDist)
    {
        return ClampLengthFast(target - source, shiftDist) + source;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 ApproachLinear(in Vector2 source, in Vector2 target, float shiftDist)
    {
        return ClampLengthFast(target - source, shiftDist) + source;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ApproachLinear(float source, float target, float shiftDist) => Clamp(target - source, -shiftDist, shiftDist) + source;

    // 
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ClampLengthFast(Vector3 vec, float maxLength)
    {
        float len = vec.Length();
        if (len > maxLength)
            vec *= maxLength / len; // normalize & delta scale                
        return vec;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 ClampLengthFast(Vector2 vec, float maxLength)
    {
        float len = vec.Length();
        if (len > maxLength)
            vec *= maxLength / len; // normalize & delta scale
        return vec;
    }

    public static Vector3 GetVelocityAtPoint(this PhysicsDirectBodyState state, in Vector3 local_position)
    {
        var rel_pos = local_position - state.CenterOfMass;
        return state.LinearVelocity + state.AngularVelocity.Cross(rel_pos);
    }

    /// <summary>
    /// dir * dir.Dot(v);
    /// returns the value of the vector projected onto the axis (dir)
    /// projection of v onto a single direction vector
    /// the angle between the result and v will always be less than 90
    /// regardless of which way v is directed relative to dir
    /// the result is the same for dir = -dir
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ProjectionOn(this Vector3 v, in Vector3 dir)
    {
        return dir * dir.Dot(v);
    }

    /// <summary>
    /// a vector in the plane specified by the normal
    /// it is also a vector projected onto a plane
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 InPlane(this Vector3 v, in Vector3 norm)
    {
        return v - v.ProjectionOn(norm);
    }

    /// <summary>
    /// reflection of v from a plane with a dir normal
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Reflection(this Vector3 v, in Vector3 dir)
    {
        return v.ProjectionOn(dir) * -2f + v;
    }
}

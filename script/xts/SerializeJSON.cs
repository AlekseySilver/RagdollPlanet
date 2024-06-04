using Godot;
using Godot.Collections;
using System;
using System.Reflection;

public class SerializeJSON
{
    public static string ToJSON(object obj)
    {
        return JSON.Print(GetElement(obj));
    }

    static object GetElement(object obj)
    {
        if (obj == null)
            return null;
        var type = obj.GetType();
        if (type.IsArray)
        {
            var array = new Godot.Collections.Array();
            foreach (var a in obj as System.Array)
            {
                var el = GetElement(a);
                if (el != null)
                    array.Add(el);
            }
            return array;
        }
        else if (type == typeof(string) || type == typeof(int) || type == typeof(float) || type == typeof(bool) || type == typeof(long) || type == typeof(double))
        {
            return obj;
        }
        var dict = new Dictionary();
        var fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
        foreach (var field in fields)
        {
            dict.Add(field.Name, GetElement(field.GetValue(obj)));
        }
        return dict;
    } // GetElement

    public static T FromJSON<T>(string json)
    {
        var res = JSON.Parse(json)?.Result;
        if (res is Dictionary root)
            return (T)CreateFromDictionary(typeof(T), root);
        if (res is Godot.Collections.Array array)
            return (T)CreateFromArray(typeof(T).GetElementType(), array);
        return default(T);
    } // FromJSON

    static object CreateFromDictionary(Type type, Dictionary dict)
    {
        var obj = Activator.CreateInstance(type);
        SetFieldsFromDictionary(obj, dict);
        return obj;
    }
    static object CreateFromArray(Type type, Godot.Collections.Array array)
    {
        var obj = System.Array.CreateInstance(type, array.Count);
        for (int i = 0; i < array.Count; ++i)
        {
            if (type.IsArray)
            {
                if (array[i] is Godot.Collections.Array a)
                    obj.SetValue(CreateFromArray(type, a), i);
            }
            else
            {
                if (array[i] is Dictionary d)
                    obj.SetValue(CreateFromDictionary(type, d), i);
            }
        }
        return obj;
    }

    static void SetFieldsFromDictionary(object obj, Dictionary dict)
    {
        var fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
        foreach (var field in fields)
        {
            if (dict.Contains(field.Name))
            {
                var type = field.FieldType;
                var value = dict[field.Name];
                if (type.IsArray)
                {
                    if (value is Godot.Collections.Array arr)
                    {
                        type = type.GetElementType();
                        var gen = SetValueArray<string>(field, type, arr, obj)
                            || SetValueArray<int>(field, type, arr, obj)
                            || SetValueArray<float>(field, type, arr, obj)
                            || SetValueArray<bool>(field, type, arr, obj)
                            || SetValueArray<long>(field, type, arr, obj)
                            || SetValueArray<double>(field, type, arr, obj)
                            ;
                        if (gen == false)
                        {
                            field.SetValue(obj, CreateFromArray(type, arr));
                        }
                    }
                }
                else
                {
                    var gen = SetValue<string>(field, type, value, obj)
                            || SetValue<int>(field, type, value, obj)
                            || SetValue<float>(field, type, value, obj)
                            || SetValue<bool>(field, type, value, obj)
                            || SetValue<long>(field, type, value, obj)
                            || SetValue<double>(field, type, value, obj)
                            ;
                    if (gen == false && value is Dictionary d)
                    {
                        field.SetValue(obj, CreateFromDictionary(type, d));
                    }
                }
            }
        }
    } // SetFieldsFromDictionary

    static bool SetValue<T>(System.Reflection.FieldInfo field, Type type, object value, object obj)
    {
        if (type == typeof(T))
        {
            field.SetValue(obj, (T)Convert.ChangeType(value, type));
            return true;
        }
        return false;
    }

    static bool SetValueArray<T>(System.Reflection.FieldInfo field, Type type, Godot.Collections.Array array, object obj)
    {
        if (type == typeof(T))
        {
            var arr = new T[array.Count];
            for (int i = 0; i < arr.Length; ++i)
            {
                arr[i] = (T)Convert.ChangeType(array[i], type);
            }
            field.SetValue(obj, arr);
            return true;
        }
        return false;
    }
} // SerializeJSON

using Godot;
using System.Linq;
using System.Runtime.CompilerServices;

public class AAnimation {
    public string Name;
    public AAnimBone[] Bones { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; private set; }

    public float TotalTime = 0.0f;
    public float CurrentTime { get; private set; } = 0.0f;

    public float Overdrive = 1.0f;

    public bool IsLoop = false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset() {
        CurrentTime = 0.0f;
        ResetBones();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ResetBones() {
        for (int i = 0; i < Bones.Length; ++i)
            Bones[i].Reset();
    }

    public void Update(float timeStep) {
        if (CurrentTime >= TotalTime)
            return;

        CurrentTime += timeStep * Overdrive;
        for (int i = 0; i < Bones.Length; ++i)
            Bones[i].UpdateChangeKey(CurrentTime);

        if (CurrentTime >= TotalTime) {
            if (IsLoop) {
                ResetBones();
                CurrentTime -= TotalTime;
            }
            else {
                CurrentTime = TotalTime;
            }
        }
    } // UpdateOverride

    public void GotoTime(float time) {
        if (time != CurrentTime) {
            CurrentTime = time;
            for (int i = 0; i < Bones.Length; ++i)
                Bones[i].GoToTime(time);
        }
    } // GotoTime

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GotoTimePart(float part) {
        GotoTime(TotalTime > 0.0f ? part * TotalTime : 0.0f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SingleFrame() {
        for (int i = 0; i < Bones.Length; ++i)
            Bones[i].ApplyFirstKey();
    }

    public void JsonAppend(System.Text.StringBuilder builder) {
        if (Bones.Length == 0)
            return;
        builder.Append("{\"n\":\"");
        builder.Append(Name);
        builder.Append("\",\"t\":");
        builder.Append(TotalTime.ToString3());
        builder.Append(",\"o\":");
        builder.Append(Overdrive.ToString3());
        builder.Append(",\"l\":");
        builder.Append(IsLoop ? 1 : 0);
        builder.Append(",\"a\":[");
        for (int i = 0; i < Bones.Length; ++i) {
            Bones[i].JsonAppend(builder);
        }
        --builder.Length; // Remove last ,
        builder.Append("]}");
    }

    public void Save(string animFileName) {
        var file = new File();
        file.Open(animFileName, File.ModeFlags.Write);

        var sb = new System.Text.StringBuilder();
        JsonAppend(sb);
        file.StoreString(sb.ToString());
        file.Close();
    } // Save

    public void Load(APerson person, string animFileName) {
        using (var file = new File()) {
            if (!file.FileExists(animFileName))
                return;

            file.Open(animFileName, File.ModeFlags.Read);

            var text = file.GetAsText();
            var json = JSON.Parse(text);

            if (!(json.Result is Godot.Collections.Dictionary animDict))
                return;

            Name = animDict.S("n");
            TotalTime = animDict.F("t");
            Overdrive = animDict.F("o");
            IsLoop = animDict.I("l") == 1;

            // Bones
            RemoveAllBones();

            var bonesArray = animDict.Array("a");
            if (bonesArray == null)
                return;
            foreach (Godot.Collections.Dictionary b in bonesArray) {
                var boneName = b.S("b");
                var animBone = AddBone(person, boneName);
                if (animBone == null)
                    continue;

                // Keys
                var keysArray = b.Array("a");
                if (keysArray == null)
                    continue;

                foreach (Godot.Collections.Dictionary k in keysArray) {
                    var time = k.F("t");
                    var rate = k.F("r");
                    var type = (AAnimKey.EType)k.I("e");
                    var x = k.F("x");
                    var y = k.F("y");
                    var z = k.F("z");

                    animBone.AddKey(new AAnimKey {
                        Time = time,
                        Rate = rate,
                        Type = type,
                        Value = new Vector3(x, y, z)
                    });
                } // foreach key
            } // foreach Bone

            file.Close();
        }
    } // Load

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AAnimation Create() {
        var a = new AAnimation {
            Bones = new AAnimBone[0]
        };
        return a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AAnimation Create(string resourceFile, APerson person) {
        var a = new AAnimation();
        a.Load(person, resourceFile);
        return a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AAnimBone GetBone(ABone bone) {
        return Bones.FirstOrDefault(x => x.Bone == bone);       
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AAnimBone GetBone(string boneName) {
        return Bones.FirstOrDefault(x => x.Bone.Name == boneName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AAnimBone AddBone(APerson person, string boneName) {
        var bone = person.Bones.FirstOrDefault(x => x.Name == boneName);
        if (bone != null)
            return AddBone(bone);
        if (person.Body.Name == boneName)
            return AddBone(person.Body);
        return null;
    }

    public AAnimBone AddBone(ABone bone) {
        var h = GetBone(bone);
        if (h != null)
            return h;

        var list = Bones.ToList();
        var s = AAnimBone.Create(bone);
        list.Add(s);
        Bones = list.ToArray();
        return s;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void RemoveAllBones() {
        Bones = new AAnimBone[0];
    }
} // public class AAnimation

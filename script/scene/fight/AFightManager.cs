using Godot;
using System.Collections.Generic;

public struct SHitData
{
    public APerson Hitter;
    public RigidBody HitterBody;
    public IHarmable Victim;
    public RigidBody VictimBody;
    public Vector3 Impulse;
    public Vector3 WorldPosition;
}

public class AFightManager
{
    /// <summary>
    /// current player
    /// </summary>
    public APerson Player { get; private set; } = null;

    /// <summary>
    /// current Enemy
    /// </summary>
    public APerson Enemy
    {
        get => _enemy;
        private set
        {
            (_enemy ?? value)?.SceneManager.SetEnemy(value);
            _enemy = value;
        }
    }

    /// <summary>
    /// all Person list
    /// </summary>
    readonly AUniqueHolder<APerson> _persons = new AUniqueHolder<APerson>();

    /// <summary>
    /// the list of bodies that can be damaged
    /// to access a Person by body
    /// </summary>
    readonly Dictionary<RigidBody, IHarmable> _harmBodies = new Dictionary<RigidBody, IHarmable>(4);

    APerson _enemy = null;

    public void SetPlayer(APerson player)
    {
        if (this.Player != null)
            RemovePerson(this.Player);

        this.Player = player;
        _persons.Add(player);

        foreach (var rb in player.HarmBodies)
            _harmBodies.Add(rb, player);
    }

    public void AddEnemy(APerson enemy)
    {
        this.Enemy = enemy;
        _persons.Add(enemy);

        foreach (var rb in enemy.HarmBodies)
            _harmBodies.Add(rb, enemy);
    }

    public void AddNPC(APerson npc)
    {
        _persons.Add(npc);

        _harmBodies.Add(npc.Body, npc);
    }

    public void QueueFreePlayer()
    {
        QueueFreePerson(Player);
    }
    public void QueueFreePerson(APerson person)
    {
        if (person == null)
            return;
        var p = person;
        RemovePerson(p);
        p.Finish();
        p.QueueFree();
    }
    public void RemovePerson(APerson person)
    {
        _persons.Remove(person);
        foreach (var rb in person.HarmBodies)
            _harmBodies.Remove(rb);

        if (person == Enemy)
        {
            FindNewEnemy();
        }

        if (Player == person)
            Player = null;
    } // RemovePerson

    public void FindNewEnemy()
    {
        Enemy = null;
        foreach (var p in _persons)
        {
            if (p.TYPE == APerson.EType.ENEMY && p.IsKnockedOut == false)
            {
                Enemy = p;
                break;
            }
        }
    }

    public void AddProjectile(IHarmable obj)
    {
        _harmBodies.Add(obj.RBody, obj);
    }

    public void Update()
    {
        foreach (var p in _persons)
            p.Update();
    }

    public void FixedUpdate()
    {
        foreach (var p in _persons)
            p.FixedUpdate();
    }

    public IHarmable GetHarmable(RigidBody body)
    {
        _harmBodies.TryGetValue(body, out IHarmable h);
        return h;
    }

    public void PerformHit(ref SHitData data)
    {
        var iv = GetHarmable(data.VictimBody);
        if (iv == null || iv.CanReceiveHit() == false)
            return;

        data.Victim = iv;
        data.Hitter.HitDone(ref data);
        iv.ReceiveHit(ref data);
    }

    public bool HasNearestRival(APerson person, Vector3 pos, out APerson rivalPerson, out RigidBody rivalBody)
    {
        rivalPerson = null;
        rivalBody = null;
        float min_sq = person.SceneManager.FIGHT_CHECK_LEN_SQ;

        foreach (var k in _harmBodies)
        {
            if (k.Value is APerson p == false || p == person || p.TYPE == person.TYPE)
                continue;
            var sq = (k.Key.GlobalTransform.origin - pos).LengthSquared();
            if (sq < min_sq && p.IsKnockedOut == false)
            {
                rivalPerson = p;
                rivalBody = k.Key;
                min_sq = sq;
            }
        }
        return rivalBody != null;
    } // HasNearestRival

    public bool KnockOut(RigidBody body)
    {
        if (!_harmBodies.TryGetValue(body, out IHarmable iv))
            return false;
        iv.KnockOut();
        return true;
    }
} // public class AFightManager

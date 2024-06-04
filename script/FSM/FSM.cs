using System.Collections.Generic;

public class FSM<T>
{
	public struct SJumpReason
	{
		public System.Func<bool> NeedJump;
		public T Jump2Action;
	}

	public class AJumpAction: AAction
	{
		readonly List<SJumpReason> _jumps = new List<SJumpReason>();
		public List<SJumpReason> Jumps => _jumps;
	}

	readonly Dictionary<T, AJumpAction> _actions = new Dictionary<T, AJumpAction>();

	AJumpAction _activeAction = null;
	T _defaultActionKey;

	public T ActiveActionKey { get; protected set; }
	public T PrevActionKey { get; protected set; }

	public void Update()
	{
		if (_activeAction != null)
		{
			foreach (var j in _activeAction.Jumps)
			{
				if (j.NeedJump != null && j.NeedJump())
				{
					if (SetAsActive(j.Jump2Action) == null)
						return;
					else
						break;
				}
			}
			_activeAction.Update();
		}
		else
			SetAsActive(_defaultActionKey);
	} // void UpdateOverride

	public AJumpAction GetAction(T key) => _actions[key];

	public AJumpAction AddAction(T key)
	{
		var a = new AJumpAction();
		_actions.Remove(key);
		_actions.Add(key, a);
		return a;
	} // AddAction

	public bool RemoveAction(T key) => _actions.Remove(key);

	bool AddJumpReason(List<SJumpReason> jumpList, T toKey, System.Func<bool> needJump)
	{
		if (!_actions.ContainsKey(toKey))
			return false;

		var r = new SJumpReason
		{
			NeedJump = needJump,
			Jump2Action = toKey
		};
		jumpList.Remove(r);
		jumpList.Add(r);

		return true;
	} // AddJumpReason

    /// <summary>
    /// adding a transition from an action named fromKey to toKey when the needJump condition is met
    /// </summary>
    public bool AddJumpReason(T fromKey, T toKey, System.Func<bool> needJump)
	{
		if (!_actions.TryGetValue(fromKey, out AJumpAction f))
			return false;
		return AddJumpReason(f.Jumps, toKey, needJump);
	} // AddJumpReason

	public bool RemoveJumpReason(T fromKey, T toKey)
	{
		if (!_actions.TryGetValue(fromKey, out AJumpAction f))
			return false;

		int i = f.Jumps.FindIndex(r => { return r.Jump2Action.Equals(toKey); });
		if (i < 0)
			return false;
		f.Jumps.RemoveAt(i);

		return true;
	} // RemoveJumpReason

	public AJumpAction SetAsActive(T key)
	{
		var prev = _activeAction;
		_activeAction = _actions[key];
		PrevActionKey = ActiveActionKey;
		ActiveActionKey = key;

		if (_activeAction == null && !key.Equals(_defaultActionKey))
			return SetAsActive(_defaultActionKey);

		_activeAction?.Start();
		if (prev != _activeAction)
			prev?.OnJumpOff?.Invoke();
		return _activeAction;
	}

	public AJumpAction SetAsDefault(T key)
	{
		_defaultActionKey = key;
		return GetAction(key);
	}

	public bool IsActiveActionFinished() => _activeAction.IsFinished;

	public AJumpAction ActiveAction => _activeAction;
} // public class FSM

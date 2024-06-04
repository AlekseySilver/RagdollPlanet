using System;

public class AAction
{
	protected int _state = 0; // 0 - finished, 1 - active, 2 - Init Start

	protected Action _onStart = null;
	protected Action _onJumpOff = null;
	protected Func<bool> _checkFinish = null;

	public void Start() => _state = 2;
	
	public void Init(Action onStart, Func<bool> checkFinish)
	{
		_onStart = onStart;
		_checkFinish = checkFinish;
	}

	public void Update()
	{
		// check finished
		if (_state == 0)
			return;

		// check need Start
		if (_state == 2)
		{
			_state = 1;
			_onStart?.Invoke();
		}

		// check is finished
		if (_checkFinish != null && _checkFinish())
			_state = 0;
	}

	public Action OnStart
	{
		get => _onStart;
		set => _onStart = value;
	}
	public Func<bool> CheckFinish
	{
		get => _checkFinish;
		set => _checkFinish = value;
	}
	public Action OnJumpOff
	{
		get => _onJumpOff;
		set => _onJumpOff = value;
	}

	public bool IsFinished => _state == 0;
}

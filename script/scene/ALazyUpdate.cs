public class ALazyUpdate
{
    readonly System.Collections.Generic.List<IUpdateable> _list = new System.Collections.Generic.List<IUpdateable>(10);

    public float TimeBetweenUpdate { get; set; } = 1.0f;

    int _id = 0;
    float _restTime;

    public void Update()
    {
        _restTime -= A.TimeStep;
        if (_restTime > 0.0f)
            return;
        _restTime = TimeBetweenUpdate;

        ++_id;
        if (_id >= _list.Count)
            _id = 0;
        if (_list.Count > 0)
        {
            _list[_id].UpdateOverride();
        }
    } // UpdateOverride


    public void Add(IUpdateable obj)
    {
        _list.Add(obj);
    }
    public void Remove(IUpdateable obj)
    {
        _list.Remove(obj);
        // TODO optimize
        //var I = list.IndexOf(obj);
    }
}

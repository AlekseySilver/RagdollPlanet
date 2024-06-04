public interface IControlable
{
    bool CanSeeRival(out APerson rival);

    ABone3 Body { get; }

    bool HasGroundContact { get; }
    APerson.EGMAction FSMActiveAction { get; }
    bool IsGrabbed { get; }

    void ReleaseGrab();
}
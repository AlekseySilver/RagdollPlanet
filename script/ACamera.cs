using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public sealed class ACamera: Camera
{
    [Export]
    public float CAM_ARM_SPEED = 50.0f;

    [Export]
    public float CAM_ROT_SPEED = 2.0f;

    [Export]
    public float GAP
    {
        get => _gap;
        set
        {
            _gap = value;
        }
    }

    [Export] public float LOOK_AT_SPEED = 3.0f;
    [Export] public float CLOSE_TARGET_DIST = 7.0f;
    [Export] public float AUTOTURN_RAY_APERTURE = 15.0f;
    [Export] public float AUTOTURN_SPEED = 0.5f;
    [Export] public float ARM_GAP = 1.0f;
    [Export] public float USER_X_OVERRIDE_SPEED = 1.0f;


    public float ArmLength
    {
        get => _armLength;
        set
        {
            _armLength = Mathf.Clamp(value, 10f, 350f);
        }
    }
    float _armLength = 15.0f;
    float _gap = 25.0f;
    float _rayHitLength = 0.0f;

    public bool IsTargetClose => _rayHitLength > 0.0f && _rayHitLength < CLOSE_TARGET_DIST;

    public float MinRadius
    {
        get => _minRadius;
        set
        {
            _minRadius = value;
            _minRadiusSq = value * value;
        }
    }
    float _minRadius = 0.0f;
    float _minRadiusSq = 0.0f;

    Vector2 _userFlatRotate = Vector2.Zero;

    Spatial _targetNode;
    Vector3 _targetWorldOffset;

    Vector3 _2lookAtDirection = Vector3.Zero;
    Vector3 _2lookAtAround = Vector3.Zero;
    float _2lookAtAngle = 0.0f;

    Vector3 _linVel = Vector3.Zero;

    float _userXOverride = 0.0f;
    float _userXOverrideChange = 0.0f;


    public Spatial TargetNode
    {
        get => _targetNode;
        private set
        {
            _targetNode = value;
            LastTargetPosition = GetTargetPosition();
        }
    }
    public Vector3 TargetWorldOffset
    {
        get => _targetWorldOffset;
        private set
        {
            _targetWorldOffset = value;
            LastTargetPosition = GetTargetPosition();
        }
    }
    public Vector3 CurrentCameraDirection { get; private set; }
    public Vector3 GlobalUp { get; set; } = Vector3.Up;
    public Vector3 CurrentCameraRight { get; set; } = Vector3.Right;

    Vector3 GetTargetPosition() => TargetNode.GlobalTransform.origin + TargetWorldOffset;

    public Vector3 DirectPad2World(in Vector2 pad)
    {
        var forward = GlobalUp.Cross(CurrentCameraRight);
        return CurrentCameraRight * pad.x + forward * pad.y;
    }

    public void Rotate(in Vector2 flat)
    {
        _userFlatRotate = flat;
    }
    public void Zoom(float v)
    {
        if (v != 0.0f)
            ArmLength += v * CAM_ARM_SPEED * A.TimeStep;
    }
    public void RotateAndZoom(in Vector3 v)
    {
        _userFlatRotate = v.XY();
        if (v.z != 0.0f)
            ArmLength += v.z * CAM_ARM_SPEED * A.TimeStep;
    }


    public Vector2 FrustumNear
    {
        get
        {
            var camera = GetCamera();
            var viewportSize = GetViewport().GetVisibleRect().Size;
            var aspect = viewportSize.Aspect();

            float halfFOV = Mathf.Deg2Rad(camera.Fov) * 0.5f;
            var tan = (float)Math.Tan(halfFOV);

            if (camera.KeepAspect == Camera.KeepAspectEnum.Width)
                return new Vector2(tan, -tan / aspect);
            return new Vector2(tan * aspect, -tan);
        }
    }

    Camera GetCamera() => this.FirstChild<Camera>(false);


    /// <summary>
    /// the Position of the Target, where the Camera is looking
    /// it may differ from the actual Position of the Target GetTargetPosition() by the GAP
    /// </summary>
    public Vector3 LastTargetPosition { get; private set; }

    public override void _Process(float delta)
    {
        if (TargetNode == null)
            return;

        ImmediateXform();

        if (_2lookAtAngle > 0.0f)
        { // look at point
            _2lookAtAngle -= delta * LOOK_AT_SPEED;
            if (_2lookAtAngle <= 0.0f)
            {
                CurrentCameraDirection = _2lookAtDirection;
            }
            else
            {
                CurrentCameraDirection = new Basis(_2lookAtAround, _2lookAtAngle).XformInv(_2lookAtDirection);
            }
        }
        else
        {
            Vector2 rot2 = _userFlatRotate;
            if (_userXOverrideChange != 0.0f)
            {
                _userXOverride += _userXOverrideChange * A.TimeStep * AUTOTURN_SPEED;
            }
            else if (_userXOverride != 0.0f)
            {
                if (_userXOverride > 0.0f)
                {
                    _userXOverride -= A.TimeStep * AUTOTURN_SPEED;
                    if (_userXOverride < 0.0f)
                        _userXOverride = 0.0f;
                }
                else
                {
                    _userXOverride += A.TimeStep * AUTOTURN_SPEED;
                    if (_userXOverride > 0.0f)
                        _userXOverride = 0.0f;
                }
            }
            if (_userXOverride != 0.0f && rot2.x == 0.0f)
            {
                rot2.x = _userXOverride;
            }

            if (rot2 != Vector2.Zero)
            { // user'S turn
                float d = CurrentCameraDirection.Dot(GlobalUp);
                if (rot2.y != 0.0f)
                {
                    CurrentCameraDirection = new Basis(CurrentCameraRight, rot2.y * delta * CAM_ROT_SPEED).Xform(CurrentCameraDirection);

                    if (d > Xts.SIN80)
                    {
                        CurrentCameraDirection = GlobalUp.Cross(CurrentCameraRight) * Xts.SIN10 + GlobalUp * Xts.SIN80;
                    }
                    else if (d < -Xts.SIN80)
                    {
                        CurrentCameraDirection = GlobalUp.Cross(CurrentCameraRight) * Xts.SIN10 - GlobalUp * Xts.SIN80;
                    }

                    CurrentCameraDirection = CurrentCameraDirection.Normalized();
                }

                if (rot2.x != 0.0f)
                {
                    if (d < 0.0f)
                        d = -d;
                    d = 1f - d * .75f;
                    CurrentCameraDirection = new Basis(Transform.basis.Column1, rot2.x * delta * -CAM_ROT_SPEED * d).Xform(CurrentCameraDirection);
                }
                if (_2lookAtAngle > 0.0f)
                    _2lookAtAngle = 0.0f; // cancel look at
            }
        }
    } // _Process

    public void ImmediateXform()
    {
        var pos = GetTargetPosition();
        var ln = (pos - LastTargetPosition).LengthSquared();
        if (ln > GAP * GAP)
            LastTargetPosition = pos;
        else
        {
            ln = (float)Math.Sqrt(ln);
            ln /= GAP;
            ln = ln * ln * (3f - 2f * ln);
            LastTargetPosition = Xts.Lerp(LastTargetPosition, pos, ln);
        }

        // Camera Direction
        GlobalUp = LastTargetPosition.Normalized();
        CurrentCameraRight = CurrentCameraDirection.Cross(GlobalUp).Normalized();
        var up = CurrentCameraRight.Cross(CurrentCameraDirection).Normalized();

        var armOffset = CurrentCameraDirection * -(_rayHitLength != 0.0f ? _rayHitLength : ArmLength);
        pos = armOffset + LastTargetPosition;

        Transform = new Transform(CurrentCameraRight, up, -CurrentCameraDirection, pos);
    } // ImmediateXform

    public override void _PhysicsProcess(float _delta)
    {
        var ds = PhysicsServer.SpaceGetDirectState(GetWorld().Space);

        var arm_offset = CurrentCameraDirection * -ArmLength;
        var pos = arm_offset + LastTargetPosition;
        var delta = pos - LastTargetPosition;

        var col = ds.IntersectRay(LastTargetPosition, pos, collisionMask: Xts.GROUND_LAYER_VALUE);
        // TODO optimize
        var col_left = ds.IntersectRay(LastTargetPosition
                , LastTargetPosition + new Basis(GlobalUp, Xts.deg2rad * AUTOTURN_RAY_APERTURE).Xform(delta)
                , collisionMask: Xts.GROUND_LAYER_VALUE);
        var col_right = ds.IntersectRay(LastTargetPosition
                , LastTargetPosition + new Basis(GlobalUp, Xts.deg2rad * -AUTOTURN_RAY_APERTURE).Xform(delta)
                , collisionMask: Xts.GROUND_LAYER_VALUE);

        if (col.Count > 0)
        {
            // If main ray was occluded, Get Camera closer, this is the worst case scenario.
            delta = (Vector3)col["position"] - LastTargetPosition;
            _rayHitLength = delta.Length() - ARM_GAP;
        }
        else
        {
            _rayHitLength = 0.0f;
        }

        if (col_left.Count > 0 && col_right.Count == 0)
        {
            // If only left ray is occluded, turn the Camera around to the right.
            _userXOverrideChange = 1f;
        }
        else if (col_left.Count == 0 && col_right.Count > 0)
        {
            // If only right ray is occluded, turn the Camera around to the left.
            _userXOverrideChange = -1;
        }
        else // Do nothing otherwise, left and right are occluded but Center is not, so do not autoturn.
        {
            _userXOverrideChange = 0.0f;
        }
    } // _PhysicsProcess

    public void LookAt(in Vector3 dir, bool forced = false)
    {
        _2lookAtDirection = dir;

        float d = CurrentCameraDirection.Dot(_2lookAtDirection);
        if (d > Xts.SIN85)
            return;

        if (d < -Xts.SIN85)
        {
            _2lookAtAround = GlobalUp;
        }
        else
        {
            _2lookAtAround = CurrentCameraDirection.Cross(_2lookAtDirection).Normalized();
        }

        _2lookAtAngle = (float)Math.Acos(d);
    } // LookAt



    #region target assing

    public void AssignTarget(Spatial targetNode, Vector3 offset)
    {
        StopAssignTarget();
        if (targetNode != null)
        {
            this.TargetNode = targetNode;
            TargetWorldOffset = offset;
            LastTargetPosition = GetTargetPosition();

            var delta = LastTargetPosition - Transform.origin;
            CurrentCameraDirection = delta.Normalized();
            if (delta.LengthSquared() > 100f || GlobalUp == Vector3.Zero)
                ImmediateXform();
        }
    }
    public void AssignTarget(Spatial targetNode)
    {
        AssignTarget(targetNode, Vector3.Zero);
    }

    readonly Queue<Action> _onStopAssignQueue = new Queue<Action>(1);
    void StopAssignTarget()
    {
        while (_onStopAssignQueue.Count > 0)
            _onStopAssignQueue.Dequeue()?.Invoke();
    }

    public async Task AssignTargetAsync(Spatial targetNode)
    {
        await AssignTargetAsync(targetNode, Vector3.Zero);
    }
    public async Task AssignTargetAsync(Spatial targetNode, Vector3 offset)
    {
        await AssignTargetAsync(targetNode, offset, GetTargetPosition());
    }
    public async Task AssignTargetAsync(Spatial targetNode, Vector3 offset, Vector3 prevWorldPosition)
    {
        if (this.TargetNode == null)
        {
            AssignTarget(targetNode, offset);
            return;
        }
        StopAssignTarget();

        // increase the indentation so that when the Target is changed, the final point does not change
        TargetWorldOffset += prevWorldPosition - targetNode.GlobalTransform.origin;

        // after that, replace the Target
        this.TargetNode = targetNode;

        // Next, the offset shift from the current to the new one
        var l = new ALerperVector3
        {
            A = TargetWorldOffset,
            B = offset,
        };
        var a = new ACustomAwaiterUpdate<bool>((A.App.SceneManager as IUpdateHolder) ?? A.App);
        a.OnUpdate = () =>
        {
            if (l.Update(A.TimeStep))
                TargetWorldOffset = l.C;
            else
                a.Finish(true);
        };
        _onStopAssignQueue.Enqueue(() => a.Finish(false));
        if (await a.WaitAsync())
            TargetWorldOffset = offset;
    } // AssignTargetAsync

    #endregion
}
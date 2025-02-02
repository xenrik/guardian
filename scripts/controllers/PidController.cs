using System;
using System.Reflection;
using Godot;

public abstract class PidController<T> {
    public T Target;

    private T _current;
    public T Current {
        get {
            return _current;
        }

        set {
            Last = _current;
            _current = value;
        }
    }

    public T Last { get; private set; }
    public T Min;
    public T Max;

    private PidSettings pid;

    public static PidController<float> Instantiate(float initial = default, PidSettings pid = default) {
        return new PidController_float(initial, pid);
    }
    public static PidController<Vector2> Instantiate(Vector2 initial = default, PidSettings pid = default) {
        return new PidController_Vector2(initial, pid);
    }
    public static PidController<Vector3> Instantiate(Vector3 initial = default, PidSettings pid = default) {
        return new PidController_Vector3(initial, pid);
    }

    protected PidController(T initial, PidSettings pid) {
        this.pid = pid ?? new PidSettings();

        _current = initial;
        Last = initial;
    }

    public abstract T Update(float delta);

    protected float Update(float target, float current, float last, float delta, float min, float max, ref float termIntegral) {
        float error = target - current;

        termIntegral += pid.I * error * delta;
        termIntegral = MathUtils.Clamp(termIntegral, min, max);

        float input = current - last;
        float tD = pid.D * (input / delta);

        float tP = pid.P * error;

        float result = tP + termIntegral - tD;
        result = MathUtils.Clamp(result, min, max);

        return result;
    }

    private class PidController_float : PidController<float> {
        private float termIntegral;

        public PidController_float(float initial, PidSettings pid = default)
                : base(initial, pid) {
            Min = float.MinValue;
            Max = float.MaxValue;
        }

        public override float Update(float delta) {
            return Update(Target, Current, Last, delta, Min, Max, ref termIntegral);
        }
    }

    private class PidController_Vector2 : PidController<Vector2> {
        private Vector2 termIntegral;

        public PidController_Vector2(Vector2 initial, PidSettings pid = default)
                : base(initial, pid) {
            Min = new Vector2(float.MinValue, float.MinValue);
            Max = new Vector2(float.MaxValue, float.MaxValue);
        }

        public override Vector2 Update(float delta) {
            float x = Update(Target.X, Current.X, Last.X, delta, Min.X, Max.X, ref termIntegral.X);
            float y = Update(Target.Y, Current.Y, Last.Y, delta, Min.Y, Max.Y, ref termIntegral.Y);

            return new Vector2(x, y);
        }
    }

    private class PidController_Vector3 : PidController<Vector3> {
        private Vector3 termIntegral;

        public PidController_Vector3(Vector3 initial, PidSettings pid = default)
                : base(initial, pid) {
            Min = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            Max = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        }

        public override Vector3 Update(float delta) {
            float x = Update(Target.X, Current.X, Last.X, delta, Min.X, Max.X, ref termIntegral.X);
            float y = Update(Target.Y, Current.Y, Last.Y, delta, Min.Y, Max.Y, ref termIntegral.Y);
            float z = Update(Target.Z, Current.Z, Last.Z, delta, Min.Z, Max.Z, ref termIntegral.Z);

            return new Vector3(x, y, z);
        }
    }
}

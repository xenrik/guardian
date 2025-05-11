using Godot;
using Godot.Collections;

public static class SerializerUtils {
    public static Variant ToJson<[MustBeVariant] T>(T value) {
        if (typeof(T) == typeof(string)) {
            return (string)(object)value;
        } else if (typeof(T) == typeof(Dictionary)) {
            return (Dictionary)(object)value;
        } else if (typeof(T) == typeof(Array)) {
            return (Array)(object)value;
        } else if (typeof(T) == typeof(Transform3D)) {
            return ToJson_Transform3D((Transform3D)(object)value);
        } else {
            Logger.Error("Unsupported type: " + typeof(T));
            return default;
        }
    }

    private static Variant ToJson_Transform3D(Transform3D value) {
        Dictionary d = new Dictionary();
        Array x = new();
        x.Add(value.Basis.Column0.X); x.Add(value.Basis.Column0.Y); x.Add(value.Basis.Column0.Z);
        d["X"] = x;

        Array y = new();
        y.Add(value.Basis.Column1.X); y.Add(value.Basis.Column1.Y); y.Add(value.Basis.Column1.Z);
        d["Y"] = y;

        Array z = new();
        z.Add(value.Basis.Column2.X); z.Add(value.Basis.Column2.Y); z.Add(value.Basis.Column2.Z);
        d["Z"] = z;

        Array o = new();
        o.Add(value.Origin.X); o.Add(value.Origin.Y); o.Add(value.Origin.Z);
        d["O"] = o;

        return d;
    }

    public static T FromJson<[MustBeVariant] T>(Variant value) {
        if (typeof(T) == typeof(string) ||
                typeof(T) == typeof(Dictionary) ||
                typeof(T) == typeof(Array)) {
            return (T)(object)value.As<T>();
        } else if (typeof(T) == typeof(Transform3D)) {
            return (T)(object)FromJson_Transform3D(value.AsGodotDictionary());
        } else {
            Logger.Error("Unsupported type: " + typeof(T));
            return default;
        }
    }

    private static Transform3D FromJson_Transform3D(Dictionary d) {
        Transform3D t = new();

        Array x = d["X"].AsGodotArray();
        Array y = d["Y"].AsGodotArray();
        Array z = d["Z"].AsGodotArray();
        Array o = d["O"].AsGodotArray();

        t.Basis.Column0 = new Vector3(x[0].AsSingle(), x[1].AsSingle(), x[2].AsSingle());
        t.Basis.Column1 = new Vector3(y[0].AsSingle(), y[1].AsSingle(), y[2].AsSingle());
        t.Basis.Column2 = new Vector3(z[0].AsSingle(), z[1].AsSingle(), z[2].AsSingle());
        t.Origin = new Vector3(o[0].AsSingle(), o[1].AsSingle(), o[2].AsSingle());

        return t;
    }
}
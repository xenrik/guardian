using System;
using System.Globalization;
using Godot;

/**
 * A variant of Vector3 that uses doubles instead of floats. Used in floating origin
 * calculations for example.
 */
[Serializable]
public struct Vector3d : IEquatable<Vector3d> {
    public static readonly Vector3d Zero = new Vector3d(0, 0, 0);

    public static implicit operator Vector3d(Vector3 a) => new Vector3d(a.X, a.Y, a.Z);
    public static explicit operator Vector3(Vector3d a) => new Vector3((float)a.X, (float)a.Y, (float)a.Z);

    public static Vector3d operator +(Vector3d a) => a;
    public static Vector3d operator -(Vector3d a) => new Vector3d(-a.X, -a.Y, -a.Z);

    public static Vector3d operator +(Vector3d left, Vector3d right) {
        left.X += right.X;
        left.Y += right.Y;
        left.Z += right.Z;

        return left;
    }
    public static Vector3d operator -(Vector3d left, Vector3d right) {
        left.X -= right.X;
        left.Y -= right.Y;
        left.Z -= right.Z;

        return left;
    }

    public static bool operator ==(Vector3d left, Vector3d right) {
        return left.Equals(right);
    }
    public static bool operator !=(Vector3d left, Vector3d right) {
        return !left.Equals(right);
    }

    public double X;
    public double Y;
    public double Z;

    public Vector3d(double x, double y, double z) {
        X = x;
        Y = y;
        Z = z;
    }

    public override readonly bool Equals(object obj) {
        if (obj is Vector3d other) {
            return Equals(other);
        }

        return false;
    }

    public override readonly int GetHashCode() {
        return HashCode.Combine(X, Y, Z);
    }

    public override readonly string ToString() {
        return ToString(null);
    }

    public readonly bool Equals(Vector3d other) {
        return X == other.X && Y == other.Y && Z == other.Z;
    }

    public readonly string ToString(string? format) {
        return $"({X.ToString(format, CultureInfo.InvariantCulture)}, {Y.ToString(format, CultureInfo.InvariantCulture)}, {Z.ToString(format, CultureInfo.InvariantCulture)})";
    }
}
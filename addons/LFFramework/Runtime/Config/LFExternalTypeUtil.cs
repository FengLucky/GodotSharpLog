using Godot;

namespace Config;
public static class LFExternalTypeUtil
{
    public static Vector2 NewVector2(Vector2Bean vector)
    {
        return new Vector2(vector.X, vector.Y);
    }
    public static Vector3 NewVector3(Vector3Bean vector)
    {
        return new Vector3(vector.X, vector.Y, vector.Z);
    }

    public static Color NewColor(ColorBean color)
    {
        return new Color(color.R/255f, color.G/255f, color.B/255f, color.A/255f);
    }
}

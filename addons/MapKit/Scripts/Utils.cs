namespace YYZ.MapKit
{


using Godot;

public class Utils // Extracted utilities from YYZ package.
{
    public static string ReadText(string path)
    {
        path = ProjectSettings.GlobalizePath(path);
        return System.IO.File.ReadAllText(path); // Godot's `res://` path should be resolved by Godot itself
    }
}


}
namespace YYZ.MapKit
{


using Godot;
using System;

public class Utils // Extracted utilities from YYZ package.
{
    public static string ReadText(string path)
    {
        var file = new File();
        var error = file.Open(path, File.ModeFlags.Read);

        if(error != Error.Ok)
            throw new ArgumentException($"Load file failed: {error}");

        var text = file.GetAsText();
        file.Close();

        return text;

        /*
        path = ProjectSettings.GlobalizePath(path);
        return System.IO.File.ReadAllText(path); // Godot's `res://` path should be resolved by Godot itself
        */
    }
}


}
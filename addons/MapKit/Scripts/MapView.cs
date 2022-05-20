namespace YYZ.MapKit
{


using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// MapView controls all the map related UI (MapShower sprite, counters on the map etc...). MapView is relatively self-included,
/// so it has capture some events from the engine and sends them to MapShower, and it's expected to be contained in a `Viewport` or `ViewportContainer`. 
/// 
/// MapView has some "widgets", the outside should not access those widget directly.
///
/// MapView will store state to or restore state from a resource, since itself doesn't control scene switching, the store should be called from outside.
/// </summary>
public class MapView<TArea> : Node2D where TArea : IArea //, IMapView 
{
    [Export] public NodePath mapShowerPath;
    [Export] NodePath cameraPath;

    protected MapShower<TArea> mapShower;
    protected Camera2D camera;

    protected bool dragging = false;

    Vector2 cameraBeginPos;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        mapShower = (MapShower<TArea>)GetNode(mapShowerPath);
        camera = (Camera2D)GetNode(cameraPath);

        cameraBeginPos = camera.Position;
    }

    /// <summary>
    /// Handle navigation and zooming
    /// </summary>
    public override void _UnhandledInput(InputEvent @event) // event is C# keyword
    {
        var buttonEvent = @event as InputEventMouseButton;
        if(buttonEvent != null)
        {
            switch(buttonEvent.ButtonIndex)
            {
                case (int)ButtonList.Left: // 1
                    if(buttonEvent.Pressed)
                    {
                        GD.Print("Left Click");
                        var pos = mapShower.GetLocalMousePosition();
                        mapShower.OnClick(pos);
                    }
                    break;

                case (int)ButtonList.Right: // 2
                    DraggingClickHandler(buttonEvent);
                    break;

                case (int)ButtonList.Middle: // 3 (wheel click)
                    GD.Print("Middle click");
                    break;

                case (int)ButtonList.WheelUp: // 4
                    if(buttonEvent.Pressed)
                    {
                        GD.Print("WheelUp Start");
                        camera.Zoom += new Vector2(0.1f, 0.1f);
                    }
                    break;

                case (int)ButtonList.WheelDown: // 5
                    if(buttonEvent.Pressed)
                    {
                        GD.Print("WheelDown Start");
                        var testZoom = camera.Zoom - new Vector2(0.1f, 0.1f);
                        if(testZoom.x > 0.01f & testZoom.y > 0.01f)
                        {
                            camera.Zoom = testZoom;
                        }
                    }
                    break;

                default:
                    GD.Print($"Dedefined actions or wheel? {buttonEvent.ButtonIndex}");
                    break;
            }
        }

        var motionEvent = @event as InputEventMouseMotion;
        if(motionEvent != null)
        {
            var pos = mapShower.GetLocalMousePosition();
            mapShower.OnRaycastHit(pos);
            if(dragging)
            {
                camera.Position -= motionEvent.Relative;
            }
        }
    }

    void DraggingClickHandler(InputEventMouseButton buttonEvent)
    {
        dragging = buttonEvent.Pressed;
        // GD.Print($"dragging: {dragging}");
        if(buttonEvent.Pressed)
        {
            cameraBeginPos = camera.Position;
        }
        else
        {
            if(camera.Position.DistanceTo(cameraBeginPos) < 2)
            {
                var pos = mapShower.GetLocalMousePosition();
                mapShower.OnRightClick(pos);
            }
        }
    }


    public MapShower<TArea> GetMapShower() => mapShower;

    public Camera2D GetCamera() => camera;
}


}
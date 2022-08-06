using Godot;
using System;

public class PlayerCamera : Camera2D
{
    public const string GROUP_CAMERAS = "Cameras";

    public static PlayerCamera Instance { get; private set; } = null;

    ///
    /// For camera zoom
    ///
    private readonly Vector2 ZOOM_MIN = new Vector2(0.5f, 0.5f);
    private readonly Vector2 ZOOM_MAX = new Vector2(10f, 10f);
    private readonly Vector2 ZOOM_STEP = new Vector2(0.2f, 0.2f);
    private readonly float ZOOM_SNAP_DISTANCE = 0.02f;
    private readonly float ZOOM_FACTOR = 10f;
    private Vector2 _targetZoom = Vector2.One;

    ///
    /// For camera pan
    ///
    private readonly float PAN_FACTOR = 10f;
    private readonly float PAN_SPEED = 2.5f;
    private bool _isDragging = false;
    private Vector2 _lastMousePos;
    private Vector2 _targetCameraPosition;
    private bool _active = true;

    private Rect2 _cameraBounds;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (Instance != null)
        {
            GD.Print($"Tried to add more than one PlayerCamera");
            QueueFree();
            return;
        }
        Name = "PlayerCamera";
        Instance = this;
        AddToGroup(GROUP_CAMERAS);
        _targetCameraPosition = GlobalPosition;

        // This might need to be adjusted when your primary actor moves
        _cameraBounds = new Rect2(new Vector2(-499999999, -499999999), new Vector2(999999999, 999999999));
        _targetZoom = ZOOM_MIN;

    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public override void _Process(float delta)
    {
        // We don't disable this so we could subscribe to events for panning / zooming the camera from cinematics
        if (_isDragging)
        {
            UpdateTargetPosition();
            _lastMousePos = GetGlobalMousePosition();
        }

        // Adjust zoom
        Zoom = Zoom - ((Zoom - _targetZoom) * delta * ZOOM_FACTOR);
        if (Zoom.DistanceTo(_targetZoom) <= ZOOM_SNAP_DISTANCE)
        {
            Zoom = _targetZoom;
        }
        // Adjust pan pos
        GlobalPosition = GlobalPosition - ((GlobalPosition - _targetCameraPosition) * delta * PAN_FACTOR);
    }

    private void UpdateTargetPosition()
    {
        _targetCameraPosition += (_lastMousePos - GetGlobalMousePosition()) * PAN_SPEED;
        if (!_cameraBounds.HasPoint(_targetCameraPosition))
        {
            if (_targetCameraPosition.x < _cameraBounds.Position.x)
            {
                _targetCameraPosition.x = _cameraBounds.Position.x;
            }
            if (_targetCameraPosition.y < _cameraBounds.Position.y)
            {
                _targetCameraPosition.y = _cameraBounds.Position.y;
            }
            if (_targetCameraPosition.x > _cameraBounds.Position.x + _cameraBounds.Size.x)
            {
                _targetCameraPosition.x = _cameraBounds.Position.x + _cameraBounds.Size.x;
            }
            if (_targetCameraPosition.y > _cameraBounds.Position.y + _cameraBounds.Size.y)
            {
                _targetCameraPosition.y = _cameraBounds.Position.y + _cameraBounds.Size.y;
            }
        }
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (!_active)
        {
            return;
        }
        if (inputEvent is InputEventMouseButton)
        {
            UpdateCamera(inputEvent as InputEventMouseButton);
        }
    }

    public void UpdateCamera(InputEventMouseButton inputEvent)
    {
        UpdateZoom(inputEvent);
        UpdatePan(inputEvent);
    }

    public void Pan(Vector2 panAmount)
    {
        _targetCameraPosition += panAmount;
    }

    private void UpdatePan(InputEventMouseButton inputEvent)
    {
        if (inputEvent.ButtonIndex == (int)ButtonList.Middle)
        {
            GetTree().SetInputAsHandled();
            _isDragging = inputEvent.IsPressed();
            _lastMousePos = GetGlobalMousePosition();
            _targetCameraPosition = GlobalPosition;
        }
    }

    private void UpdateZoom(InputEventMouseButton inputEvent)
    {
        // Adjust our zoom target
        if (inputEvent.ButtonIndex == (int)ButtonList.WheelDown)
        {
            _targetZoom += ZOOM_STEP;
            GetTree().SetInputAsHandled();
        }
        else if (inputEvent.ButtonIndex == (int)ButtonList.WheelUp)
        {
            _targetZoom -= ZOOM_STEP;
            GetTree().SetInputAsHandled();
        }

        // Make sure values are limited
        if (_targetZoom < ZOOM_MIN)
        {
            _targetZoom = ZOOM_MIN;
        }
        else if (_targetZoom > ZOOM_MAX)
        {
            _targetZoom = ZOOM_MAX;
        }
    }

    public static void Init(Node parent)
    {
        if (PlayerCamera.Instance == null)
        {
            PlayerCamera cam = new PlayerCamera();
            parent.AddChild(cam);
            cam.Current = true;
        }
    }

    public static IntPair GetTileUnderMouse()
    {
        if (Instance == null)
        {
            return new IntPair(0, 0);
        }
        Vector2 mousePos = Instance.GetGlobalMousePosition();
        var pos = new Vector2(Mathf.Floor(mousePos.x / GameManager.TILE_SIZE),
                                     Mathf.Floor(mousePos.y / GameManager.TILE_SIZE));
        return new IntPair(pos);
    }

    public static Rect2 GetCurrentViewAreaTilesAsRect()
    {
        if (Instance == null)
        {
            return new Rect2();
        }
        var vTrans = Instance.GetCanvasTransform();
        var topLeft = -vTrans.origin / vTrans.Scale;
        var vSize = Instance.GetViewportRect().Size * Instance.Zoom;

        var topLeftPair = new Vector2(Mathf.Floor(topLeft.x / GameManager.TILE_SIZE),
                                     Mathf.Floor(topLeft.y / GameManager.TILE_SIZE));
        var btmRight = new Vector2(Mathf.Ceil((topLeft.x + vSize.x) / GameManager.TILE_SIZE),
                                   Mathf.Ceil((topLeft.y + vSize.y) / GameManager.TILE_SIZE));
        return new Rect2(topLeftPair, btmRight - topLeftPair);
    }

    public static IntPair GetTopLeftTile()
    {
        if (Instance == null)
        {
            return new IntPair(0, 0);
        }
        var vTrans = Instance.GetCanvasTransform();
        var topLeft = -vTrans.origin / vTrans.Scale;

        return new IntPair(Mathf.FloorToInt(topLeft.x / GameManager.TILE_SIZE),
                                     Mathf.FloorToInt(topLeft.y / GameManager.TILE_SIZE));
    }

    public static IntPair GetBottomRightTile()
    {
        if (Instance == null)
        {
            return new IntPair(0, 0);
        }
        var vTrans = Instance.GetCanvasTransform();
        var topLeft = -vTrans.origin / vTrans.Scale;
        var vSize = Instance.GetViewportRect().Size * Instance.Zoom;

        var topLeftPair = new IntPair(Mathf.FloorToInt(topLeft.x / GameManager.TILE_SIZE),
                                     Mathf.FloorToInt(topLeft.y / GameManager.TILE_SIZE));
        return new IntPair(Mathf.CeilToInt((topLeft.x + vSize.x) / GameManager.TILE_SIZE),
                                   Mathf.CeilToInt((topLeft.y + vSize.y) / GameManager.TILE_SIZE));
    }
}

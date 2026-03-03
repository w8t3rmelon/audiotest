using Godot;

[Tool]
[GlobalClass]
public partial class Knob : Range
{
    private bool _captured;
    private Vector2? _lastPosition = null;

    [Export] public int ArcPointCount = 60;
    [Export] public float ArcWidth = 3;

    [Export] public bool FromCenter = false;

    [Export]
    [ExportGroup("Theme Overrides")]
    public Color TrackColour
    {
        get => field;
        set
        {
            if (HasThemeColorOverride("track_colour") && value == default) RemoveThemeColorOverride("track_colour");
            else AddThemeColorOverride("track_colour", value);
            field = value;
        }
    } = new Color(0.1f, 0.1f, 0.1f);

    [Export]
    [ExportGroup("Theme Overrides")]
    public Color FillColour
    {
        get => field;
        set
        {
            if (HasThemeColorOverride("fill_colour") && value == default) RemoveThemeColorOverride("fill_colour");
            else AddThemeColorOverride("fill_colour", value);
            field = value;
        }
    } = new Color(0.8f, 0.8f, 0.8f);

    [Export]
    [ExportGroup("Theme Overrides")]
    public Color DialColour
    {
        get => field;
        set
        {
            if (HasThemeColorOverride("dial_colour") && value == default) RemoveThemeColorOverride("dial_colour");
            else AddThemeColorOverride("dial_colour", value);
            field = value;
        }
    } = new Color(0.2f, 0.2f, 0.2f);

    public override void _Ready()
    {
            
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton)
        {
            InputEventMouseButton e = (InputEventMouseButton)@event;
            if (e.ButtonIndex == MouseButton.Left)
            {
                if (e.Pressed)
                {
                    _captured = true;
                    _lastPosition = GetGlobalMousePosition();
                    Input.MouseMode = Input.MouseModeEnum.Captured;
                } else
                {
                    _captured = false;
                    Input.MouseMode = Input.MouseModeEnum.Visible;
                    if (_lastPosition is not null) Input.WarpMouse((Vector2)_lastPosition);
                }
            } else if (e.ButtonIndex == MouseButton.WheelDown && e.Pressed)
            {
                Value -= Step;
            } else if (e.ButtonIndex == MouseButton.WheelUp && e.Pressed)
            {
                Value += Step;
            }
        }
    }

    private float _delta;

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion)
        {
            InputEventMouseMotion e = (InputEventMouseMotion)@event;
            if (_captured)
            {
                float magnitude = e.Relative.Length();
                float dir = (e.Relative.Angle() + Mathf.Pi);
                if (dir < 2.35619449019 || dir > 5.49778714378) magnitude *= -1;

                _delta += magnitude;

                if (Mathf.Abs(_delta) > Step)
                {
                    Value -= _delta;
                    _delta -= (float)Mathf.Snapped(_delta, Step);
                }
            }
        }
    }

    public override void _Draw()
    {
        double val = (Value + Mathf.Abs(MinValue)) / (MaxValue + Mathf.Abs(MinValue));
        DrawCircle(Size / 2, Size.Length() / 4, DialColour, true, -1, true);
        DrawArc(
            Size / 2, 
            (Size.Length() / 4) + (Size.Length() / 16), 
            Mathf.DegToRad(-240), 
            Mathf.DegToRad(60),
            ArcPointCount, 
            TrackColour,
            ArcWidth, 
            true
        );
        DrawArc(
            Size / 2, 
            (Size.Length() / 4) + (Size.Length() / 16), 
            Mathf.DegToRad(FromCenter ? -90 : -240), 
            (float)Mathf.DegToRad(-240 + (val * 300)),
            ArcPointCount, 
            FillColour,
            ArcWidth, 
            true
        );
    }
}
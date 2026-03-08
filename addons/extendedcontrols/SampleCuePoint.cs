
using Godot;

[Tool]
[GlobalClass]
public partial class SampleCuePoint : Range
{
    [Export]
    [ExportGroup("Theme Overrides")]
    public Color PointColour
    {
        get => field;
        set
        {
            if (HasThemeColorOverride("point_colour") && value == default) RemoveThemeColorOverride("point_colour");
            else AddThemeColorOverride("point_colour", value);
            field = value;
        }
    } = Colors.White;

    private float Pos => (float)((Value - MinValue) / (MaxValue - MinValue)) * Size.X;

    private float KnobWidth => Size.Y / 3;

    private bool _captured;

    public override void _Draw()
    {
        DrawColoredPolygon([
            new Vector2(-KnobWidth + Pos, 0),
            new Vector2(Pos, Size.Y),
            new Vector2(KnobWidth + Pos, 0),
            new Vector2(-KnobWidth + Pos, 0)
        ], PointColour);
        DrawLine(
            new Vector2(Pos, 0),
            new Vector2(Pos, 9999),
            PointColour,
            1,
            true
        );
    }

    private bool PositionIsTouchingKnob(Vector2 pos)
    {
        return pos.X >= Pos - KnobWidth / 2 &&
               pos.X <= Pos + KnobWidth / 2;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion eMove)
        {
            if (_captured)
            {
                Value = (MinValue + (((eMove.Position.X + Position.X) / Size.X) * MaxValue));
            }
        } else if (@event is InputEventMouseButton eBtn)
        {
            if (eBtn.ButtonIndex == MouseButton.Left)
            {
                if (eBtn.Pressed && PositionIsTouchingKnob(eBtn.Position))
                    _captured = true;
                else _captured = false;
            }
        }
    }
}
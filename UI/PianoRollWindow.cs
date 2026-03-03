using Godot;

namespace audiotest.UI
{
	public partial class PianoRollWindow : Control
	{
		[Export] public PianoRoll PianoRoll;
		public override void _Ready()
		{
			PianoRoll = GetNode<PianoRoll>("PianoRoll");

			GetNode<SpinBox>("Toolbar/SnapToOption").ValueChanged += (double val) =>
			{
				PianoRoll.Snap = (float)val;
				PianoRoll.QueueRedraw();
			};

			GetNode<Button>("Toolbar/DrawMode").Pressed += () =>
            {
                PianoRoll.PreviousMode = PianoRollMode.Draw;
                PianoRoll.Mode = PianoRollMode.Draw;
				PianoRoll.QueueRedraw();
			};

			GetNode<Button>("Toolbar/EraseMode").Pressed += () =>
            {
                PianoRoll.PreviousMode = PianoRollMode.Erase;
                PianoRoll.Mode = PianoRollMode.Erase;
				PianoRoll.QueueRedraw();
			};


			GetNode<Button>("Toolbar/SelectMode").Pressed += () =>
            {
                PianoRoll.PreviousMode = PianoRollMode.Select;
                PianoRoll.Mode = PianoRollMode.Select;
				PianoRoll.QueueRedraw();
			};

            GetNode<Button>("Toolbar/Send").Pressed += () =>
            {
                PianoRoll.Pattern.SendNotesToInstrument();
            };
        }
	}
}

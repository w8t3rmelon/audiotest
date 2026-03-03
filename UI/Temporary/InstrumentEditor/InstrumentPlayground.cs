using Godot;

using audiotest.Core.AudioEngine;
using audiotest.Core.Sequencing;

namespace audiotest.UI.Temporary.InstrumentEditor
{
	public partial class InstrumentPlayground : Control
	{
		public Instrument Instrument;

		[Export] public Label NoteNumberLabel;
		[Export] public SpinBox NoteNumberInput;

		[Export] public SpinBox BeatNumberInput;
		[Export] public SpinBox BeatLengthNumberInput;

		[Export] public Slider VelocitySlider;

		[Export] public Button EmitButton;
		[Export] public Button ScheduleForButton;
		[Export] public Button ClearQueueButton;
		[Export] public Button PanicButton;

		public override void _Ready()
		{
			EmitButton.ButtonDown += () => {
				UI.MainController.Instance.Mixer.TryOpenPreviewClock();
                Instrument.SendEventNow(
					UI.MainController.Instance.Mixer.PreviewClock,
					new NoteEvent(
						new Note { Number = (byte)NoteNumberInput.Value },
						(byte)VelocitySlider.Value,
						true
					)
				);
			};
			EmitButton.ButtonUp += () => {
				Instrument.SendEventNow(
					UI.MainController.Instance.Mixer.Clock,
					new NoteEvent(
						new Note { Number = (byte)NoteNumberInput.Value },
						(byte)VelocitySlider.Value,
						false
					)
				);
			};
			ScheduleForButton.Pressed += () => {
				Instrument.ScheduleNote(
					UI.MainController.Instance.Mixer.Clock.TimeFromBeat(0, BeatNumberInput.Value),
					new NoteEvent(
						new Note { Number = (byte)NoteNumberInput.Value },
						(byte)VelocitySlider.Value,
						true
					)
				);
				Instrument.ScheduleNote(
					UI.MainController.Instance.Mixer.Clock.TimeFromBeat(0, BeatNumberInput.Value + BeatLengthNumberInput.Value),
					new NoteEvent(
						new Note { Number = (byte)NoteNumberInput.Value },
						(byte)VelocitySlider.Value,
						false
					)
				);
			};
			ClearQueueButton.Pressed += () =>
			{
				Instrument.EventQueue.Clear();
			};
			PanicButton.Pressed += () =>
			{
				Instrument.Channels.Clear();
			};
			NoteNumberInput.ValueChanged += (double val) =>
			{
				NoteNumberLabel.Text = $"Note Number ({new Note { Number = (byte)val }.Name})";
			};
		}
	}
}

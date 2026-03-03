using Godot;
using System;

using audiotest.Core.AudioEngine;
using audiotest.Core.Instruments;

namespace audiotest.UI.Temporary.InstrumentEditor
{
	public partial class InstrumentCreator : Control
	{
		[Export] public Button DestroyButton;
		[Export] public Button EditButton;

		[Export] public LineEdit IdInput;

		[Export] public Label InstrumentList;

		public override void _Ready()
		{
			GetNode<Button>("AddTestInstrumentButton").Pressed += () =>
			{
				UI.MainController.Instance.Mixer.SampleProviders.Add(new TestInstrument());
			};
			GetNode<Button>("AddSamplerInstrumentButton").Pressed += () =>
			{
				UI.MainController.Instance.Mixer.SampleProviders.Add(new SamplerInstrument(GetNode<LineEdit>("SamplePathInput").Text));
			};

			DestroyButton.Pressed += () =>
			{
				int idx;
				if (int.TryParse(IdInput.Text, out idx))
				{
					UI.MainController.Instance.Mixer.SampleProviders[idx].Dispose();
                    UI.MainController.Instance.Mixer.SampleProviders.RemoveAt(idx);
					GC.Collect();
                }
			};

			EditButton.Pressed += () =>
			{
				Window window = new Window();
				InstrumentEditor editor = GD.Load<PackedScene>("res://UI/Temporary/InstrumentEditor/InstrumentEditor.tscn").Instantiate<InstrumentEditor>();
				//editor.InstrumentIndex = Int32.Parse(IdInput.Text);
				window.AddChild(editor);
				window.Title = "instrument editor";
				window.Size = new Vector2I(415, 613);
				window.InitialPosition = Window.WindowInitialPosition.CenterPrimaryScreen;
				window.CloseRequested += () => { UI.MainController.Instance.RemoveChild(window); };
				UI.MainController.Instance.AddChild(window);
			};
		}

		public override void _Process(double delta)
		{
			InstrumentList.Text = "";
			for (int i = 0; i < UI.MainController.Instance.Mixer.SampleProviders.Count; i++)
			{
				Instrument inst = (Instrument)UI.MainController.Instance.Mixer.SampleProviders[i];
				InstrumentList.Text += $"[{i}] {inst.Name} ({inst.GetType().Name})\n";
			}
		}
	}
}

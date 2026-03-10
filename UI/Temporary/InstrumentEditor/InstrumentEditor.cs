using Godot;

using audiotest.Core.AudioEngine;
using audiotest.Core.Instruments;

namespace audiotest.UI.Temporary.InstrumentEditor
{
	public partial class InstrumentEditor : Control
	{
		public Instrument Instrument;
		public override void _Ready()
		{
			GetNode<Label>("Title").Text = $"instrument: {Instrument.Name} ({Instrument.GetType().Name})";

			GetNode<InstrumentPlayground>("Tabs/Playground").Instrument = Instrument;
			GetNode<InstrumentParamWindow>("Tabs/Parameters").Instrument = Instrument;
			GetNode<InstrumentParamWindow>("Tabs/Parameters").Refresh();
            GetNode<ADSREditor>("Tabs/Envelope").Envelope = Instrument.Envelope;
			GetNode<ADSREditor>("Tabs/Envelope").Updated += (envelope) =>
			{
                Instrument.Envelope = envelope;
			};
			if (Instrument is SamplerInstrument sampler)
			{
				SampleLoopPointEditor loopPointEditor =
					GD.Load<PackedScene>("res://UI/SampleLoopPointEditor.tscn").Instantiate<SampleLoopPointEditor>();
				loopPointEditor.Name = "Loop Points";
				loopPointEditor.Instrument = sampler;
				GetNode<TabContainer>("Tabs").AddChild(loopPointEditor);
			}
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
		}
	}

}

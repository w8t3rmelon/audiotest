using Godot;

using audiotest.Core.AudioEngine;

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
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
		}
	}

}

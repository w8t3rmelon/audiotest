using audiotest.Core.AudioEngine;

namespace audiotest.Core.Instruments
{
	public class FuckInstrument : SamplerInstrument
	{
		public FuckInstrument() : base("res://Assets/FUCK.wav")
		{
			Name = "FUCK";
			Params = new() {
				{ "loop", new InstrumentParameter(InstrumentParameterType.Toggle, "Loop", false) }
			};
		}
    }
}

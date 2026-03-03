using Godot;

using audiotest.Core.AudioEngine;

namespace audiotest.Core.Instruments
{
    public class TestInstrument : Instrument
    {
        public TestInstrument()
        {
            Name = "Test Instrument";
        }
        public override Vector2 GetNoteSample(Clock clock, ref NoteState state)
        {
            return Vector2.One * 
                (float)Mathf.Sin(((clock.Time - state.StartTime) / (double)clock.SampleRate) * (state.Event.Note.Frequency * (Tuning / 440.0f)) * Mathf.Tau)
                        * (state.Event.Velocity / 255.0f);
        }
    }
}

            //return Vector2.One * (((clock.Time * (state.Event.Note.Frequency / (Math.PI * 7e3))) % 2) < 0.5 ? 1 : -1) * (state.Event.Velocity / 255.0f);
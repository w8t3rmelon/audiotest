using System;
using System.Collections.Generic;

using audiotest.Core.AudioEngine;

namespace audiotest.Core.Sequencing
{
#nullable enable
    public class Pattern
    {
        public Instrument? Instrument;
        public List<PatternNote> Notes;

        public double MaxBars = 4.0;

        public Pattern(Instrument? inst = null)
        {
            Instrument = inst;
            Notes = new();
        }

        public void SendNotesToInstrument()
        {
            if (Instrument is null) throw new InvalidOperationException("instrument is null");

            Instrument.EventQueue.Clear();
            Dictionary<byte, ushort> NoteCount = new();
            foreach (PatternNote note in Notes)
            {
                uint onPos = UI.MainController.Instance.Mixer.Clock.TimeFromBeat(note.Position, 0);
                uint offPos = UI.MainController.Instance.Mixer.Clock.TimeFromBeat(note.Position + note.Duration, 0);
                while (Instrument.EventQueue.ContainsKey(onPos)) onPos++;
                while (Instrument.EventQueue.ContainsKey(offPos)) offPos++;
                Instrument.ScheduleNote(onPos, new NoteEvent(note.Note, note.Velocity, true));
                Instrument.ScheduleNote(offPos, new NoteEvent(note.Note, note.Velocity, false));
            }
        }
    }
#nullable restore
}

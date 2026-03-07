using System;
using System.Collections.Generic;
using System.Linq;
using audiotest.Core.AudioEngine;

namespace audiotest.Core.Sequencing
{
#nullable enable
    public class Pattern
    {
        public Instrument? Instrument;
        public List<PatternNote> Notes;
        public List<PatternEvent> Events;

        public double MaxBars = 4.0;

        public Pattern(Instrument? inst = null)
        {
            Instrument = inst;
            Notes = new();
            Events = new();
        }

        public void ReorderEvents()
        {
            Events = Events.OrderBy(e => e.Time).ToList();
        }

        [Obsolete("Pattern playback will soon be moved to the Conductor rendering this function useless in the near future")]
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

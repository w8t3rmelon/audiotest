using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using audiotest.Core.Sequencing;
using audiotest.Core.Sequencing.EventData;

namespace audiotest.Core.AudioEngine
{
    public struct NoteState
    {
        public NoteEvent Event;
        public uint StartTime;
        public uint InternalStartTime;
        public double? ReleaseTime;
        public bool ToBeDestroyed;
    }
    public abstract class Instrument : ISampleProvider
    {
        public string Name { get; set; } = "Base Instrument";
        public ADSR Envelope { get; set; } = new ADSR(0.0, 0.0, 1.0, 0.0);

        public float Volume { get; set; } = 1.0f;
        public float Tuning { get; set; } = 440.0f;

        public Dictionary<string, InstrumentParameter> Params { get; init; }

        public event Action ParamsPanelRefreshTriggered;

        public Dictionary<byte, NoteState> Channels = new Dictionary<byte, NoteState>();
        
        /*
         * TODO: part of legacy EventQueue-based sequencing; remove as part
         * of transition to the new Conductor-based system
         */
        public Dictionary<uint, NoteEvent> EventQueue = new Dictionary<uint, NoteEvent>();
        
        private bool _disposed;

        public List<Pattern> Patterns = new();
        public List<uint> PatternSequence = new();

        public abstract Vector2 GetNoteSample(Clock clock, ref NoteState state);

        public Instrument()
        {
            Params = new();
        }

        public void TriggerParamsPanelRefresh()
        {
            ParamsPanelRefreshTriggered?.Invoke();
        }

        /*
         * TODO: part of legacy EventQueue-based sequencing; remove as part
         * of transition to the new Conductor-based system
         */
        public void ScheduleNote(uint Time, NoteEvent e)
        {
            EventQueue[Time] = e;
        }
        
        public double AmplitudeAt(double time, NoteState state)
        {
            if (state.Event.Pressed)
            {
                if (time <= Envelope.Attack)
                    return (time / Envelope.Attack);
                else if (time < (Envelope.Attack + Envelope.Decay))
                    return 1.0 + ((time - Envelope.Attack) / Envelope.Decay) * (Envelope.Sustain - 1.0);
                else return Envelope.Sustain;
            }
            else
            {
                if ((time - (double)state.ReleaseTime!) < Envelope.Release)
                    return Envelope.Sustain - ((time - (double)state.ReleaseTime) / Envelope.Release);
                else return 0.0;
            }
        }

        public void SendSeqEvent(Clock clock, SeqEvent e)
        {
            switch (e.Type)
            {
                case SeqEventType.NoteOn:
                    if (true) {
                        NoteData data = e.GetNoteData();
                        Channels[data.Note] = new NoteState
                        {
                            Event = new NoteEvent { Note = new Note(data.Note), Velocity = data.Velocity, Pressed = true },
                            StartTime = clock.Time,
                            InternalStartTime = clock.Time
                        };
                    }
                    break;
                case SeqEventType.NoteOff:
                    // i can't define a variable with the same name in different cases???
                    if (true)
                    {
                        NoteData data = e.GetNoteData();
                        uint? oldStartTime = null;
                        uint? oldInternalStartTime = null;
                        if (Channels.TryGetValue(data.Note, out var channel)) oldStartTime = channel.StartTime;
                        if (Channels.TryGetValue(data.Note, out var channel1)) oldInternalStartTime = channel1.InternalStartTime;
                        Channels[data.Note] = new NoteState
                        {
                            Event = new NoteEvent { Note = new Note(data.Note), Velocity = data.Velocity, Pressed = false },
                            StartTime = oldStartTime ?? clock.Time,
                            InternalStartTime = oldInternalStartTime ?? clock.Time
                        };
                    }
                    break;
                default:
                    Debug.WriteLine($"unsupported event {e.Type}");
                    break;
            }
        }

        /*
         * TODO: part of legacy EventQueue-based sequencing; remove as part
         * of transition to the new Conductor-based system
         */
        public void SendEventNow(Clock clock, NoteEvent e)
        {
            if (!Channels.ContainsKey(e.Note.Number) && !e.Pressed) { }
            else
                Channels[e.Note.Number] = new NoteState
                {
                    Event = e,
                    StartTime = clock.Time,
                    InternalStartTime = clock.Time
                };
        }

        public Vector2 GetSample(Clock clock)
        {
            /*
             * TODO: part of legacy EventQueue-based sequencing; remove as part
             * of transition to the new Conductor-based system
             */
            if (EventQueue.ContainsKey(clock.Time))
            {
                NoteEvent e = EventQueue[clock.Time];
                SendEventNow(clock, e);
                EventQueue.Remove(clock.Time);
            }

            Vector2 sample = Vector2.Zero;
            for (int i = 0; i < Channels.Count; i++)
            {
                var pair = Channels.ElementAt(i);
                NoteState s = pair.Value;
                double beat = (((double)(clock.Time - s.StartTime) / clock.SampleRate) / (60 / clock.BPM));
                if (s.ReleaseTime == null && !s.Event.Pressed)
                    s.ReleaseTime = beat;
                double amplitude = AmplitudeAt(beat, s);
                sample += GetNoteSample(clock, ref s) * (float)amplitude * Volume;
                if (amplitude <= 0.0 && s.ReleaseTime != null) s.ToBeDestroyed = true;
                Channels[pair.Key] = s;
            }
            
            for (int i = 0; i < Channels.Count; i++)
            {
                KeyValuePair<byte, NoteState> pair = Channels.ElementAt(i);
                if (pair.Value.ToBeDestroyed) Channels.Remove(pair.Key);
            }

            return sample;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // ...
                }

                Channels = null;
                EventQueue = null;


                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

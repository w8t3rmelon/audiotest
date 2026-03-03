namespace audiotest.Core.AudioEngine
{
    public class Clock
    {
        public uint SampleRate { get; set; }
        public TimeSignature TimeSignature { get; set; }
        public double BPM { get; set; }
        public bool Running { get; set; }
        public uint Time { get; set; }

        public double TimeSeconds
        {
            get => (double)Time / SampleRate;
        }

        public double Bar
        {
            get => (TimeSeconds / (60 / BPM)) / TimeSignature.NotesPerBar;
        }

        public double Beat
        {
            get => (TimeSeconds / (60 / BPM)) % TimeSignature.NotesPerBar;
        }

        public Clock(uint sampleRate)
        {
            TimeSignature = new TimeSignature(4, 4);
            SampleRate = sampleRate;
            BPM = 120;
            Running = false;
            Time = 0;
        }
        
        public void Reset()
        {
            Running = false;
            Time = 0;
        }

        public void Advance(uint delta)
        {
            if (Running)
                Time += delta;
        }

        public uint TimeFromBeat(double bar, double beat)
        {
            uint beats = (uint)((beat * (60.0 / BPM)) * (double)SampleRate);
            uint bars = (uint)((bar * (60.0 / BPM)) * TimeSignature.NotesPerBar * (double)SampleRate);
            return bars + beats;
        }

        public Clock Clone()
        {
            Clock instance = new Clock(SampleRate);
            instance.BPM = BPM;
            instance.TimeSignature = TimeSignature;
            return instance;
        }
    }
}

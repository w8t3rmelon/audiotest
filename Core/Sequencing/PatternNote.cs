namespace audiotest.Core.Sequencing
{
    public class PatternNote
    {
        public Note Note { get; set; }
        public byte Velocity { get; set; }
        public double Position { get; set; }
        public double Duration { get; set; }

        public bool Selected { get; set; }
    }
}

namespace audiotest.Core.Sequencing
{
    public struct NoteEvent(Note note, byte velocity, bool pressed)
    {
        public Note Note { get; set; } = note;
        public byte Velocity { get; set; } = velocity;
        public bool Pressed { get; set; } = pressed;

        public override string ToString()
        {
            return $"{Note} {(Pressed ? "On" : "Off")} @ Vel {Velocity}";
        }
    }
}

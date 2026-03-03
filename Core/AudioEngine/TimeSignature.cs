
namespace audiotest.Core.AudioEngine
{
    public struct TimeSignature(uint notesPerBar, uint divisor)
    {
        public uint NotesPerBar { get; set; } = notesPerBar;
        public uint Divisor { get; set; } = divisor;
    }
}

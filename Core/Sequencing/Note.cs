// most information taken from this stinky website https://inspiredacoustics.com/en/MIDI_note_numbers_and_center_frequencies
// update 26 feb 2026: eww they have ai now ????

using System;

namespace audiotest.Core.Sequencing
{
    public enum NoteLetter : byte
    {
        C = 0,
        CSharp = 1,
        D = 2,
        DSharp = 3,
        E = 4,
        F = 5,
        FSharp = 6,
        G = 7,
        GSharp = 8,
        A = 9,
        ASharp = 10,
        B = 11
    }
    public struct Note
    {
        public static readonly string[] NoteNames = ["C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"];
        public static readonly string[] FlatNoteNames = ["C", "Db", "D", "Eb", "E", "F", "Gb", "G", "Ab", "A", "Bb", "B"];

        /// <summary>
        /// The MIDI note number. Used as the basis for calculating
        /// other values in this struct like <see cref="Frequency"/>.
        /// </summary>
        public byte Number { get; set; }
        public sbyte Octave {
            get => (sbyte)(Math.Floor((double)(Number / 12)) - 1);
            set {
                // I LOVE TYPECASTING!!!!!
                Number = (byte)(12 * ((byte)Math.Clamp(value, (sbyte)-1, (sbyte)9) + 1));
            }
        }
        public NoteLetter Letter {
            get => (NoteLetter)((Number % 12));
            set {
                Number = (byte)(((Octave + 1) * 12) + (byte)value);
            }
        }
        public double Frequency
        {
            get => 440.0 * Math.Pow(2.0, ((double)Number - 69.0) / 12.0);
        }

        /// <summary>
        /// The label of the note, using the sharp (#) symbol for accidentals.
        /// For example, a note number of 73 would produce "C#5"
        /// </summary>
        public string Name
        {
            get => $"{NoteNames[(int)Letter]}{Octave}";
        }

        /// <summary>
        /// The label of the note, using the flat (b) symbol for accidentals.
        /// For example, a note number of 73 would produce "Db5"
        /// </summary>
        public string NameFlat
        {
            get => $"{FlatNoteNames[(int)Letter]}{Octave}";
        }

        public bool Accidental
        {
            get => Number % 2 == ((Number % 12) >= 5 ? 0 : 1);
        }

        public Note(byte number)
        {
            Number = number;
        }

        public Note(sbyte octave, NoteLetter letter)
        {
            Octave = octave;
            Letter = letter;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

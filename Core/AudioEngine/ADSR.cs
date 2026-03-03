namespace audiotest.Core.AudioEngine
{
    public struct ADSR(double attack = 0.0, double decay = 0.0, double sustain = 1.0, double release = 0.0)
    {
        /// <summary>
        /// Time in beats to reach maximum amplitude
        /// </summary>
        public double Attack { get; set; } = attack;
        /// <summary>
        /// Time in beats to reach sustain amplitude
        /// </summary>
        public double Decay { get; set; } = decay;
        /// <summary>
        /// Target amplitude after delay is finished
        /// </summary>
        public double Sustain { get; set; } = sustain;
        /// <summary>
        /// Time in beats to reach zero amplitude after the note is released
        /// </summary>
        public double Release { get; set; } = release;
    }
}

namespace audiotest.Core
{
    public class Config
    {
        public float AudioBufferSize { get; set; }
        public uint SampleRate { get; set; }
        public uint APThreadUpdateRate { get; set; }

        public Config()
        {
            AudioBufferSize = 0.033f;
            SampleRate = 44100;
            APThreadUpdateRate = 35;
        }
    }
}

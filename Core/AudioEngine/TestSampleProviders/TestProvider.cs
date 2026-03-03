using Godot;
using System;

using audiotest.Core.Sequencing;

namespace audiotest.Core.AudioEngine.TestSampleProviders
{
    public class TestProvider(Note note) : ISampleProvider
    {
        private bool _disposed;

        public double Phase = 0.0;
        public Vector2 GetSample(Clock clock)
        {
            Phase = Mathf.PosMod(Phase + ((note.Frequency) / (double)clock.SampleRate), 1.0);
            return Vector2.One * 0.25f * (float)Mathf.Sin(Phase * Mathf.Tau);
        }

        protected virtual void Dispose(bool disposing)
        {
            // nothing to dispose here, really..
            if (!_disposed)
            {
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

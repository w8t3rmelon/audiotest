using Godot;
using System;

namespace audiotest.Core.AudioEngine.TestSampleProviders
{
    public class SampleS16TestProvider : ISampleProvider
    {
        private AudioStreamWav _stream;
        private byte[] _sampleData;
        private bool _disposed;

        public SampleS16TestProvider(string path)
        {
            _stream = GD.Load<AudioStreamWav>(path);
            _sampleData = _stream.Data;
        }
        public Vector2 GetSample(Clock clock)
        {
            if (clock.Time < _sampleData.Length / 4)
            {
                uint pos = clock.Time * 4;
                float l = (BitConverter.ToInt16([_sampleData[pos], _sampleData[pos + 1]]) / 16384.0f);
                float r = (BitConverter.ToInt16([_sampleData[pos + 2], _sampleData[pos + 3]]) / 16384.0f);
                return new Vector2(l, r);
            } else
            {
                return Vector2.Zero;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _stream.Dispose();
                }

                _stream = null;
                _sampleData = null;

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

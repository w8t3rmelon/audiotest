using Godot;
using System;

namespace audiotest.Core.AudioEngine.TestSampleProviders
{
	public class BytebeatTestProvider : ISampleProvider
	{
		private bool _disposed;

		public Vector2 GetSample(Clock clock)
		{
			int t = (int)(clock.Time / (clock.SampleRate / 8000f));
			sbyte r = (sbyte)(t * (42 & t >> 10));
			return Vector2.One * (r / 256.0f);
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

using Godot;
using System;

namespace audiotest.Core.AudioEngine
{
    public interface ISampleProvider : IDisposable
    {
        public Vector2 GetSample(Clock clock);
    }
}

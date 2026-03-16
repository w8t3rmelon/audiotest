using Godot;
using System;
using System.Linq;

using audiotest.Core.AudioEngine;

namespace audiotest.UI
{
    public partial class DebugDraw : Control
    {
        private double _lastDelta;
        private int _size = 18;
        private int _adv = 0;
        public override void _Process(double delta)
        {
            _lastDelta = delta;
            QueueRedraw();
        }

        private void DrawText(string s)
        {
            DrawStringOutline(MainController.Instance.DebugFont, new Vector2(0, _adv), s, size: _size / 2, fontSize: _size, modulate: new Color(0, 0, 0, 1f));
            DrawString(MainController.Instance.DebugFont, new Vector2(0, _adv), s, fontSize: _size, modulate: new Color(1, 1, 1, 1f));
            _adv += _size;
        }

        public override void _Draw()
        {
            if (MainController.Instance.DebugMode)
            {
                _adv = 96;
                DrawText($"--Config--");
                DrawText($"Sample Rate: {MainController.Instance.Config.SampleRate}Hz");
                DrawText($"Buffer size: {Math.Floor(MainController.Instance.Config.AudioBufferSize * 1000)}ms");
                DrawText($"Audio processing thread update rate: {MainController.Instance.Config.APThreadUpdateRate}Hz");
                DrawText($"--Mixer--");
                DrawText($"Clock: {MainController.Instance.Mixer.Clock.Time} (Bar {Math.Floor(MainController.Instance.Mixer.Clock.Bar)} Beat {MainController.Instance.Mixer.Clock.Beat:0.00})");
                DrawText($"PreviewClock: {MainController.Instance.Mixer.PreviewClock.Time} ({(MainController.Instance.Mixer.PreviewClock.Running ? "Active" : "Idle")})");
                DrawText($"Running: {MainController.Instance.Mixer.Clock.Running}");
                DrawText($"Providers: {MainController.Instance.Mixer.SampleProviders.Count}");
                for (int i = 0; i < MainController.Instance.Mixer.SampleProviders.Count; i++)
                {
                    ISampleProvider provider = MainController.Instance.Mixer.SampleProviders[i];
                    if (provider is Instrument inst)
                    {
                        DrawText($"[{i}:Instrument] {inst.Name} ({provider.GetType().Name}) ({inst.Channels.Count} ch)");
                        DrawText($"-- Envelope: {inst.Envelope.Attack}, {inst.Envelope.Decay}, {inst.Envelope.Sustain}, {inst.Envelope.Release}");
                        DrawText($"-- Vol: {inst.Volume:0.00}, Tuning: A4 = {inst.Tuning:0.0}Hz");
                        DrawText($"-- Patterns: {inst.Patterns.Count}");
                        DrawText($"-- Pattern sequence: {string.Join(',', inst.PatternSequence)}");
                    }
                    else
                    {
                        DrawText($"[{i}:Provider] {provider.GetType().Name}");
                    }
                }
                DrawText($"--Performance--");
                DrawText($"Heap size: {GC.GetGCMemoryInfo().HeapSizeBytes / 1024 / 1024}MB");
                DrawText($"lastDelta: {_lastDelta * 1000}ms");
            }
        }
    }
}

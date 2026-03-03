using Godot;
using System;

using audiotest.Core.AudioEngine;

namespace audiotest.UI
{
	public partial class ADSREditor : Control
	{
		[Export] public Line2D Graph;

		[Export] public Slider Attack;
		[Export] public Slider Decay;
		[Export] public Slider Sustain;
		[Export] public Slider Release;

		public ADSR Envelope;

		public event Action<ADSR> Updated;

		public override void _Ready()
		{
			Envelope = new ADSR();
		}

		public override void _Process(double delta)
		{
			Graph.Points = [
				new Vector2(0, 192),
				new Vector2((float)(Size.X * 0.0625 * Attack.Value), 0),
				new Vector2((float)((Size.X * 0.0625 * Attack.Value) + (Size.X * 0.0625 * Decay.Value)), (float)(192 - (Sustain.Value * 192))),
				new Vector2((float)((Size.X * 0.0625 * Attack.Value) + (Size.X * 0.0625 * Decay.Value) + (Size.X * 0.0625 * Release.Value)), 192),
			];
			Envelope.Attack = Attack.Value;
			Envelope.Decay = Decay.Value;
			Envelope.Sustain = Sustain.Value;
			Envelope.Release = Release.Value;
			Updated(Envelope);
		}
	}
}

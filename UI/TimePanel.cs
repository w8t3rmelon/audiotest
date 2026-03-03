using Godot;
using System;

namespace audiotest.UI;

public partial class TimePanel : Control
{
	private Label _timeLabel;
	public override void _Ready()
	{
		_timeLabel = GetNode<Label>("Label");
	}

	public override void _Process(double delta)
	{
		double time = MainController.Instance.Mixer.Clock.TimeSeconds;
		_timeLabel.Text = $"{Math.Floor(time / 60):00}:{Math.Floor(time % 60):00}.{Math.Floor((time % 1) * 100):00}";
	}
}

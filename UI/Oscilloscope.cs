using Godot;
using System.Linq;

namespace audiotest.UI
{
	public partial class Oscilloscope : Panel
	{
		public Line2D LeftChannel;
		public Line2D RightChannel;

		public Timer Timer;

		public override void _Ready()
		{
			LeftChannel = GetNode<Line2D>("Left");
			RightChannel = GetNode<Line2D>("Right");

			Timer = GetNode<Timer>("Timer");

			Timer.Start();
			Timer.Timeout += Update;
		}

		public void Update()
		{
			// weird things happen when you access a collection modified by another thread, so we acquire the mutex from the mixer's processing thread
			audiotest.UI.MainController.Instance.Mixer.ProcessingThreadMutex.WaitOne();
			LeftChannel.Points = audiotest.UI.MainController.Instance.Mixer.SampleHistory.Select(
				(s, i) => new Vector2(((float)i / (float)audiotest.UI.MainController.Instance.Mixer.SampleHistory.Count) * Size.X, (Size.Y / 2) + (s.X * Size.Y / 2))
			).ToArray();
			RightChannel.Points = audiotest.UI.MainController.Instance.Mixer.SampleHistory.Select(
				(s, i) => new Vector2(((float)i / (float)audiotest.UI.MainController.Instance.Mixer.SampleHistory.Count) * Size.X, (Size.Y / 2) + (s.Y * Size.Y / 2))
			).ToArray();
			audiotest.UI.MainController.Instance.Mixer.ProcessingThreadMutex.ReleaseMutex();
		}

		public override void _Process(double delta)
		{
			Timer.WaitTime = audiotest.UI.MainController.Instance.Config.AudioBufferSize;
		}
	}
}
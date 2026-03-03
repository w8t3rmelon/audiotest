using Godot;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Mutex = System.Threading.Mutex;

namespace audiotest.Core.AudioEngine
{
	[GlobalClass]
	public partial class Mixer : AudioStreamPlayer
	{
		private AudioStreamGeneratorPlayback _streamPlayback;

		public List<ISampleProvider> SampleProviders;
		public List<Vector2> SampleHistory;

		public const int HistoryResolution = 512;

		public float Volume = 1.0f;

		public Clock Clock;
		public Clock PreviewClock;

		public Thread ProcessingThread;
		public ManualResetEventSlim ProcessingThreadCancel;
		public Mutex ProcessingThreadMutex;

		private DateTime LastProcess;

		public void Reset()
		{
			Stop();
			Clock.Reset();
			Clock.SampleRate = UI.MainController.Instance.Config.SampleRate;

			PreviewClock = Clock.Clone();

			SampleProviders = new();
			SampleHistory = new(HistoryResolution);
			AudioStreamGenerator stream = new AudioStreamGenerator();
			stream.MixRate = UI.MainController.Instance.Config.SampleRate;
			stream.BufferLength = UI.MainController.Instance.Config.AudioBufferSize;
			Stream = stream;

			Play();
			_streamPlayback = (AudioStreamGeneratorPlayback)GetStreamPlayback();
		}

		public async Task AudioProcess()
		{
			while (ProcessingThreadCancel.IsSet)
			{
				int framesAvailable = _streamPlayback.GetFramesAvailable();

				if (framesAvailable > 0)
				{
					ProcessingThreadMutex.WaitOne();

					Span<Vector2> buf = new Vector2[framesAvailable];
					Clock? activeClock =
						Clock.Running ? Clock :
							PreviewClock.Running ? PreviewClock : null;
					for (int i = 0; i < framesAvailable; i++)
					{
						Vector2 sample = Vector2.Zero;
						if (activeClock is not null)
						{
							foreach (ISampleProvider provider in SampleProviders)
							{
								Vector2 psample = provider.GetSample(activeClock);
								sample += psample;
                            }
                            activeClock.Advance(1);
                        }
						buf[i] = sample;
						if (SampleHistory.Count >= HistoryResolution) 
							SampleHistory.RemoveRange(0, Math.Max(SampleHistory.Count - HistoryResolution, 1));
						else SampleHistory.Add(sample);
					}
					_streamPlayback.PushBuffer(buf);
					buf = null;

					ProcessingThreadMutex.ReleaseMutex();
				}

				await Task.Delay((int)((1.0 / UI.MainController.Instance.Config.APThreadUpdateRate) * 1000));
			}

            ProcessingThreadCancel.Set();
        }

		public bool TryOpenPreviewClock()
		{
			if (Clock.Running)
				return false;
			else
			{
				PreviewClock.Reset();
				PreviewClock.Running = true;
				return true;
			}
		}

		public void ClosePreviewClock()
        {
            PreviewClock.Reset();
        }

        public override void _Ready()
		{
			Clock = new Clock(UI.MainController.Instance.Config.SampleRate);
			Reset();

			//SampleProviders.Add(new TestProvider(new Note { Octave = 4, Letter = NoteLetter.C }));
			//SampleProviders.Add(new TestProvider(new Note { Octave = 5, Letter = NoteLetter.C }));
			//SampleProviders.Add(new TestProvider(new Note { Octave = 6, Letter = NoteLetter.C }));
			//SampleProviders.Add(new SampleS16TestProvider("res://test.wav"));
			//SampleProviders.Add(new BytebeatTestProvider());

			/*
			TestInstrument testInstrument = new TestInstrument();
			testInstrument.Envelope = new ADSR(0.0, 0.0, 1, 0.80);

			testInstrument.ScheduleNote(Clock.TimeFromBeat(0, 0), new NoteEvent(new Note { Octave = 5, Letter = NoteLetter.D }, 64, true));
			testInstrument.ScheduleNote(Clock.TimeFromBeat(0, 0.5), new NoteEvent(new Note { Octave = 5, Letter = NoteLetter.D }, 64, false));

			testInstrument.ScheduleNote(Clock.TimeFromBeat(0, 1.5), new NoteEvent(new Note { Octave = 4, Letter = NoteLetter.B }, 64, true));
			testInstrument.ScheduleNote(Clock.TimeFromBeat(0, 2), new NoteEvent(new Note { Octave = 4, Letter = NoteLetter.B }, 64, false));

			SampleProviders.Add(testInstrument);
			*/

			/*
			SamplerInstrument meow = new SamplerInstrument("res://FUCK.wav");
			meow.Name = "FUCK!";
			meow.ScheduleNote(0, new NoteEvent(new Note { Octave = 4, Letter = NoteLetter.A }, 255, true));
			for (int i = 0; i < 64; i++)
			{
				meow.ScheduleNote(Clock.TimeFromBeat(0, i), new NoteEvent(new Note { Number = (byte)(i + 64) }, 255, true));
			}
			SampleProviders.Add(meow);
			*/

			LastProcess = DateTime.Now;

			ProcessingThreadCancel = new ManualResetEventSlim(true);
			ProcessingThread = new Thread(() => AudioProcess());
			ProcessingThreadMutex = new Mutex();

			ProcessingThread.Start();
		}
	}
}

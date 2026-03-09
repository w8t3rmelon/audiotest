using Godot;
using System;
using System.IO;

using audiotest.Core.AudioEngine;

namespace audiotest.Core.Instruments
{
	/// <summary>
	/// Sampler instrument for 8 and 16-bit PCM WAV files.
	/// </summary>
	public class SamplerInstrument : Instrument
	{
		public AudioStreamWav Stream;
		public byte[] SampleData;
		public int SampleLength;
		
		public Func<uint, Vector2> ProcessingFunction;

		private bool _disposed;

		public bool Ready;

		public Vector2 Read8BitMonoSample(uint index)
		{
			return Vector2.One * ((sbyte)SampleData[index] / 128.0f);
		}
		public Vector2 Read8BitStereoSample(uint index)
		{
			uint pos = index * 2;
			return new Vector2(
				((sbyte)SampleData[pos] / 128.0f),
				((sbyte)SampleData[pos + 1] / 128.0f)
			);
		}
		public Vector2 Read16BitMonoSample(uint index)
		{
			uint pos = index * 2;
			return Vector2.One * (BitConverter.ToInt16([SampleData[pos], SampleData[pos + 1]]) / 16384.0f);
		}
		public Vector2 Read16BitStereoSample(uint index)
		{
			uint pos = index * 4;
			return new Vector2(
				(BitConverter.ToInt16([SampleData[pos], SampleData[pos + 1]]) / 16384.0f),
				(BitConverter.ToInt16([SampleData[pos + 2], SampleData[pos + 3]]) / 16384.0f)
			);
		}

		public SamplerInstrument()
		{
			Name = "Sampler";
			Params = new() {
				{ "path", new InstrumentParameter(InstrumentParameterType.String, "Sample Path", "") },
				{ "load", new InstrumentParameter(InstrumentParameterType.Invoke, "Load", LoadSampleFromParam) },

                { "loop", new InstrumentParameter(InstrumentParameterType.Toggle, "Loop", false) },
                { "lppt", new InstrumentParameter(InstrumentParameterType.Toggle, "Loop Points", false, true)},
                { "lpst", new InstrumentParameter(InstrumentParameterType.Slider, "Loop Start", 0, 0, 0, 1, true)},
                { "lpen", new InstrumentParameter(InstrumentParameterType.Slider, "Loop End", 0, 0, 0, 1, true)},
            };

			Params["loop"].BoolChanged += enabled =>
			{
				Params["lppt"].Hidden = !enabled;
				if (Params["lppt"].BoolValue)
				{
					Params["lpst"].Hidden = !enabled;
					Params["lpen"].Hidden = !enabled;
				}
				TriggerParamsPanelRefresh();
			};

			Params["lppt"].BoolChanged += enabled =>
			{
				Params["lpst"].Hidden = !enabled;
				Params["lpen"].Hidden = !enabled;
				TriggerParamsPanelRefresh();
			};
		}

		public SamplerInstrument(string path) : this()
		{
			Stream = GD.Load<AudioStreamWav>(path);
			SampleData = Stream.Data;

			ProcessingFunction = Stream.Format switch
			{
				AudioStreamWav.FormatEnum.Format8Bits => Stream.Stereo ? Read8BitStereoSample : Read8BitMonoSample,
				AudioStreamWav.FormatEnum.Format16Bits => Stream.Stereo ? Read16BitStereoSample : Read16BitMonoSample,
				_ => throw new InvalidOperationException($"Stream format {Stream.Format} is unsupported..")
			};

			SampleLength = Stream.Format switch
			{
				AudioStreamWav.FormatEnum.Format8Bits => Stream.Stereo ? SampleData.Length / 2 : SampleData.Length,
				AudioStreamWav.FormatEnum.Format16Bits => Stream.Stereo ? SampleData.Length / 4 : SampleData.Length / 2,
				_ => 0
			};

			Ready = true;
		}

		public void LoadSampleFromParam()
		{
			Ready = false;

			try
			{
                FileStream file = File.Open(Params["path"].StringValue, FileMode.Open);
                byte[] data = new byte[file.Length];
                file.Read(data, 0, (int)file.Length);

                Stream = AudioStreamWav.LoadFromBuffer(data);
                SampleData = Stream.Data;

                ProcessingFunction = Stream.Format switch
                {
                    AudioStreamWav.FormatEnum.Format8Bits => Stream.Stereo ? Read8BitStereoSample : Read8BitMonoSample,
                    AudioStreamWav.FormatEnum.Format16Bits => Stream.Stereo ? Read16BitStereoSample : Read16BitMonoSample,
                    _ => throw new InvalidOperationException($"Stream format {Stream.Format} is unsupported..")
                };

                SampleLength = Stream.Format switch
                {
                    AudioStreamWav.FormatEnum.Format8Bits => Stream.Stereo ? SampleData.Length / 2 : SampleData.Length,
                    AudioStreamWav.FormatEnum.Format16Bits => Stream.Stereo ? SampleData.Length / 4 : SampleData.Length / 2,
                    _ => 0
                };

                Params["lpst"].DoubleValue = 0;
                Params["lpst"].DoubleMax = SampleLength;
                
                Params["lpen"].DoubleValue = SampleLength;
                Params["lpen"].DoubleMax = SampleLength;

				OS.Alert("sample loaded successfully!", "sampler");

				Ready = true;
            } catch (Exception e)
            {
                OS.Alert($"error loading sample:\n{e}", "sampler");
            }
		}

		public override Vector2 GetNoteSample(Clock clock, ref NoteState state)
		{
			if (!Ready)
			{
				state.ToBeDestroyed = true;
				return Vector2.Zero;
			}
			
			double rate = (((state.Event.Note.Frequency * Stream.MixRate) / Tuning) / Stream.MixRate) * ((double)Stream.MixRate / (double)clock.SampleRate);
			
			uint time = clock.Time - state.InternalStartTime;
			
			if (Params["loop"].BoolValue)
			{
				if (Params["lppt"].BoolValue)
				{
					if (time > Params["lpen"].DoubleValue / rate)
						state.InternalStartTime = clock.Time - (uint)(Params["lpst"].DoubleValue / rate);
				}
				else if (time >= SampleLength / rate)
				{
					state.InternalStartTime = clock.Time;
				}
			}

			time = clock.Time - state.InternalStartTime;

			if (time < SampleLength / rate)
			{
				return ProcessingFunction((uint)(time * rate)) * (state.Event.Velocity / 255f);
			}
			else
			{
				state.ToBeDestroyed = true;
				return Vector2.Zero;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					Stream.Dispose();
				}

				SampleLength = 0;
				SampleData = null;
				ProcessingFunction = null;

				_disposed = true;
			}
		}
	}
}

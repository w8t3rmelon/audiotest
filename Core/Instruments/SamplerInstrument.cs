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
		private AudioStreamWav _stream;
		private byte[] _data;

		private Func<uint, Vector2> _processingFunction;
		private int _length;

		private bool _disposed;

		public bool Ready;

		public Vector2 Read8BitMonoSample(uint index)
		{
			return Vector2.One * ((sbyte)_data[index] / 128.0f);
		}
		public Vector2 Read8BitStereoSample(uint index)
		{
			uint pos = index * 2;
			return new Vector2(
				((sbyte)_data[pos] / 128.0f),
				((sbyte)_data[pos + 1] / 128.0f)
			);
		}
		public Vector2 Read16BitMonoSample(uint index)
		{
			uint pos = index * 2;
			return Vector2.One * (BitConverter.ToInt16([_data[pos], _data[pos + 1]]) / 16384.0f);
		}
		public Vector2 Read16BitStereoSample(uint index)
		{
			uint pos = index * 4;
			return new Vector2(
				(BitConverter.ToInt16([_data[pos], _data[pos + 1]]) / 16384.0f),
				(BitConverter.ToInt16([_data[pos + 2], _data[pos + 3]]) / 16384.0f)
			);
		}

		public SamplerInstrument()
		{
			Name = "Sampler";
			Params = new() {
				{ "path", new InstrumentParameter(InstrumentParameterType.String, "Sample Path", "") },
				{ "load", new InstrumentParameter(InstrumentParameterType.Invoke, "Load", LoadSampleFromParam) },

                { "loop", new InstrumentParameter(InstrumentParameterType.Toggle, "Loop", false) },
            };
		}

		public SamplerInstrument(string path) : this()
		{
			_stream = GD.Load<AudioStreamWav>(path);
			_data = _stream.Data;

			_processingFunction = _stream.Format switch
			{
				AudioStreamWav.FormatEnum.Format8Bits => _stream.Stereo ? Read8BitStereoSample : Read8BitMonoSample,
				AudioStreamWav.FormatEnum.Format16Bits => _stream.Stereo ? Read16BitStereoSample : Read16BitMonoSample,
				_ => throw new InvalidOperationException($"Stream format {_stream.Format} is unsupported..")
			};

			_length = _stream.Format switch
			{
				AudioStreamWav.FormatEnum.Format8Bits => _stream.Stereo ? _data.Length / 2 : _data.Length,
				AudioStreamWav.FormatEnum.Format16Bits => _stream.Stereo ? _data.Length / 4 : _data.Length / 2,
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

                _stream = AudioStreamWav.LoadFromBuffer(data);
                _data = _stream.Data;

                _processingFunction = _stream.Format switch
                {
                    AudioStreamWav.FormatEnum.Format8Bits => _stream.Stereo ? Read8BitStereoSample : Read8BitMonoSample,
                    AudioStreamWav.FormatEnum.Format16Bits => _stream.Stereo ? Read16BitStereoSample : Read16BitMonoSample,
                    _ => throw new InvalidOperationException($"Stream format {_stream.Format} is unsupported..")
                };

                _length = _stream.Format switch
                {
                    AudioStreamWav.FormatEnum.Format8Bits => _stream.Stereo ? _data.Length / 2 : _data.Length,
                    AudioStreamWav.FormatEnum.Format16Bits => _stream.Stereo ? _data.Length / 4 : _data.Length / 2,
                    _ => 0
                };

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

			uint time = clock.Time - state.StartTime;
			double rate = (((state.Event.Note.Frequency * _stream.MixRate) / Tuning) / _stream.MixRate) * ((double)_stream.MixRate / (double)clock.SampleRate);
			if (time < _length / rate && state.Event.Pressed)
				return _processingFunction((uint)(time * rate)) * (state.Event.Velocity / 255f);
			else if (Params["loop"].BoolValue)
			{
				state.StartTime = clock.Time;
				return Vector2.Zero;
			} else
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
					_stream.Dispose();
				}

				_length = 0;
				_data = null;
				_processingFunction = null;

				_disposed = true;
			}
		}
	}
}

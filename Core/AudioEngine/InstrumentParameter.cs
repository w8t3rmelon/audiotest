using System;

namespace audiotest.Core.AudioEngine
{
	public enum InstrumentParameterType
	{
		Toggle,
		Slider,
		Spin,
		Knob,
		String,
		Invoke,
		FilePath
	}
	public class InstrumentParameter
	{
		public InstrumentParameterType Type;

		public string Name;
		public bool Hidden;

		private void ThrowIfNotDoubleType()
		{
			if (Type != InstrumentParameterType.Knob && Type != InstrumentParameterType.Slider && Type != InstrumentParameterType.Spin)
				throw new InvalidOperationException("Parameter type is not Knob, Slider or Spin");
		}

		private void ThrowIfNotStringType()
		{
			if (Type != InstrumentParameterType.String && Type != InstrumentParameterType.FilePath)
				throw new InvalidOperationException("Parameter type is not String or FilePath");
		}

		private void ThrowIfNotBoolType()
		{
			if (Type != InstrumentParameterType.Toggle)
				throw new InvalidOperationException("Parameter type is not Toggle");
		}
		private void ThrowIfNotActionType()
		{
			if (Type != InstrumentParameterType.Invoke)
				throw new InvalidOperationException("Parameter type is not Invoke");
		}
		

		public event Action<double> DoubleChanged;
		public event Action<string> StringChanged;
		public event Action<bool> BoolChanged;

		public event Action UIRefreshTriggered;

		public void TriggerUIRefresh()
		{
			UIRefreshTriggered?.Invoke();
		}


		public double DoubleValue
		{
			get
			{
				ThrowIfNotDoubleType();
				return field;
			}
			set
			{
				ThrowIfNotDoubleType();
				DoubleChanged?.Invoke(value);
				field = value;
			}
		}

		public double DoubleMin
		{
			get
			{
				ThrowIfNotDoubleType();
				return field;
			}
			set
			{
				ThrowIfNotDoubleType();
				field = value;
			}
		}
		public double DoubleMax
		{
			get
			{
				ThrowIfNotDoubleType();
				return field;
			}
			set
			{
				ThrowIfNotDoubleType();
				field = value;
			}
		}
		public double DoubleStep
		{
			get
			{
				ThrowIfNotDoubleType();
				return field;
			}
			set
			{
				ThrowIfNotDoubleType();
				field = value;
			}
		}
		public double DoubleDefault
		{
			get
			{
				ThrowIfNotDoubleType();
				return field;
			}
			set
			{
				ThrowIfNotDoubleType();
				field = value;
			}
		}


		public string StringValue
		{
			get
			{
				ThrowIfNotStringType();
				return field;
			}
			set
			{
				ThrowIfNotStringType();
				field = value;
				StringChanged?.Invoke(value);
			}
		}
		public string StringDefault
		{
			get
			{
				ThrowIfNotStringType();
				return field;
			}
			set
			{
				ThrowIfNotStringType();
				field = value;
			}
		}


		public bool BoolValue
		{
			get
			{
				ThrowIfNotBoolType();
				return field;
			}
			set
			{
				ThrowIfNotBoolType();
				field = value;
				BoolChanged?.Invoke(value);
			}
		}
		public bool BoolDefault
		{
			get
			{
				ThrowIfNotBoolType();
				return field;
			}
			set
			{
				ThrowIfNotBoolType();
				field = value;
			}
		}

		public Action Action
		{
			get
			{
				ThrowIfNotActionType();
				return field;
			}
			set
			{
				ThrowIfNotActionType();
				field = value;
			}
		}

		public InstrumentParameter(InstrumentParameterType type, string name, double defaultValue, double min, double max, double step, bool hidden = false)
		{
			Type = type;
			ThrowIfNotDoubleType();
			Hidden = hidden;
			Name = name;

			DoubleDefault = defaultValue;
			DoubleMin = min;
			DoubleMax = max;
			DoubleStep = step;

			DoubleValue = defaultValue;
		}

		public InstrumentParameter(InstrumentParameterType type, string name, bool defaultValue, bool hidden = false)
		{
			Type = type;
			ThrowIfNotBoolType();
			Hidden = hidden;
            Name = name;

            BoolDefault = defaultValue;

			BoolValue = defaultValue;
		}

		public InstrumentParameter(InstrumentParameterType type, string name, string defaultValue, bool hidden = false)
		{
			Type = type;
			ThrowIfNotStringType();
			Hidden = hidden;
            Name = name;

            StringDefault = defaultValue;

			StringValue = defaultValue;
		}

		public InstrumentParameter(InstrumentParameterType type, string name, Action action, bool hidden = false)
		{
			Type = type;
			ThrowIfNotActionType();
			Hidden = hidden;
            Name = name;
            Action = action;
		}
	}
}

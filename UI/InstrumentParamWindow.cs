using Godot;
using System.Collections.Generic;

using audiotest.Core.AudioEngine;

namespace audiotest.UI
{
    public partial class InstrumentParamWindow : Control
    {
        public Instrument Instrument;

        private HBoxContainer Knobs;

        public override void _Ready()
        {
            Knobs = new HBoxContainer();
            Knobs.CustomMinimumSize = new Vector2(0, 96);
            AddChild(Knobs);
        }

        private void MakeToggle(InstrumentParameter parm)
        {
            CheckBox checkBox = new CheckBox();

            checkBox.ButtonPressed = parm.BoolValue;
            checkBox.Text = parm.Name;

            checkBox.Toggled += (val) =>
            {
                parm.BoolValue = val;
            };

            AddChild(checkBox);
        }

        private void MakeSlider(InstrumentParameter parm)
        {
            HBoxContainer container = new HBoxContainer();
            HSlider slider = new HSlider();
            Label label = new Label();

            slider.SizeFlagsHorizontal = SizeFlags.Expand;
            slider.CustomMinimumSize = new Vector2(0, 32);

            slider.MinValue = parm.DoubleMin;
            slider.MaxValue = parm.DoubleMax;
            slider.Step = parm.DoubleStep;
            slider.Value = parm.DoubleValue;

            slider.SizeFlagsHorizontal = SizeFlags.ExpandFill;

            label.Text = parm.Name;

            slider.ValueChanged += (val) =>
            {
                parm.DoubleValue = val;
            };

            parm.UIRefreshTriggered += () =>
            {
                label.Text = parm.Name;
                slider.MinValue = parm.DoubleMin;
                slider.MaxValue = parm.DoubleMax;
                slider.Step = parm.DoubleStep;
                slider.Value = parm.DoubleValue;
            };

            container.AddChild(label);
            container.AddChild(slider);
            AddChild(container);
        }

        private void MakeSpin(InstrumentParameter parm)
        {
            HBoxContainer container = new HBoxContainer();
            SpinBox slider = new SpinBox();
            Label label = new Label();

            slider.SizeFlagsHorizontal = SizeFlags.Expand;
            slider.CustomMinimumSize = new Vector2(0, 32);

            slider.MinValue = parm.DoubleMin;
            slider.MaxValue = parm.DoubleMax;
            slider.Step = parm.DoubleStep;
            slider.Value = parm.DoubleValue;

            label.Text = parm.Name;
            label.VerticalAlignment = VerticalAlignment.Center;

            slider.ValueChanged += (val) =>
            {
                parm.DoubleValue = val;
            };

            parm.UIRefreshTriggered += () =>
            {
                label.Text = parm.Name;
                slider.MinValue = parm.DoubleMin;
                slider.MaxValue = parm.DoubleMax;
                slider.Step = parm.DoubleStep;
                slider.Value = parm.DoubleValue;
            };

            container.AddChild(label);
            container.AddChild(slider);
            AddChild(container);
        }

        private void MakeKnob(InstrumentParameter parm)
        {
            Knob knob = new Knob();
            Label label = new Label();

            knob.CustomMinimumSize = new Vector2(64, 64);

            knob.MinValue = parm.DoubleMin;
            knob.MaxValue = parm.DoubleMax;
            knob.Step = parm.DoubleStep;
            knob.Value = parm.DoubleValue;

            label.Text = parm.Name;
            label.VerticalAlignment = VerticalAlignment.Center;
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.SetAnchorsPreset(LayoutPreset.BottomWide, true);
            label.AddThemeFontSizeOverride("font_size", 8);

            knob.ValueChanged += (val) =>
            {
                parm.DoubleValue = val;
            };

            parm.UIRefreshTriggered += () =>
            {
                label.Text = parm.Name;
                knob.MinValue = parm.DoubleMin;
                knob.MaxValue = parm.DoubleMax;
                knob.Step = parm.DoubleStep;
                knob.Value = parm.DoubleValue;
            };

            knob.AddChild(label);
            Knobs.AddChild(knob);
        }

        private void MakeString(InstrumentParameter parm)
        {
            LineEdit lineEdit = new LineEdit();

            lineEdit.PlaceholderText = parm.Name;
            lineEdit.Text = parm.StringDefault;

            lineEdit.TextChanged += (text) => { parm.StringValue = text; };

            parm.UIRefreshTriggered += () =>
            {
                lineEdit.PlaceholderText = parm.Name;
                lineEdit.Text = parm.StringValue;
            };

            AddChild(lineEdit);
        }

        private void MakeInvoke(InstrumentParameter parm)
        {
            Button button = new Button();

            button.Text = parm.Name;
            button.Pressed += parm.Action;

            AddChild(button);
        }

        private void MakeVolumeKnob(Instrument inst)
        {
            Knob knob = new Knob();
            Label label = new Label();

            knob.CustomMinimumSize = new Vector2(64, 64);
            knob.FillColour = Colors.White;

            knob.MinValue = 0.0;
            knob.MaxValue = 100.0;
            knob.Step = 5.0;
            knob.Value = inst.Volume * 100.0;

            label.Text = "Vol";
            label.VerticalAlignment = VerticalAlignment.Center;
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.SetAnchorsAndOffsetsPreset(LayoutPreset.BottomWide, LayoutPresetMode.KeepHeight, 8);
            label.AddThemeFontSizeOverride("font_size", 8);

            knob.ValueChanged += (val) => { inst.Volume = (float)(val / 100.0); };

            knob.AddChild(label);
            Knobs.AddChild(knob);
        }

        private void MakeTuningKnob(Instrument inst)
        {
            Knob knob = new Knob();
            Label label = new Label();

            knob.CustomMinimumSize = new Vector2(64, 64);
            knob.FillColour = Colors.Red;
            knob.FromCenter = true;

            knob.MinValue = 0.0;
            knob.MaxValue = 880.0;
            knob.Step = 2.0;
            knob.Value = inst.Tuning;

            knob.ValueChanged += (val) => { inst.Tuning = (float)(val); };

            label.Text = "Tune";
            label.VerticalAlignment = VerticalAlignment.Center;
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.SetAnchorsAndOffsetsPreset(LayoutPreset.BottomWide, LayoutPresetMode.KeepHeight, 8);
            label.AddThemeFontSizeOverride("font_size", 8);

            knob.AddChild(label);
            Knobs.AddChild(knob);
        }

        public void Refresh()
        {
            Instrument.ParamsPanelRefreshTriggered -= Refresh;
            foreach (var child in GetChildren())
                RemoveChild(child);

            Knobs = new HBoxContainer();
            AddChild(Knobs);

            MakeVolumeKnob(Instrument);
            MakeTuningKnob(Instrument);

            Instrument.ParamsPanelRefreshTriggered += Refresh;

            foreach (KeyValuePair<string, InstrumentParameter> pair in Instrument.Params)
            {
                if (pair.Value.Hidden) continue;
                
                if (pair.Value.Type == InstrumentParameterType.Toggle)
                    MakeToggle(pair.Value);
                else if (pair.Value.Type == InstrumentParameterType.Slider)
                    MakeSlider(pair.Value);
                else if (pair.Value.Type == InstrumentParameterType.Spin)
                    MakeSpin(pair.Value);
                else if (pair.Value.Type == InstrumentParameterType.Knob)
                    MakeKnob(pair.Value);
                else if (pair.Value.Type == InstrumentParameterType.String)
                    MakeString(pair.Value);
                else if (pair.Value.Type == InstrumentParameterType.Invoke)
                    MakeInvoke(pair.Value);
            }
        }
    }
}

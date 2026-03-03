using System;
using Godot;

using audiotest.Core;
using audiotest.Core.AudioEngine;
using audiotest.Core.Instruments;

namespace audiotest.UI;

[GlobalClass]
public partial class MainController : Control
{
	public static MainController Instance { get; private set; }

	public ProjectData ProjectData { get; set; }

	public bool DebugMode { get; set; }
	public Font DebugFont;
	public bool ForceQuit = false;

	// scenes
	public PackedScene InstrumentEntry { get; private set; }
	public PackedScene PatternLine { get; private set; }

	// instances
	public Config Config { get; private set; }
	public Mixer Mixer { get; private set; }
	public Clock GlobalClock { get; private set; }
	public DebugDraw DebugDraw { get; private set; }

	// top bar
	public TimePanel TimePanel { get; private set; }
	public Button PlayButton { get; private set; }
	public Button StopButton { get; private set; }
	public SpinBox BPMInput { get; private set; }
	public Knob VolumeKnob { get; private set; }
	public PopupMenu InstrumentMenu { get; private set; }

	public Container InstrumentPane { get; private set; }
	public Container PatternPane { get; private set; }

	public override void _Ready()
	{
		Instance = this;

		GetTree().AutoAcceptQuit = false;

		Config = new();
		ProjectData = new();

		Mixer = new Mixer();
		CallDeferred("add_child", Mixer);
		GD.Print("Mixer added");

		DebugDraw = new DebugDraw();
		CallDeferred("add_child", DebugDraw);
		GD.Print("DebugDraw added");

		InstrumentEntry = GD.Load<PackedScene>("res://UI/InstrumentEntry.tscn");
		PatternLine = GD.Load<PackedScene>("res://UI/PatternLine.tscn");

		TimePanel = GetNode<TimePanel>("TopBar/Time");

		PlayButton = GetNode<Button>("TopBar/PlayButton");
		StopButton = GetNode<Button>("TopBar/StopButton");
		BPMInput = GetNode<SpinBox>("TopBar/BPMInput");
		VolumeKnob = GetNode<Knob>("TopBar/VolumeKnob");

		InstrumentMenu = GetNode<PopupMenu>("TopBar/MenuBar/Instrument");

		InstrumentPane = GetNode<Container>("ActualMain/InstrumentPane");
		PatternPane = GetNode<Container>("ActualMain/PatternPane");

		DebugFont = GD.Load<FontFile>("res://Assets/Fonts/Hack-Regular.ttf");


		PlayButton.Toggled += (bool toggled) =>
		{
			Mixer.Clock.Running = toggled;

			var man = GetNode<AnimatedSprite2D>("RunningMan");
			man.SpeedScale = (float)((Mixer.Clock.BPM / 10));
			if (toggled) man.Play();
			else man.Stop();
		};
		StopButton.Pressed += () =>
		{
			PlayButton.ButtonPressed = false;
			Mixer.PreviewClock.Reset();
			Mixer.Clock.Reset();
		};

		BPMInput.ValueChanged += (val) =>
		{
			Mixer.Clock.BPM = val;
		};

		VolumeKnob.ValueChanged += (val) =>
		{
			Mixer.VolumeLinear = (float)(val / 100);
		};

		PopupMenu newInstrumentMenu = InstrumentMenu.GetNode<PopupMenu>("New");


		InstrumentMenu.AddSubmenuNodeItem("New", newInstrumentMenu, 0);

		newInstrumentMenu.AddItem("Test Instrument", 0);
		newInstrumentMenu.AddItem("Sampler", 1);
		newInstrumentMenu.AddItem("FUCK", 2);

		newInstrumentMenu.IdPressed += (long id) =>
		{
			((Action)(id switch
			{
				0 => () => {
					Mixer.SampleProviders.Add(new TestInstrument());
					ResetInstrumentAndPatternPanes();
				},
				1 => () => {
					Mixer.SampleProviders.Add(new SamplerInstrument());
					ResetInstrumentAndPatternPanes();
				}
				,
				2 => () => {
					Mixer.SampleProviders.Add(new FuckInstrument());
					ResetInstrumentAndPatternPanes();
				}
				,
				_ => null
			}) ?? (() => { }))();
		};
	}

	public void ResetInstrumentAndPatternPanes()
	{
		foreach (var child in InstrumentPane.GetChildren())
			InstrumentPane.RemoveChild(child);

		foreach (var child in PatternPane.GetChildren())
			PatternPane.RemoveChild(child);

		foreach (ISampleProvider provider in Mixer.SampleProviders)
		{
			if (provider is not Instrument) continue;
			InstrumentEntry entry = InstrumentEntry.Instantiate<InstrumentEntry>();
			PatternLine line = PatternLine.Instantiate<PatternLine>();
			entry.Instrument = (Instrument)provider;
			line.Instrument = (Instrument)provider;
			InstrumentPane.AddChild(entry);
			PatternPane.AddChild(line);
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey)
		{
			InputEventKey e = (InputEventKey)@event;
			if (e.Keycode == Key.Quoteleft && e.Pressed)
			{
				DebugMode = !DebugMode;
			}
		}
	}

	public override void _Notification(int what)
	{
		if (what == NotificationWMCloseRequest)
		{
			if (ForceQuit)
			{
				GetTree().Quit();
				return;
			}

			GD.Print("sending signal to mixer..");
			Mixer.ProcessingThreadCancel.Reset();
			GD.Print("waiting for audio process thread to stop...");
			bool success = Mixer.ProcessingThreadCancel.Wait(15000);
			if (!success)
			{
				ForceQuit = true;
				GD.PushWarning("something HORRIBLE is happening");
				OS.Alert("the mixer didn't respond after 15 seconds. if you want to close anyway (which may result in a crash!), press the close button again.", "mixer thread isn't feeling good today");
			}
			else
			{
				GD.Print("okay we're done. hopefully we don't crash here..");
				GetTree().Quit();
			}
		}
	}
}
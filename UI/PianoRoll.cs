using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

using audiotest.Core.Sequencing;

namespace audiotest.UI;

public enum NoteModificationType
{
	None,
	ResizeL,
	ResizeR,
	Move
}

[Tool]
[GlobalClass]
public partial class PianoRoll : Control
{
	[Export] public float KeySize = 24;
	[Export] public float AccidentalHSize = 0.3f;

	[Export] public float MaxScroll;
	[Export] public float HMaxScroll;

	[Export] public float HSpaceBetweenSteps = 48.0f;

	public double LastLength = 0.5;

	public float ScrollOffset = 0.0f;
	public float HScrollOffset = 0.0f;

	public float HZoomFactor = 1.0f;

	public PianoRollMode PreviousMode = PianoRollMode.Draw;
	public PianoRollMode Mode = PianoRollMode.Draw;

	public VScrollBar VScrollBar;
	public HScrollBar HScrollBar;

	public float Snap = 4.0f;

	private bool _activeRedraw = false;

	public int ModifyingNote = -1;
	public NoteModificationType NoteModificationType = NoteModificationType.None;
	public bool ActuallyModifyingNote = false;
    public double NoteMoveOffset = 0;
    public double NoteOriginalSize = 0;
    public double NoteOriginalPosition = 0;
    public double PositionWithinNote = 0;

    public IEnumerable<int> NoteTouchingNotes;
	public IEnumerable<int> CursorTouchingNotes;

	private uint _npb;

	public float KeyHSize
	{
		get => KeySize * 4;
	}
	public float AccidentalVSize
	{
		get => KeySize;
	}
	public int KeyFontSize
	{
		get => (int)(KeySize / 2);
	}
	public int AccidentalKeyFontSize
	{
		get => (int)(KeySize / 2);
	}
    public double PositionUnderCursorUnrounded
    {
        get => (GetLocalMousePosition().X - KeyHSize + HScrollOffset) / ((HSpaceBetweenSteps * HZoomFactor) / Snap);
    }
    public double PositionUnderCursor
	{
		get => Math.Floor(PositionUnderCursorUnrounded);
    }
    public double BeatUnderCursorUnrounded
    {
        get => PositionUnderCursorUnrounded / Snap / _npb;
    }
    public double BeatUnderCursor
	{
		get => PositionUnderCursor / Snap / _npb;
    }
    public double BarUnderCursor
    {
        get => BeatUnderCursor * _npb;
    }

    public byte NoteUnderCursor
	{
		get => (byte)(127 - Math.Floor((GetLocalMousePosition().Y + ScrollOffset) / KeySize));
	}

	public bool CanPlaceNote
	{
		get => 
			PositionUnderCursor >= 0 && 
			NoteUnderCursor <= 127 && 
			NoteUnderCursor >= 0 &&
			(PositionUnderCursor / Snap / _npb) + LastLength <= Pattern.MaxBars && 
			!ActuallyModifyingNote &&
			NoteTouchingNotes.Count() == 0;
	}

	public Pattern Pattern = new Pattern
	{
		Notes = []
	};

	public override void _UnhandledKeyInput(InputEvent @event)
	{
		if (@event is InputEventKey)
		{
			InputEventKey e = (InputEventKey)@event;
			if (e.Keycode == Key.Ctrl)
			{
				if (e.Pressed) Mode = PianoRollMode.Select;
				Mode = PreviousMode;
			}
			QueueRedraw();
		}
	}

	private void PlayNoteIndex(int idx)
	{
		PatternNote note = Pattern.Notes[idx];
		if (Pattern.Instrument is not null)
		{
			audiotest.UI.MainController.Instance.Mixer.TryOpenPreviewClock();
            Pattern.Instrument.ScheduleNote(audiotest.UI.MainController.Instance.Mixer.PreviewClock.Time + 16, new NoteEvent
            {
                Note = note.Note,
                Pressed = true,
				Velocity = 255
            });
            Pattern.Instrument.ScheduleNote(
				audiotest.UI.MainController.Instance.Mixer.PreviewClock.Time + 16 + 
				audiotest.UI.MainController.Instance.Mixer.PreviewClock.TimeFromBeat(note.Duration, 0), new NoteEvent
            {
                Note = note.Note,
                Pressed = false,
                Velocity = 255
            });
		}
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventPanGesture)
		{
			InputEventPanGesture e = (InputEventPanGesture)@event;
			if (ScrollOffset > 0 && ScrollOffset < MaxScroll - Size.Y)
			{
				ScrollOffset += e.Delta.Y;
				HScrollOffset += e.Delta.X;
			}
			QueueRedraw();
		}
		else if (@event is InputEventMagnifyGesture)
		{
			InputEventMagnifyGesture e = (InputEventMagnifyGesture)@event;
			HZoomFactor = Math.Clamp(HZoomFactor + e.Factor * 0.1f, 0.1f, 3.0f);
			QueueRedraw();
		} else if (@event is InputEventMouseButton)
		{
			InputEventMouseButton e = (InputEventMouseButton)@event;
			if (e.ButtonIndex == MouseButton.WheelUp)
				ScrollOffset -= e.Factor * 20;
			else if (e.ButtonIndex == MouseButton.WheelDown)
				ScrollOffset += e.Factor * 20;
			else if (e.ButtonIndex == MouseButton.WheelLeft)
				HScrollOffset -= e.Factor * 20;
			else if (e.ButtonIndex == MouseButton.WheelRight)
				HScrollOffset += e.Factor * 20;
			else if (e.ButtonIndex == MouseButton.Left)
            {
                UpdateTouchingNotes();
                if (e.Pressed)
				{
                    if (Mode == PianoRollMode.Draw)
                    {
						if (ModifyingNote != -1 && NoteModificationType != NoteModificationType.None)
						{
							ActuallyModifyingNote = true;
							PlayNoteIndex(ModifyingNote);
						}
                        else if (CanPlaceNote && !ActuallyModifyingNote)
						{
                            Pattern.Notes.Add(new PatternNote
                            {
                                Position = PositionUnderCursor / Snap / _npb,
                                Note = new Note(NoteUnderCursor),
                                Duration = LastLength,
                                Velocity = 255,
                                Selected = false
                            });
                            PlayNoteIndex(Pattern.Notes.Count - 1);
                        }
                    }
                    else if (Mode == PianoRollMode.Erase && CursorTouchingNotes.Count() > 0 && !ActuallyModifyingNote)
                    {
                        Pattern.Notes.RemoveAt(CursorTouchingNotes.First());
                    }
                } else
				{
                    ActuallyModifyingNote = false;
                }
			}
			else if (e.ButtonIndex == MouseButton.Right && e.Pressed)
			{
				UpdateTouchingNotes();
				if (Mode == PianoRollMode.Draw && CursorTouchingNotes.Count() > 0 && !ActuallyModifyingNote)
				{
					Pattern.Notes.RemoveAt(CursorTouchingNotes.First());
				}
			}
			QueueRedraw();
		} else if (@event is InputEventMouseMotion)
		{
			if (Mode == PianoRollMode.Draw)
			{
				if (ActuallyModifyingNote)
                {
                    PatternNote note = Pattern.Notes[ModifyingNote];
					if (NoteModificationType == NoteModificationType.ResizeR)
					{
						double newSize = BeatUnderCursor - note.Position;
						if (newSize > 0)
						{
							note.Duration = newSize;
                            LastLength = newSize;
                        }
					}
					else if (NoteModificationType == NoteModificationType.ResizeL)
					{
                        double newPosition = BeatUnderCursor;
						double newSize = NoteOriginalSize + (NoteOriginalPosition - newPosition);
                        if (newPosition >= 0 && newSize > 0)
						{
                            note.Position = newPosition;
							note.Duration = newSize;
							LastLength = newSize;
                        }
                    }
					else if (NoteModificationType == NoteModificationType.Move)
					{
						double newPos = BeatUnderCursor - NoteMoveOffset;
                        if (newPos >= 0 && NoteUnderCursor < 127)
						{
                            note.Position = newPos;
							note.Note = new Note(NoteUnderCursor);
                        }
					}
				}
				else if (CursorTouchingNotes.Count() > 0)
				{
					if (!ActuallyModifyingNote)
					{
						int firstIndex = CursorTouchingNotes.First();
						PatternNote note = Pattern.Notes[firstIndex];

						PositionWithinNote = ((BeatUnderCursorUnrounded - note.Position) / ((BeatUnderCursorUnrounded - note.Position) + note.Duration)) * 2;

						ModifyingNote = firstIndex;

						if (PositionWithinNote > 0.95)
						{
							MouseDefaultCursorShape = CursorShape.Hsize;
							NoteModificationType = NoteModificationType.ResizeR;
						} else if (PositionWithinNote < 0.05)
						{
							MouseDefaultCursorShape = CursorShape.Hsize;
							NoteModificationType = NoteModificationType.ResizeL;
							NoteOriginalSize = note.Duration;
                            NoteOriginalPosition = note.Position;
                        } else
						{
							MouseDefaultCursorShape = CursorShape.Move;
							NoteModificationType = NoteModificationType.Move;
							NoteMoveOffset = (BeatUnderCursor - note.Position);
                        }
                    }
                } else
                {
					if (!ActuallyModifyingNote)
                    {
						MouseDefaultCursorShape = CursorShape.Arrow;
						ModifyingNote = -1;
                        NoteModificationType = NoteModificationType.None;
                        NoteMoveOffset = 0;
                    }
                }
                QueueRedraw();
            }
		}
	}

	private IEnumerable<int> GetTouchingNotes(double pos, double len, byte no)
	{
		for (int i = 0; i < Pattern.Notes.Count; i++)
		{
			PatternNote note = Pattern.Notes[i];
			if (
				note is not null &&
				note.Note.Number == no &&
				pos + len > note.Position &&
				pos < note.Position + note.Duration
			)
			{
				yield return i;
			}
		}
	}

	private void DrawKey(Note note, Vector2 position)
	{
		if (note.Accidental)
		{
			DrawRect(new Rect2(position, new Vector2(KeyHSize, AccidentalVSize)), Colors.White);
			DrawRect(
				new Rect2(
					position, 
					new Vector2((KeyHSize) - (KeyHSize * AccidentalHSize), AccidentalVSize)
				),
				Colors.Black
			);
			DrawString(ThemeDB.FallbackFont, position + new Vector2(0, (AccidentalVSize / 2) + (AccidentalKeyFontSize / 2)), note.Name, fontSize: AccidentalKeyFontSize);
			DrawLine(position + new Vector2(0, AccidentalVSize), position + new Vector2(Size.X, AccidentalVSize), new Color(0.3f, 0.3f, 0.3f), 1, true);
		}
		else
		{
			DrawRect(new Rect2(position, new Vector2(KeyHSize, KeySize)), Colors.White);
			DrawString(ThemeDB.FallbackFont, position + new Vector2(0, (KeySize / 2) + (KeyFontSize / 2)), note.Name, fontSize: KeyFontSize, modulate: Colors.Black);
			DrawLine(position + new Vector2(0, KeySize), position + new Vector2(Size.X, KeySize), new Color(0.3f, 0.3f, 0.3f), 1, true);
		}
	}

	private void DrawNote(PatternNote note)
	{
		uint npb = audiotest.UI.MainController.Instance.Mixer.Clock.TimeSignature.NotesPerBar;

		Vector2 pos =
			new Vector2(
				(float)((note.Position * npb * (HSpaceBetweenSteps * HZoomFactor)) + KeyHSize - HScrollOffset),
				(float)((127 - note.Note.Number) * KeySize) - (ScrollOffset)
			);

		Vector2 size = 
			new Vector2(
				(float)(note.Duration * npb * ((HSpaceBetweenSteps * HZoomFactor))),
				KeySize
			);


		// cull off-screen notes
		if (
			pos.X + size.X >= KeyHSize
			&& pos.X <= Size.X
			&& pos.Y < Size.Y
			&& pos.Y + size.Y > 0)
		{
			DrawRect(new Rect2(pos, size), note.Selected ? new Color(0.3f, 0.3f, 1f) : new Color(0.3f, 1f, 0.3f));
			DrawRect(new Rect2(pos, size), new Color(0f, 0f, 0f), false, 2);
			DrawString(ThemeDB.FallbackFont, new Vector2(pos.X + 2, pos.Y + (KeySize / 2) + (KeyFontSize / 2)), note.Note.Name, width: size.X, fontSize: KeyFontSize, modulate: Colors.Black);
		}
	}

	public override void _Ready()
	{
		if (!Engine.IsEditorHint())
		{
			VScrollBar = new VScrollBar();
			VScrollBar.SetAnchorsPreset(LayoutPreset.RightWide, true);
			VScrollBar.OffsetLeft = -8;
			VScrollBar.Connect("value_changed", Callable.From<double>((val) =>
			{
				ScrollOffset = (float)val;
				QueueRedraw();
			}));
			CallDeferred("add_child", VScrollBar);

			HScrollBar = new HScrollBar();
			HScrollBar.SetAnchorsPreset(LayoutPreset.TopWide, true);
			HScrollBar.OffsetBottom = 8;
			HScrollBar.Connect("value_changed", Callable.From<double>((val) =>
			{
				HScrollOffset = (float)val;
				QueueRedraw();
			}));
			CallDeferred("add_child", HScrollBar);
		}
	}

	private const float rate = 8.0f;

	public override void _Process(double delta)
	{
		if (ScrollOffset < 0)
			ScrollOffset = (float)Mathf.Lerp(ScrollOffset, 0, 1.0f - Mathf.Exp(-delta * rate));
		else if (ScrollOffset > MaxScroll - Size.Y)
			ScrollOffset = (float)Mathf.Lerp(ScrollOffset, MaxScroll - Size.Y, 1.0f - Mathf.Exp(-delta * rate));


		if (HScrollOffset < 0)
			HScrollOffset = (float)Mathf.Lerp(HScrollOffset, 0, 1.0f - Mathf.Exp(-delta * rate));
		else if (HScrollOffset > HMaxScroll)
			HScrollOffset = (float)Mathf.Lerp(HScrollOffset, HMaxScroll, 1.0f - Mathf.Exp(-delta * rate));

		_activeRedraw = 
			(ScrollOffset < 0 || ScrollOffset > MaxScroll - Size.Y) 
			|| (HScrollOffset < 0 || HScrollOffset > HMaxScroll);

		if (_activeRedraw)
			QueueRedraw();
	}

	private static int _debugFontSize = 16;
	private int _debugTextVAdvance = _debugFontSize;
	private ulong _fc = 0;

	public void DrawDebugString(string s)
	{
		DrawStringOutline(audiotest.UI.MainController.Instance.DebugFont, new Vector2(0, _debugTextVAdvance), s, fontSize: _debugFontSize, modulate: new Color(0, 0, 0), size: 6);
		DrawString(audiotest.UI.MainController.Instance.DebugFont, new Vector2(0, _debugTextVAdvance), s, fontSize: _debugFontSize, modulate: new Color(1, 1, 1));
		_debugTextVAdvance += _debugFontSize;
	}

	public void UpdateTouchingNotes()
	{
		NoteTouchingNotes = GetTouchingNotes(BeatUnderCursor, LastLength, NoteUnderCursor);
		CursorTouchingNotes = GetTouchingNotes(BeatUnderCursor, 0.05f, NoteUnderCursor);
	}

	public override void _Draw()
	{
		float advance = 0;

		if (!Engine.IsEditorHint())
        {
            // calculate max scroll for x axis
            MaxScroll = 0;
			for (byte i = 127; i > 0; i--)
			{
				Note note = new Note { Number = i };
				MaxScroll += note.Accidental ? AccidentalVSize : KeySize;
			}

			VScrollBar.Value = ScrollOffset;
			VScrollBar.MaxValue = MaxScroll - Size.Y;

			HScrollBar.Value = HScrollOffset;

			_npb = MainController.Instance.Mixer.Clock.TimeSignature.NotesPerBar;
		}

		float advancestep = (HSpaceBetweenSteps * HZoomFactor) / Snap;
		uint advancebeat = (uint)(HScrollOffset / (advancestep));

        HMaxScroll = (float)((Pattern.MaxBars * _npb * advancestep * Snap));
        HScrollBar.MaxValue = HMaxScroll;

        if (!Engine.IsEditorHint())
		{
			Vector2 mouseposition = GetLocalMousePosition();

			while ((advancebeat * advancestep) <= HMaxScroll)
			{
				if (advancebeat % (_npb * Snap) == 0)
				{
					DrawLine(
						Vector2.Right * ((advancebeat * advancestep) + KeyHSize - HScrollOffset),
						(Vector2.Right * ((advancebeat * advancestep) + KeyHSize - HScrollOffset)) + (Vector2.Down * Size.Y),
						new Color(0.3f, 0.3f, 0.3f), 2, true
					);
					DrawString(ThemeDB.FallbackFont, new Vector2((advancebeat * advancestep) + KeyHSize - HScrollOffset + 3, 16), $"{advancebeat / (_npb * Snap)}");
				}
				else
				{
					DrawLine(
						Vector2.Right * ((advancebeat * advancestep) + KeyHSize - HScrollOffset),
						(Vector2.Right * ((advancebeat * advancestep) + KeyHSize - HScrollOffset)) + (Vector2.Down * Size.Y),
						new Color(0.2f, 0.2f, 0.2f, 0.75f), 1, true
					);
				}
				advancebeat++;
			}

			foreach (PatternNote note in Pattern.Notes)
			{
				if (note is null) continue;
				DrawNote(note);
			}

			if (Mode == PianoRollMode.Draw)
			{
				UpdateTouchingNotes();
				if (CanPlaceNote) DrawRect(new Rect2(new Vector2((float)((PositionUnderCursor * advancestep) + KeyHSize - HScrollOffset), (float)((Math.Floor((mouseposition.Y + ScrollOffset) / KeySize) * KeySize) - (ScrollOffset))), new Vector2((float)(LastLength * _npb * ((HSpaceBetweenSteps * HZoomFactor))), KeySize)), new Color(0.3f, 1f, 0.3f, 0.5f));
			}
		}

		for (byte i = 127; i > 0; i--)
		{
			Note? note = new Note { Number = i };
			if (advance > ScrollOffset - KeySize && advance < Size.Y + ScrollOffset)
				DrawKey((Note)note, new Vector2(0, advance - ScrollOffset));
			advance += ((Note)note).Accidental ? AccidentalVSize : KeySize;
			note = null;
		}

		if (!Engine.IsEditorHint() && audiotest.UI.MainController.Instance.DebugMode)
		{
			_debugTextVAdvance = _debugFontSize;
			DrawDebugString($"--Pat--");
			DrawDebugString($"Inst: {(Pattern.Instrument is not null ? Pattern.Instrument : "null")}");
			DrawDebugString($"Length: {Pattern.Notes.Count}");
			DrawDebugString($"--Interact--");
			DrawDebugString($"Mode: {Mode}/{PreviousMode}");
			DrawDebugString($"NoteTouchingNotes: {string.Join(',', NoteTouchingNotes)}");
			DrawDebugString($"CursorTouchingNotes: {string.Join(',', CursorTouchingNotes)}");
            DrawDebugString($"ModifyingNote: {ModifyingNote}");
            DrawDebugString($"NoteModificationType: {NoteModificationType}");
            DrawDebugString($"ActuallyModifyingNote: {ActuallyModifyingNote}");
            DrawDebugString($"NoteMoveOffset: {NoteMoveOffset}");
            DrawDebugString($"PositionWithinNote: {PositionWithinNote}");
            DrawDebugString($"--Renderer--");
			DrawDebugString($"fc: {_fc}");
			DrawDebugString($"Redrawing: {(_activeRedraw ? "Active" : "Lazy")}");
			DrawDebugString($"PositionUnderCursor: {PositionUnderCursor} ({PositionUnderCursorUnrounded})");
			DrawDebugString($"NoteUnderCursor: {NoteUnderCursor}");
		}

		_fc++;
	}
}

using Godot;

using audiotest.Core.AudioEngine;
using audiotest.Core.Sequencing;

namespace audiotest.UI
{
    public partial class PatternLine : Control
    {
        public PackedScene PatternEntry;

        public HBoxContainer Entries;

        public Button NewButton;
        public Button CloneButton;
        public Button CloneIndexButton;
        public Button DeleteButton;
        public Button MoveLeftButton;
        public Button ShrinkButton;
        public Button GrowButton;
        public Button MoveRightButton;

        public Instrument Instrument;

        private int _selectedPatternIndex = -1;

        public override void _Ready()
        {
            PatternEntry = GD.Load<PackedScene>("res://UI/PatternEntry.tscn");

            Entries = GetNode<HBoxContainer>("Entries");

            NewButton = GetNode<Button>("Buttons/NewButton");
            CloneButton = GetNode<Button>("Buttons/CloneButton");
            CloneIndexButton = GetNode<Button>("Buttons/CloneIndexButton");
            DeleteButton = GetNode<Button>("Buttons/DeleteButton");
            MoveLeftButton = GetNode<Button>("Buttons/MoveLeftButton");
            ShrinkButton = GetNode<Button>("Buttons/ShrinkButton");
            GrowButton = GetNode<Button>("Buttons/GrowButton");
            MoveRightButton = GetNode<Button>("Buttons/MoveRightButton");

            ReloadPatterns();

            NewButton.Pressed += () =>
            {
                Instrument.Patterns.Add(new Pattern(Instrument));
                Instrument.PatternSequence.Add((uint)Instrument.Patterns.Count - 1);
                ReloadPatterns();
            };

            CloneButton.Pressed += () =>
            {
                if (_selectedPatternIndex < 0 || _selectedPatternIndex > Instrument.PatternSequence.Count)
                    return;

                uint patternIndex = Instrument.PatternSequence[_selectedPatternIndex];
                Pattern pattern = Instrument.Patterns[(int)patternIndex];

                Instrument.Patterns.Add(pattern);
                Instrument.PatternSequence.Add((uint)Instrument.Patterns.Count - 1);

                ReloadPatterns();
            };

            CloneIndexButton.Pressed += () =>
            {
                if (_selectedPatternIndex < 0 || _selectedPatternIndex > Instrument.PatternSequence.Count)
                    return;

                uint patternIndex = Instrument.PatternSequence[_selectedPatternIndex];
                Instrument.PatternSequence.Add(patternIndex);

                ReloadPatterns();
            };

            DeleteButton.Pressed += () =>
            {
                if (_selectedPatternIndex < 0 || _selectedPatternIndex > Instrument.PatternSequence.Count)
                    return;

                uint patternIndex = Instrument.PatternSequence[_selectedPatternIndex];

                Instrument.Patterns.RemoveAt((int)patternIndex);
                Instrument.PatternSequence.RemoveAt(_selectedPatternIndex);

                ReloadPatterns();
            };
        }

        public void ReloadPatterns()
        {
            foreach (Node child in Entries.GetChildren())
                Entries.RemoveChild(child);

            foreach (int idx in Instrument.PatternSequence)
            {
                Button entry = PatternEntry.Instantiate<Button>();
                if (_selectedPatternIndex == idx) entry.GetNode<Control>("Active").Visible = true;
                entry.GetNode<Label>("Label").Text = $"{idx}";
                entry.Pressed += () => { 
                    if (_selectedPatternIndex == idx)
                    {
                        Window window = new Window();
                        PianoRollWindow c = GD.Load<PackedScene>("res://UI/PianoRoll.tscn").Instantiate<PianoRollWindow>();
                        c.GetNode<PianoRoll>("PianoRoll").Pattern = Instrument.Patterns[(int)Instrument.PatternSequence[_selectedPatternIndex]];
                        window.Title = "piano roll";
                        window.InitialPosition = Window.WindowInitialPosition.CenterPrimaryScreen;
                        window.Size = new Vector2I(700, 400);
                        window.CloseRequested += () => MainController.Instance.RemoveChild(window);
                        window.CallDeferred("add_child", c);
                        MainController.Instance.CallDeferred("add_child", window);
                    } else
                    {
                        _selectedPatternIndex = idx;
                        ReloadPatterns();
                    }
                };
                Entries.AddChild(entry);
            }
        }
    }
}

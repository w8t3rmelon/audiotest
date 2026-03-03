using Godot;
using System;

using Range = Godot.Range;

using audiotest.UI.Temporary.InstrumentEditor;
using audiotest.Core.AudioEngine;

namespace audiotest.UI
{
    public partial class InstrumentEntry : Control
    {
        public Instrument Instrument;

        public override void _Ready()
        {
            if (Instrument is not null)
            {
                GetNode<Label>("Inner/NameLabel").Text = Instrument.Name;

                GetNode<Range>("Inner/Volume").ValueChanged += (val) =>
                {
                    Instrument.Volume = (float)val;
                };

                GetNode<PopupMenu>("Inner/Options/Menu").IdPressed += (id) =>
                {
                    ((Action)(id switch
                    {
                        0 => () => {
                            Window window = new Window();
                            InstrumentEditor editor = GD.Load<PackedScene>("res://UI/Temporary/InstrumentEditor/InstrumentEditor.tscn").Instantiate<InstrumentEditor>();
                            editor.Instrument = Instrument;
                            window.AddChild(editor);
                            window.Title = "instrument editor";
                            window.Size = new Vector2I(415, 613);
                            window.InitialPosition = Window.WindowInitialPosition.CenterPrimaryScreen;
                            window.CloseRequested += () => { MainController.Instance.RemoveChild(window); };
                            MainController.Instance.AddChild(window);
                        }
                        , // edit
                        1 => () => {
                            MainController.Instance.Mixer.SampleProviders.Remove(Instrument);
                            MainController.Instance.ResetInstrumentAndPatternPanes();
                        }, // destroy
                        _ => null
                    }) ?? (() => { }))();
                };
            }

            GetNode<Button>("Inner/Options").Pressed += () =>
            {
                GetNode<PopupMenu>("Inner/Options/Menu").Popup(new Rect2I((Vector2I)GetGlobalMousePosition(), new Vector2I(100, 100)));
            };
        }
    }
}

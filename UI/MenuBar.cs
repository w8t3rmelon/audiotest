using Godot;
using System;

namespace audiotest.UI
{
	public partial class MenuBar : Godot.MenuBar
	{
		public override void _Ready()
		{
			GetNode<PopupMenu>("File").IdPressed += (long id) =>
			{
				((Action)(id switch
				{
					2 => () => GetTree().Quit(), // "Exit"
					_ => null
				}) ?? (() => {}))();
			};

			GetNode<PopupMenu>("Edit").IdPressed += (long id) =>
			{
				((Action)(id switch
				{
					_ => null
				}) ?? (() => { }))();
			};

			GetNode<PopupMenu>("Pattern").IdPressed += (long id) =>
			{
				((Action)(id switch
				{
					_ => null
				}) ?? (() => { }))();
			};

			GetNode<PopupMenu>("Instrument").IdPressed += (long id) =>
			{
				((Action)(id switch
				{
					_ => null
				}) ?? (() => { }))();
			};

			GetNode<PopupMenu>("Window").IdPressed += (long id) =>
			{
				((Action)(id switch
				{
					0 => () => // "Piano Roll"
					{
						Window window = new Window();
						PianoRollWindow c = GD.Load<PackedScene>("res://UI/PianoRoll.tscn").Instantiate<PianoRollWindow>();
						window.Title = "piano roll";
						window.InitialPosition = Window.WindowInitialPosition.CenterPrimaryScreen;
						window.Size = new Vector2I(700, 400);
						window.CloseRequested += () => UI.MainController.Instance.RemoveChild(window);
						window.CallDeferred("add_child", c);
						UI.MainController.Instance.CallDeferred("add_child", window);
					},
					_ => null
				}) ?? (() => { }))();
			};

			GetNode<PopupMenu>("Help").IdPressed += (long id) =>
			{
				((Action)(id switch
				{
					_ => null
				}) ?? (() => { }))();
			};
		}
	}
}

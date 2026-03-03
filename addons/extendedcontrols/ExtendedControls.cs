#if TOOLS
using Godot;
using System;

[Tool]
public partial class ExtendedControls : EditorPlugin
{
	public override void _EnterTree()
	{
		string dir = ((CSharpScript)GetScript()).ResourcePath.GetBaseDir();
		AddCustomType("Knob", "Range", GD.Load<CSharpScript>($"{dir}/Knob.cs"), new Texture2D());
	}

	public override void _ExitTree()
	{
		RemoveCustomType("Knob");
	}
}
#endif

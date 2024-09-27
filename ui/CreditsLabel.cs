using Godot;

public partial class CreditsLabel : RichTextLabel
{
	public override void _Ready()
	{
		base._Ready();

		MetaClicked += OpenLink;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		MetaClicked -= OpenLink;
	}

	private void OpenLink(Variant meta)
	{
		GD.Print("Open link called");
		OS.ShellOpen(meta.ToString());
	}
}

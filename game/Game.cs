using Godot;

public partial class Game : Node
{
	private GameScreen GameScreen { get; set; }
	private UI UI { get; set; }
	private Score Scores { get; set; }


	public override void _Ready()
	{
		base._Ready();
		
		GameScreen = GetNode<GameScreen>("%GameScreen");
		UI = GetNode<UI>("%UI");

		GameScreen.OnWinGame += WinGame;
		GameScreen.OnLoseGame += LoseGame;
		GameScreen.OnEndLevel += OpenEndLevelMenu;
		UI.OnStartPressed += StartGame;
		UI.OnNextLevelPressed += NextLevel;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		GameScreen.OnWinGame -= WinGame;
		GameScreen.OnLoseGame -= LoseGame;
		GameScreen.OnEndLevel -= OpenEndLevelMenu;
		UI.OnStartPressed -= StartGame;
		UI.OnNextLevelPressed -= NextLevel;
	}

	private void StartGame()
	{
		Scores = new Score();
		GameScreen.StartGame(Scores);
	}

	private void OpenEndLevelMenu()
	{
		UI.OpenEndLevelMenu(Scores);
	}

	private void NextLevel()
	{
		GameScreen.EndLevel();
	}

	private void WinGame()
	{
		UI.OpenGameOverMenu(Scores);
	}
	
	private void LoseGame()
	{
		UI.OpenGameOverMenu(Scores);
	}
}

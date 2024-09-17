using Godot;
using System;
using System.Collections.Generic;

public class LeaderboardHandler
{
	public event Action<List<HighScore>> OnHighScoresRecieved;

	public List<HighScore> HighScores { get; private set; } = new();

	private Node Parent { get; }

	// LootLocker
	private string GameApiKey { get; set; } = "dev_572681fbfc60412ca84c2a9c47a00326";
	private string LeaderboardKey { get; set; } = "SnoodCloneLeaderboard";
	private string SessionToken { get; set; } = "";

	// HTTP Request node can only handle one call per node.
	private HttpRequest AuthHttp { get; set; }
	private HttpRequest LeaderboardHttp { get; set; }
	private HttpRequest SubmitScoreHttp { get; set; }

	private string PlayerIdentifier { get; set; }

	public LeaderboardHandler(Node parent)
	{
		Parent = parent;
	}

	public void UploadScore(HighScore highScore)
	{
		string playerName = highScore.Name;
		int score = highScore.Score;

		Godot.Collections.Dictionary data = new()
		{
			{ "member_id", PlayerIdentifier },
			{ "score", score},
			{ "metadata", playerName }
		};

		string[] headers =
		{
			"Content-Type: application/json",
			"x-session-token:" + SessionToken
		};

		SubmitScoreHttp = new HttpRequest();
		Parent.AddChild(SubmitScoreHttp);
		SubmitScoreHttp.RequestCompleted += OnUploadScoreRequestComplete;

		// Send request
		SubmitScoreHttp.Request("https://api.lootlocker.io/game/leaderboards/" + LeaderboardKey + "/submit", headers, HttpClient.Method.Post, data.ToString());

		// Print what we're sending, for debugging purposes.
		GD.Print($"Submit score data: {data}");
	}

	public void RequestHighScores()
	{
		// TODO: Convert data to JSON string
		Godot.Collections.Dictionary data = new()
		{
			{ "game_key", GameApiKey },
			{ "game_version", "0.0.0.1"},
			{ "development_mode", true }
		};

		string jsonData = Json.Stringify(data);

		// Add "Content-Type" header
		string[] headers = { "Content-Type: application/json" };

		// Create an HTTPRequest node for authentication
		AuthHttp = new HttpRequest();
		
		// TODO: Or use CallDeferred?
		Parent.AddChild(AuthHttp);
		AuthHttp.RequestCompleted += OnAuthenticationRequestComplete;

		GD.Print($"Data as string: {data.ToString()}");

		// Send request
		AuthHttp.Request("https://api.lootlocker.io/game/v2/session/guest", headers, HttpClient.Method.Post, data.ToString());
		
		// Print what we're sending, for debugging purposes
		GD.Print($"Sent data: {data}");
		
		// TODO: Show loading label?
	}

	private void OnAuthenticationRequestComplete(long result, long responseCode, string[] headers, byte[] body)
	{
		Json json = new();
		/* Error e =  */json.Parse(body.GetStringFromUtf8());
		//GD.Print($"Error: {e}");

		// Save player_identifier to file
		Godot.Collections.Dictionary data = (Godot.Collections.Dictionary)json.Data;
/* 		foreach (string key in data.Keys)
		{
			GD.Print($"Key: {key}, Value: {data[key]}");
		} */
		PlayerIdentifier = (string)data["player_identifier"];

		// Save session token to memory
		SessionToken = (string)data["session_token"];
		GD.Print($"Session token: {SessionToken}");

		// Print server response
		GD.Print($"Response: {json.Data}");

		// Clear node		
		AuthHttp.RequestCompleted -= OnAuthenticationRequestComplete;
		AuthHttp.QueueFree();

		GetScores();
	}

	private void GetScores()
	{
		string url = "https://api.lootlocker.io/game/leaderboards/" + LeaderboardKey + "/list?count=10";
		string[] headers =
		{
			"Content-Type: application/json",
			"x-session-token:" + SessionToken
		};

		// Create a request node for getting the highscores
		LeaderboardHttp = new HttpRequest();
		Parent.AddChild(LeaderboardHttp);
		LeaderboardHttp.RequestCompleted += OnLeaderboardRequestComplete;

		// Send request
		LeaderboardHttp.Request(url, headers, HttpClient.Method.Get, "");
	}

	private void OnLeaderboardRequestComplete(long result, long responseCode, string[] headers, byte[] body)
	{
		Json json = new();
		json.Parse(body.GetStringFromUtf8());

		// Print data
		Godot.Collections.Dictionary data = (Godot.Collections.Dictionary)json.Data;
		GD.Print($"Leaderboard request completed. Data: {data}");

		// Needed?
		HighScores.Clear();

		// Formatting as a leaderboard
		string rank;
		string playerName;
		string scoreString;
		Godot.Collections.Array items = (Godot.Collections.Array)data["items"];
		GD.Print($"Json size: {items.Count}");
		// Get top ten scores
		for (int index = 0; index < Mathf.Min(items.Count, 10); index++)
		{
			Godot.Collections.Dictionary dict = (Godot.Collections.Dictionary)items[index];
			rank = dict["rank"].ToString();
			playerName = dict["metadata"].ToString();
			scoreString = dict["score"].ToString();
			if (int.TryParse(scoreString, out int score))
			{
				HighScore highScore = new(playerName, score);
				HighScores.Add(highScore);
			}
			else
			{
				GD.PushError($"Couldn't cast {scoreString} to int.");
			}

			// Print the formatted leaderboard to the console
			GD.Print($"Rank: {rank}, Name: {playerName}, Score: {scoreString}");
		}

		// TODO: Setup leaderboard data as a List<HighScore> and send it through event. To UI? Or Scoreboard?
		// Have UI control this class, then GameOverMenu and Scoreboard can both access it.
		OnHighScoresRecieved(HighScores);

		// Clear node
		LeaderboardHttp.RequestCompleted -= OnLeaderboardRequestComplete;
		LeaderboardHttp.QueueFree();
	}

	private void OnUploadScoreRequestComplete(long result, long responseCode, string[] headers, byte[] body)
	{
		Json json = new();
		json.Parse(body.GetStringFromUtf8());

		// Print data
		GD.Print($"Score request complete data: {json.Data}");

		// Clear node
		SubmitScoreHttp.RequestCompleted -= OnUploadScoreRequestComplete;
		SubmitScoreHttp.QueueFree();

		// Reload menu with new scores
		GetScores();
	}
}

using Godot;
using System;
using System.Collections.Generic;

public class LeaderboardHandler
{
	public event Action<List<HighScore>> OnHighScoresRecieved;

	private const string GAME_API_KEY = "dev_572681fbfc60412ca84c2a9c47a00326";
	private const string LEADERBOARD_KEY = "SnoodCloneLeaderboard";
	
	public List<HighScore> HighScores { get; private set; } = new();

	private Node Parent { get; }

	private string SessionToken { get; set; } = "";

	// HTTP Request node can only handle one call per node.
	private HttpRequest AuthHttp { get; set; }
	private HttpRequest LeaderboardHttp { get; set; }
	private HttpRequest SubmitScoreHttp { get; set; }


	public LeaderboardHandler(Node parent)
	{
		Parent = parent;
	}

	public void RequestHighScores()
	{
		string url = "https://api.lootlocker.io/game/v2/session/guest";
		
		string[] headers = { "Content-Type: application/json" };

		Godot.Collections.Dictionary data = new()
		{
			{ "game_key", GAME_API_KEY },
			{ "game_version", "0.0.0.1"},
			{ "development_mode", true }
		};

		// Create an HTTPRequest node for authentication
		AuthHttp = new HttpRequest();
		Parent.AddChild(AuthHttp);
		AuthHttp.RequestCompleted += OnAuthenticationRequestComplete;
		
		// Send request
		AuthHttp.Request(url, headers, HttpClient.Method.Post, data.ToString());
	}

	public void UploadScore(HighScore highScore)
	{
		string url = $"https://api.lootlocker.io/game/leaderboards/{LEADERBOARD_KEY}/submit";

		string[] headers =
		{
			"Content-Type: application/json",
			"x-session-token:" + SessionToken
		};
		
		Godot.Collections.Dictionary data = new()
		{
			{ "member_id", highScore.Name },
			{ "score", highScore.Score }
		};

		SubmitScoreHttp = new HttpRequest();
		Parent.AddChild(SubmitScoreHttp);
		SubmitScoreHttp.RequestCompleted += OnUploadScoreRequestComplete;

		// Send request
		SubmitScoreHttp.Request(url, headers, HttpClient.Method.Post, data.ToString());
	}

	private void GetScores()
	{
		string url = $"https://api.lootlocker.io/game/leaderboards/{LEADERBOARD_KEY}/list?count=10";
		
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
		LeaderboardHttp.Request(url, headers, HttpClient.Method.Get);
	}

	private void OnAuthenticationRequestComplete(long result, long responseCode, string[] headers, byte[] body)
	{
		Json json = new();
		json.Parse(body.GetStringFromUtf8());
		Godot.Collections.Dictionary data = (Godot.Collections.Dictionary)json.Data;
		
		// Save session token to memory
		SessionToken = (string)data["session_token"];
		
		// Clear node		
		AuthHttp.RequestCompleted -= OnAuthenticationRequestComplete;
		AuthHttp.QueueFree();

		GetScores();
	}

	private void OnLeaderboardRequestComplete(long result, long responseCode, string[] headers, byte[] body)
	{
		Json json = new();
		json.Parse(body.GetStringFromUtf8());
		Godot.Collections.Dictionary data = (Godot.Collections.Dictionary)json.Data;
		Godot.Collections.Array items = (Godot.Collections.Array)data["items"];
		
		// Get top ten scores
		HighScores.Clear();
		for (int index = 0; index < Mathf.Min(items.Count, 10); index++)
		{
			Godot.Collections.Dictionary dict = (Godot.Collections.Dictionary)items[index];
			string playerName = dict["member_id"].ToString();
			string scoreString = dict["score"].ToString();
			if (int.TryParse(scoreString, out int score))
			{
				HighScore highScore = new(playerName, score);
				HighScores.Add(highScore);
			}
			else
			{
				GD.PushError($"Couldn't cast {scoreString} to int.");
			}
		}
		OnHighScoresRecieved(HighScores);

		// Clear node
		LeaderboardHttp.RequestCompleted -= OnLeaderboardRequestComplete;
		LeaderboardHttp.QueueFree();
	}

	private void OnUploadScoreRequestComplete(long result, long responseCode, string[] headers, byte[] body)
	{
		// Clear node
		SubmitScoreHttp.RequestCompleted -= OnUploadScoreRequestComplete;
		SubmitScoreHttp.QueueFree();

		// Reload menu with new scores
		GetScores();
	}
}

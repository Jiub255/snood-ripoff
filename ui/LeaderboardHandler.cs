using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public class LeaderboardHandler
{
	public List<HighScore> HighScores { get; set; } = new();
	private Node Parent { get; }

	// LootLocker
	private string GameApiKey { get; set; } = "ENTER API KEY HERE";
	private bool DevelopmentMode { get; set; } = true;
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
	
	// Or call it Login?
	private void AuthenticationRequest()
	{
		// Convert data to JSON string
		Dictionary<string, object> data = new()
		{
			{ "game_key", GameApiKey },
			{ "game_version", "0.0.0.1"},
			{ "development_mode", true }
		};

		// Add "Content-Type" header
		string[] headers = { "Content-Type: application/json" };

		// Create an HTTPRequest node for authentication
		AuthHttp = new HttpRequest();
		// TODO: Or use CallDeferred?
		Parent.AddChild(AuthHttp);
		AuthHttp.RequestCompleted += OnAuthenticationRequestComplete;

		// Send request
		AuthHttp.Request("https://api.lootlocker.io/game/v2/session/guest", headers, HttpClient.Method.Post, data.ToString());
		
		// Print what we're sending, for debugging purposes
		GD.Print($"Sent data: {data}");
		
		// TODO: Show loading label?
	}
	
	private void OnAuthenticationRequestComplete(long result, long responseCode, string[] headers, byte[] body)
	{
		Json json = new();
		json.Parse(body.GetStringFromUtf8());

		// Save player_identifier to file
		Godot.Collections.Dictionary data = (Godot.Collections.Dictionary)json.Data;
		PlayerIdentifier = (string)data["player_identifier"];
		
		// Save session token to memory
		SessionToken = (string)data["session_token"];
		GD.Print($"Session token: {SessionToken}");

		// Print server response
		GD.Print($"Response: {json.Data.ToString()}");
		
		// Clear node		
		AuthHttp.RequestCompleted -= OnAuthenticationRequestComplete;
		AuthHttp.QueueFree();

		// TODO: Get leaderboard
		//GetScores();
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
		GD.Print($"Leaderboard request completed. Data: {data.ToString()}");

		// Needed?
		//HighScores.Clear();

		// Formatting as a leaderboard
		string rank;
		string playerName;
		string score;
		Godot.Collections.Array items = (Godot.Collections.Array)data["items"];
		GD.Print($"Json size: {items.Count}");
		for (int index = 0; index < items.Count; index++)
		{
			Godot.Collections.Dictionary dict = (Godot.Collections.Dictionary)items[index];
			rank = dict["rank"].ToString();
			playerName = dict["metadata"].ToString();
			score = dict["score"].ToString();

			// Print the formatted leaderboard to the console
			GD.Print($"Rank: {rank}, Name: {playerName}, Score: {score}");
			
			// Add to high scores? No.
		}
		
		// Clear node
		LeaderboardHttp.RequestCompleted -= OnLeaderboardRequestComplete;
		LeaderboardHttp.QueueFree();
	}
	
	// TODO: UpdateScore next
	private void UploadScore(string playerName, int score)
	{
		Dictionary<string, object> data = new()
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
	
	private void OnUploadScoreRequestComplete(long result, long responseCode, string[] headers, byte[] body)
	{
		Json json = new();
		json.Parse(body.GetStringFromUtf8());
		
		// Print data
		GD.Print($"Score request complete data: {json.Data}");

		// Clear node
		SubmitScoreHttp.QueueFree();

		// Reload menu with new scores
		GetScores();
		
		SubmitScoreHttp.RequestCompleted -= OnUploadScoreRequestComplete;
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[Serializable]
public class ScoreEntry {
    public int scoreValue;
    public string date;
}

[Serializable]
public class SaveData {
    public int highscore;
    public List<ScoreEntry> leaderboard = new List<ScoreEntry>();
}

public static class SaveManager 
{
    private static string Path => System.IO.Path.Combine(Application.persistentDataPath, "leaderboard.json");

    public static void SaveScore(int score) {
        SaveData data = LoadData();
        
        // Ajouter et trier
        data.leaderboard.Add(new ScoreEntry { 
            scoreValue = score, 
            date = DateTime.Now.ToString("dd/MM/yyyy HH:mm") 
        });
        data.leaderboard = data.leaderboard.OrderByDescending(s => s.scoreValue).Take(10).ToList();
        
        // Update record absolu
        if (score > data.highscore) data.highscore = score;

        File.WriteAllText(Path, JsonUtility.ToJson(data, true));
    }

    public static SaveData LoadData() {
        if (!File.Exists(Path)) return new SaveData();
        return JsonUtility.FromJson<SaveData>(File.ReadAllText(Path));
    }
    public static string GetLeaderboardFormatted() {
        SaveData data = LoadData();
        if (data.leaderboard == null || data.leaderboard.Count == 0) 
            return "AUCUN SCORE ENREGISTRÉ";

        string text = "TOP 10 SCORES\n\n";
        for (int i = 0; i < data.leaderboard.Count; i++) {
            text += $"{i + 1}. {data.leaderboard[i].scoreValue} pts ({data.leaderboard[i].date})\n";
        }
        return text;
    }
}
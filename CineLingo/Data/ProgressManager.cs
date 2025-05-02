using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Newtonsoft.Json;

public class ProgressManager
{
    private static readonly string ProgressFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "progress.json");
    private Dictionary<string, Dictionary<string, double>> progressData;

    public ProgressManager()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ProgressFilePath));
        LoadProgress();
    }

    private void LoadProgress()
    {
        try
        {
            if (File.Exists(ProgressFilePath))
            {
                string json = File.ReadAllText(ProgressFilePath);
                progressData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, double>>>(json)
                               ?? new Dictionary<string, Dictionary<string, double>>();
            }
            else
            {
                progressData = new Dictionary<string, Dictionary<string, double>>();
            }
        }
        catch (Exception ex)
        {
            progressData = new Dictionary<string, Dictionary<string, double>>();
            MessageBox.Show($"Ошибка при загрузке прогресса: {ex.Message}");
        }
    }

    private void SaveProgress()
    {
        try
        {
            string json = JsonConvert.SerializeObject(progressData, Formatting.Indented);
            File.WriteAllText(ProgressFilePath, json);
            MessageBox.Show($"Saved progress data: {json}");
            MessageBox.Show($"Saved to: {ProgressFilePath}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при сохранении прогресса: {ex.Message}");
        }
    }

    public void SaveUserProgress(string username, string moviePath, TimeSpan currentTime)
    {
        try
        {
            if (!progressData.ContainsKey(username))
            {
                progressData[username] = new Dictionary<string, double>();
            }

            progressData[username][moviePath] = currentTime.TotalSeconds;
            SaveProgress();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при установке прогресса: {ex.Message}");
        }
    }

    public TimeSpan? GetSavedPosition(string username, string moviePath)
    {
        try
        {
            if (progressData.ContainsKey(username) && progressData[username].ContainsKey(moviePath))
            {
                double seconds = progressData[username][moviePath];
                return TimeSpan.FromSeconds(seconds);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при получении прогресса: {ex.Message}");
        }

        return null;
    }
}

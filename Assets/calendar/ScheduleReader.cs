using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class ScheduleReader : MonoBehaviour
{
    public List<string[]> csvData = new List<string[]>();

    private string persistentPath;

    void Awake()
    {
        //AddRow("2025/11/04/13:39:50.56","2026/01/06/00:00","assignment","課題3(2次提出)","null");



        persistentPath = Path.Combine(Application.persistentDataPath, "schedule.csv");


        //LoadFromResources();
        //SaveToPersistent();


        //// 1. persistentDataPath の CSV があればそちらを読む
        if (File.Exists(persistentPath))
        {
            LoadFromPersistent();
        }
        else
        {
            // 2. なければ Resources を読み、persistentDataPath へコピー
            LoadFromResources();
            SaveToPersistent();
        }

        // Debug: 中身確認
        for (int i = 0; i < csvData.Count; i++)
        {
            Debug.Log($"{csvData[i][0]}, {csvData[i][1]}, {csvData[i][2]}, {csvData[i][3]}, {csvData[i][4]}");
        }
    }

    // Resources/schedule.csv を読み込む
    private void LoadFromResources()
    {
        csvData.Clear();

        TextAsset csvFile = Resources.Load<TextAsset>("schedule");
        if (csvFile == null)
        {
            Debug.LogError("Resources/schedule.csv が見つかりません");
            return;
        }

        using (StringReader reader = new StringReader(csvFile.text))
        {
            while (reader.Peek() > -1)
            {
                string line = reader.ReadLine();
                csvData.Add(line.Split(','));
            }
        }
    }

    // persistentDataPath の CSV を読み込む
    // public List<string[]> LoadFromPersistent()
    // {
    //     csvData.Clear();

    //     string[] lines = File.ReadAllLines(persistentPath);

    //     foreach (var line in lines)
    //     {
    //         csvData.Add(line.Split(','));
    //     }
    //     return csvData;
    // }

    public List<string[]> LoadFromPersistent()
    {
        if (string.IsNullOrEmpty(persistentPath))
        {
            persistentPath = Path.Combine(Application.persistentDataPath, "schedule.csv");
            Debug.Log("[ScheduleReader] persistentPath initialized lazily");
        }

        csvData.Clear();

        if (!File.Exists(persistentPath))
        {
            Debug.LogWarning("persistent csv not found");
            return csvData;
        }

        string[] lines = File.ReadAllLines(persistentPath);
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            csvData.Add(line.Split(','));
        }

        return csvData;
    }
    // csvData を persistentDataPath に保存する
    public void SaveToPersistent()
    {
        List<string> lines = new List<string>();

        foreach (var row in csvData)
        {
            lines.Add(string.Join(",", row));
        }

        File.WriteAllLines(persistentPath, lines);
        Debug.Log("保存しました → " + persistentPath);
    }
    public void AddRow(string start, string deadline, string type, string title, string other)
    {
        string[] row = new string[5] { start, deadline, type, title, other };
        csvData.Add(row);
        SaveToPersistent();
    }

}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public class AssignmentManager : MonoBehaviour
{
    [SerializeField] private ScheduleReader scheduleReader;

    private List<AssignmentTask> assignments = new List<AssignmentTask>();

    // 複数フォーマット対応（重要）
    private static readonly string[] DateFormats =
    {
        "yyyy/MM/dd/HH:mm",
        "yyyy/MM/dd/HH:mm:ss",
        "yyyy/MM/dd/HH:mm:ss.f",
        "yyyy/MM/dd/HH:mm:ss.ff",
        "yyyy/MM/dd/HH:mm:ss.fff"
    };

    void Start()
    {
        if (scheduleReader == null)
        {
            Debug.LogError("ScheduleReader が Inspector で設定されていません");
            return;
        }

        LoadAssignments();
        LogAssignments();
    }

    void LoadAssignments()
    {
        assignments.Clear();
        DateTime now = DateTime.Now;

        foreach (var row in scheduleReader.csvData)
        {
            if (row == null || row.Length < 5) continue;

            // assignment のみ抽出
            if (row[2] != "assignment") continue;

            // 締切日時パース
            if (!DateTime.TryParseExact(
                    row[1],
                    DateFormats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime deadline))
            {
                Debug.LogError($"締切パース失敗: {row[1]}");
                continue;
            }

            AssignmentTask task = new AssignmentTask
            {
                title = row[3],
                detail = row[4],
                deadline = deadline,
                remaining = deadline - now
            };

            assignments.Add(task);
        }
    }

    void LogAssignments()
    {
        Debug.Log("===== 課題一覧 =====");

        foreach (var task in assignments.OrderBy(a => a.remaining))
        {
            if (task.remaining.TotalSeconds < 0)
            {
                Debug.Log($"[期限切れ] {task.title}");
                continue;
            }

            Debug.Log(
                $"[課題]\n" +
                $"件名: {task.title}\n" +
                $"締切: {task.deadline:yyyy/MM/dd HH:mm}\n" +
                $"残り: {task.remaining.Days}日 " +
                $"{task.remaining.Hours}時間 " +
                $"{task.remaining.Minutes}分\n" +
                $"詳細: {task.detail}"
            );
        }
    }
}

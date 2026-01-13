using System;
using UnityEngine;

// 「課題1件」を表すデータ構造
[Serializable]   // ★ 将来UIや保存に使える
public class AssignmentTask
{
    public string title;        // 課題名
    public string detail;       // 詳細
    public DateTime deadline;   // 締切日時
    public TimeSpan remaining;  // 残り時間
}

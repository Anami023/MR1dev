using System;
using System.Text;
using UnityEngine;
using NativeWebSocket;

// ===== WebSocketメッセージ構造 =====
[Serializable]
public class WebSocketMessage
{
    public string type;
    public string data;
}

[Serializable]
public class sabaresuponsu
{
    public string type;
    public string message;
    public string csv_data;
    public string filename;
}

// ===== カレンダーCSV受信クライアント =====
public class CSVIO : MonoBehaviour
{
    [Header("接続設定")]
    public string serverUrl = "ws://172.21.1.123:8000/ws";

    [Header("CSV保存設定")]
    public string csvFileName = "schedule.csv";
    public bool saveToStreamingAssets = false; // true: StreamingAssets, false: persistentDataPath

    private WebSocket websocket;
    private bool isConnected = false;

    void Start()
    {
        Debug.Log("[CalendarClient] 初期化完了");
        Connect();
        // RequestCalendarCsvUpdate();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (websocket != null)
        {
            websocket.DispatchMessageQueue();
        }
#endif
    }

    // ===== 接続管理 =====
    public async void Connect()
    {
        if (isConnected)
        {
            Debug.LogWarning("[CalendarClient] 既に接続されています");
            return;
        }

        Debug.Log($"[CalendarClient] 接続中: {serverUrl}");

        try
        {
            websocket = new WebSocket(serverUrl);

            websocket.OnOpen += () =>
            {
                isConnected = true;
                Debug.Log("[CalendarClient] WebSocket接続成功");
                // RequestCalendarCsvUpdate();
            };

            websocket.OnMessage += (bytes) =>
            {
                string message = Encoding.UTF8.GetString(bytes);
                HandleMessage(message);
            };

            websocket.OnError += (error) =>
            {
                Debug.LogError($"[CalendarClient] WebSocketエラー: {error}");
            };

            websocket.OnClose += (closeCode) =>
            {
                isConnected = false;
                Debug.Log($"[CalendarClient] WebSocket切断 (Code: {closeCode})");
            };

            await websocket.Connect();
        }
        catch (Exception e)
        {
            Debug.LogError($"[CalendarClient] 接続エラー: {e.Message}");
        }
    }

    public async void Disconnect()
    {
        if (websocket != null)
        {
            await websocket.Close();
            websocket = null;
        }
        isConnected = false;
        Debug.Log("[CalendarClient] 切断しました");
    }

    // ===== CSV更新リクエスト =====
    public async void RequestCalendarCsvUpdate()
    {
        if (!isConnected || websocket == null)
        {
            Debug.LogError("[CalendarClient] WebSocketが接続されていません");
            return;
        }

        WebSocketMessage message = new WebSocketMessage
        {
            type = "get_calendar_csv"
        };

        string json = JsonUtility.ToJson(message);
        await websocket.SendText(json);

        Debug.Log("[CalendarClient] カレンダーCSV更新リクエスト送信");
    }

    // ===== メッセージ処理 =====
    void HandleMessage(string message)
    {
        try
        {
            sabaresuponsu response = JsonUtility.FromJson<sabaresuponsu>(message);

            switch (response.type)
            {
                case "connection":
                    Debug.Log($"[CalendarClient] サーバー接続: {response.message}");
                    break;

                case "calendar_csv":
                    HandleCalendarCsvReceived(response);
                    break;

                case "error":
                    Debug.LogError($"[CalendarClient] サーバーエラー: {response.message}");
                    break;

                default:
                    Debug.Log($"[CalendarClient] 未処理メッセージタイプ: {response.type}");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[CalendarClient] メッセージ解析エラー: {e.Message}");
        }
    }

    // ===== CSV受信・保存処理 =====
    void HandleCalendarCsvReceived(sabaresuponsu response)
    {
        try
        {
            if (string.IsNullOrEmpty(response.csv_data))
            {
                Debug.LogError("[CalendarClient] CSVデータが空です");
                return;
            }

            // Base64デコード
            byte[] csvBytes = Convert.FromBase64String(response.csv_data);
            string csvContent = Encoding.UTF8.GetString(csvBytes);

            // 保存先パスを決定
            string savePath = GetSavePath();

            // ディレクトリ作成（存在しない場合）
            string directory = System.IO.Path.GetDirectoryName(savePath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            // ファイル保存
            System.IO.File.WriteAllText(savePath, csvContent, Encoding.UTF8);

            Debug.Log($"[CalendarClient] CSV保存成功: {savePath}");
            Debug.Log($"[CalendarClient] ファイルサイズ: {csvBytes.Length} bytes");
            Debug.Log($"[CalendarClient] CSV内容プレビュー:\n{GetPreview(csvContent)}");

            // イベント通知（他のスクリプトで購読可能）
            OnCalendarCsvUpdated?.Invoke(savePath, csvContent);
        }
        catch (Exception e)
        {
            Debug.LogError($"[CalendarClient] CSV処理エラー: {e.Message}");
            Debug.LogError(e.StackTrace);
        }
    }

    // ===== ヘルパーメソッド =====
    string GetSavePath()
    {
        if (saveToStreamingAssets)
        {
            // StreamingAssetsに保存（Editor限定、ビルド後は読み取り専用）
#if UNITY_EDITOR
            string path = System.IO.Path.Combine(Application.streamingAssetsPath, csvFileName);
            Debug.Log($"[CalendarClient] StreamingAssetsに保存: {path}");
            return path;
#else
            Debug.LogWarning("[CalendarClient] StreamingAssetsはビルド後は読み取り専用です。persistentDataPathに保存します");
            return System.IO.Path.Combine(Application.persistentDataPath, csvFileName);
#endif
        }
        else
        {
            // persistentDataPathに保存（推奨）
            return System.IO.Path.Combine(Application.persistentDataPath, csvFileName);
        }
    }

    string GetPreview(string content, int maxLines = 5)
    {
        string[] lines = content.Split('\n');
        int previewLines = Mathf.Min(lines.Length, maxLines);
        return string.Join("\n", lines, 0, previewLines);
    }

    // ===== パブリックプロパティ =====
    public bool IsConnected => isConnected;

    public string SavedCsvPath => GetSavePath();

    // ===== イベント =====
    public delegate void CalendarCsvUpdatedHandler(string filePath, string csvContent);
    public event CalendarCsvUpdatedHandler OnCalendarCsvUpdated;

    // ===== クリーンアップ =====
    async void OnApplicationQuit()
    {
        if (websocket != null)
        {
            await websocket.Close();
        }
    }

    void OnDestroy()
    {
        if (websocket != null)
        {
            websocket.Close();
        }
    }

    // ===== デバッグ用パブリックメソッド =====
    public string LoadSavedCsv()
    {
        string path = GetSavePath();
        if (System.IO.File.Exists(path))
        {
            return System.IO.File.ReadAllText(path, Encoding.UTF8);
        }
        else
        {
            Debug.LogWarning($"[CalendarClient] CSVファイルが見つかりません: {path}");
            return null;
        }
    }
}
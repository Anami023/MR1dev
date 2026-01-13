using UnityEngine;
using TMPro; // TextMeshProを使用するために必要
using System;

public class SearchManager : MonoBehaviour
{
    // インスペクターからInputFieldを紐付けます
    [SerializeField] private TMP_InputField searchInputField;

    public void OpenKeyboard()
    {
        TouchScreenKeyboard.Open(searchInputField.text, TouchScreenKeyboardType.Default);
    }


    /// <summary>
    /// ボタンが押された時に呼び出す関数
    /// </summary>
    public void ExecuteSearch()
    {
        string url = "https://www.google.com/?zx=1768286493511&no_sw_cr=1";
        Application.OpenURL(url);
        string query = searchInputField.text;

        /*if (!string.IsNullOrEmpty(query))
        {
            // 日本語やスペースをURLで使える形式（%エンコード）に変換
            string encodedQuery = Uri.EscapeDataString(query);
            
            // Google検索のURLを作成
            
            "https://www.google.com/search?q=" + encodedQuery;

            // システムブラウザを起動してURLを開く
            Application.OpenURL(url);
            
            Debug.Log($"ブラウザを起動しました: {url}");
        }
        else
        {
            Debug.LogWarning("検索ワードが空です");
        }*/
    }
}

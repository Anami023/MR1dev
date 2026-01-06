using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;

//[ExecuteAlways]
public class daymanager : MonoBehaviour
{
    public int index;
    int date;
    public DateTime today;
    public int year, month;

    public Material highlightring;
    bool isCurrentMonth;

    // Start is called before the first frame update
    public void Start()
    {

    }
    public void GenerateDate(int year_in, int month_in)
    {
        // ClearHighlight();

        year = year_in;
        month = month_in;

        UpdateDate(year, month);
    }
    public void UpdateDate(int year, int month)
    {

        bool last_month = false, next_month = false;
        int days = DateTime.DaysInMonth(year, month);
        int days_before = (month == 1) ? DateTime.DaysInMonth(year - 1, 12)
                                       : DateTime.DaysInMonth(year, month - 1);

        int firstDayOfWeek = (int)new DateTime(year, month, 1).DayOfWeek;

        int startIndex = firstDayOfWeek;
        int endIndex = startIndex + days - 1;

        int date;
        int target_year = year;
        int target_month = month;

        if (index < startIndex)
        {
            last_month = true;
            target_month = month - 1;
            if (target_month == 0)
            {
                target_month = 12;
                target_year--;
            }
            int diff = startIndex - index;
            date = days_before - diff + 1;
        }
        else if (index > endIndex)
        {
            next_month = true;
            target_month = month + 1;
            if (target_month == 13)
            {
                target_month = 1;
                target_year++;
            }
            date = index - endIndex;
        }
        else
        {
            date = index - startIndex + 1;
        }

        today = new DateTime(target_year, target_month, date);

        TextMeshPro text = GetComponentInChildren<TextMeshPro>();
        text.text = date.ToString();

        if (today.DayOfWeek == DayOfWeek.Sunday)
        {
            text.faceColor = new Color32(255, 0, 0, 255);
        }
        else if (today.DayOfWeek == DayOfWeek.Saturday)
        {
            text.faceColor = new Color32(0, 0, 255, 255);
        }
        else
        {
            text.faceColor = new Color32(0, 0, 0, 255);
        }

        if (last_month || next_month)
        {
            text.faceColor = new Color32(text.faceColor.r, text.faceColor.g, text.faceColor.b, 127);
        }
        isCurrentMonth = !(last_month || next_month);

    }
    // public void highlight(List<string[]> csvData)
    // {
    //     // ★ まず必ず消す
    //     RemoveHighlightMaterial();

    //     if (!isCurrentMonth) return;
    //     if (csvData == null) return;

    //     bool isToday = csvData.Any(row =>
    //     {
    //         if (row == null || row.Length < 2) return false;

    //         if (DateTime.TryParseExact(
    //                 row[1],
    //                 "yyyy/MM/dd/HH:mm",
    //                 null,
    //                 System.Globalization.DateTimeStyles.None,
    //                 out DateTime deadline))
    //         {
    //             return deadline.Date == today.Date;
    //         }
    //         return false;
    //     });

    //     if (!isToday) return;

    //     // ここで付け直す
    //     Renderer rend = GetComponent<Renderer>();
    //     Material[] mats = rend.materials;

    //     if (mats.Length == 1)
    //     {
    //         rend.materials = new Material[]
    //         {
    //             mats[0],
    //             highlightring
    //         };
    //     }
    // }
    public void highlight(List<string[]> csvData)
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend == null) return;

        var mats = new List<Material>(rend.materials);

        bool match =
            csvData != null &&
            csvData.Any(row =>
            {
                if (row == null || row.Length < 2) return false;

                if (DateTime.TryParseExact(
                        row[1],
                        "yyyy/MM/dd/HH:mm",
                        null,
                        System.Globalization.DateTimeStyles.None,
                        out DateTime deadline))
                {
                    return deadline.Date == today.Date;
                }
                return false;
            });

        // ===== ハイライト付与 =====
        if (match)
        {
            if (mats.Count == 1)
            {
                mats.Add(highlightring);   // ★必ず2番目
                rend.materials = mats.ToArray();
            }
        }
        // ===== ハイライト除去 =====
        else
        {
            if (mats.Count >= 2)
            {
                mats.RemoveAt(1);          // ★2番目を強制削除
                rend.materials = mats.ToArray();
            }
        }

        //Debug.Log($"{name} {today:yyyy/MM/dd} match={match} mats={mats.Count}");
    }



    void RemoveHighlightMaterial()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend == null) return;

        Material[] mats = rend.materials;

        // マテリアルが2つ以上なら、2番目を削除
        if (mats.Length >= 2)
        {
            Material[] newMats = new Material[1];
            newMats[0] = mats[0];
            rend.materials = newMats;
        }
    }



}

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
    // Start is called before the first frame update
    public void Start()
    {

    }
    public void GenerateDate(int year_in, int month_in)
    {
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
    }
    public void highlight(List<string[]> csvData)
    {


        if (csvData.Any(row =>
        {
            string raw = row[1];

            if (DateTime.TryParseExact(
                    raw,
                    "yyyy/MM/dd/HH:mm",
                    null,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime deadline))
            {
                return deadline.Date == today.Date;
            }

            return false;
        }))
        {
            Renderer rend = this.GetComponent<Renderer>();

            Material[] mats = rend.materials;
            Material[] newMats = new Material[mats.Length + 1];

            for (int i = 0; i < mats.Length; i++)
            {
                newMats[i] = mats[i];
            }

            newMats[mats.Length] = highlightring;
            rend.materials = newMats;
        }

    }
}

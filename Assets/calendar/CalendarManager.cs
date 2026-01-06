using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CalendarManager : MonoBehaviour
{
    private List<string[]> csvData;
    ScheduleReader sr;
    void Awake()
    {
        sr = GetComponent<ScheduleReader>();
        if (sr == null)
        {
            Debug.LogError("ScheduleReader が付いていません");
        }
    }

    void Start()
    {

        csvData = this.GetComponent<ScheduleReader>().csvData;
        sr = this.GetComponent<ScheduleReader>();
        //Debug.Log(sr);
        foreach (daymanager d in FindObjectsOfType<daymanager>())
        {
            d.GenerateDate(ReceiveYear(), ReceiveMonth());
            d.highlight(csvData);
        }

        var headerObj = FindObjectOfType<header>();
        if (headerObj != null)
        {
            headerObj.UpdateOwn(ReceiveYear(), ReceiveMonth());
        }

        Debug.Log($"[CalendarManager] year={ReceiveYear()}, month={ReceiveMonth()}");
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SwitchP()
    {
        sr = this.GetComponent<ScheduleReader>();
        csvData = sr.LoadFromPersistent();
        if (ReceiveMonth() == 1)
        {
            UpdateTime(ReceiveYear() - 1, 12);
        }
        else
        {
            UpdateTime(ReceiveYear(), ReceiveMonth() - 1);
        }

        foreach (daymanager d in FindObjectsOfType<daymanager>())
        {
            d.GenerateDate(ReceiveYear(), ReceiveMonth());
            d.highlight(csvData);

        }
        FindObjectOfType<header>().UpdateOwn(ReceiveYear(), ReceiveMonth());
        Debug.Log($"{ReceiveYear()}/{ReceiveMonth()}");

    }
    public void SwitchN()
    {
        sr = this.GetComponent<ScheduleReader>();
        //Debug.Log(sr);
        csvData = sr.LoadFromPersistent();
        if (ReceiveMonth() == 12)
        {
            UpdateTime(ReceiveYear() + 1, 1);
        }
        else
        {
            UpdateTime(ReceiveYear(), ReceiveMonth() + 1);
        }

        foreach (daymanager d in FindObjectsOfType<daymanager>())
        {
            d.GenerateDate(ReceiveYear(), ReceiveMonth());
            d.highlight(csvData);

        }
        FindObjectOfType<header>().UpdateOwn(ReceiveYear(), ReceiveMonth());
        // var headerObj = FindObjectOfType<header>();
        // if (headerObj != null)
        // {
        //     headerObj.UpdateOwn(ReceiveYear(), ReceiveMonth());
        // }
        // else
        // {
        //     Debug.LogError("header が見つかりません");
        // }
        Debug.Log($"{ReceiveYear()}/{ReceiveMonth()}");
    }
    // public void SwitchN()
    // {
    //     sr = this.GetComponent<ScheduleReader>();
    //     csvData = sr.LoadFromPersistent(); // データをロード

    //     // 年月の更新
    //     if (ReceiveMonth() == 12) UpdateTime(ReceiveYear() + 1, 1);
    //     else UpdateTime(ReceiveYear(), ReceiveMonth() + 1);

    //     int y = ReceiveYear();
    //     int m = ReceiveMonth();

    //     // ★修正ポイント：indexの順番に並び替えてから処理する
    //     var days = FindObjectsOfType<daymanager>().OrderBy(d => d.index);

    //     foreach (daymanager d in days)
    //     {
    //         d.GenerateDate(y, m);
    //         d.highlight(csvData); // 並び替えた順に確実に実行
    //     }

    //     FindObjectOfType<header>().UpdateOwn(y, m);
    //     Debug.Log($"{y}/{m} 更新完了 Data数:{csvData?.Count}");
    // }
    void UpdateTime(int year_in, int month_in)
    {
        FindObjectOfType<YearMonth>().Set(year_in, month_in);
    }
    int ReceiveYear()
    {
        return FindObjectOfType<YearMonth>().year;
    }
    int ReceiveMonth()
    {
        return FindObjectOfType<YearMonth>().month;
    }
}

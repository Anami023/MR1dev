using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CalendarManager : MonoBehaviour
{
    private List<string[]> csvData;
    void Awake()
    {

    }

    void Start()
    {
        csvData = this.GetComponent<ScheduleReader>().csvData;
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
        Debug.Log($"{ReceiveYear()}/{ReceiveMonth()}");
    }
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

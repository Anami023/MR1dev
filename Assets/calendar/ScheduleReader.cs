using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ScheduleReader : MonoBehaviour
{
    private TextAsset csvFile;
    public List<string[]> csvData = new List<string[]>();
    // Start is called before the first frame update
    void Awake()
    {
        csvFile = Resources.Load("schedule") as TextAsset;
        StringReader reader = new StringReader(csvFile.text);

        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            csvData.Add(line.Split(','));
        }
        for (int i = 0; i < csvData.Count; i++)
        {
            Debug.Log(csvData[i][0] + csvData[i][1] + csvData[i][2] + csvData[i][3] + csvData[i][4]);
        }
        //Debug.Log("executed");
    }

    // Update is called once per frame
    void Update()
    {

    }
}

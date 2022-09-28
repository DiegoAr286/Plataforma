using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.IO;
using UnityEngine;

public class FileManagerNBack : MonoBehaviour
{
    private List<int> trialNumber;
    private List<int> trialType;
    private List<int> score;
    private List<int> match;
    private List<int> miss;
    private List<int> falseAlarm;
    private List<double> reactionTime;
    private List<int> currentLetter;
    private List<int> nBack1;
    private List<int> nBack2;

    public string fileName = "NBack_File";
    private string path;

    // Start is called before the first frame update
    void Start()
    {
        fileName = PlayerPrefs.GetString("Name") + "_" + fileName;

        trialNumber = new List<int>();
        trialType = new List<int>();
        score = new List<int>();
        match = new List<int>();
        miss = new List<int>();
        falseAlarm = new List<int>();
        reactionTime = new List<double>(); // Tiempo hasta que se reacciona, -1 cuando no se reaccionó o no fue un trial de coincidencia
        currentLetter = new List<int>();
        nBack1 = new List<int>();
        nBack2 = new List<int>();
    }

    public void StoreDataInBuffer(int trialN, int trialT, int scored, int matched, int missed, int falseA, double reactionT, int currentL,
        int nback1, int nback2)
    {
        trialNumber.Add(trialN);
        trialType.Add(trialT);
        score.Add(scored);
        match.Add(matched);
        miss.Add(missed);
        falseAlarm.Add(falseA);
        reactionTime.Add(reactionT);
        currentLetter.Add(currentL);
        nBack1.Add(nback1);
        nBack2.Add(nback2);
    }

    public void WriteData()
    {
        path = "Assets/Resources/Files/NBack_Files/" + fileName + ".txt";
        if (!File.Exists(path))
        {
            Debug.Log(path);
            File.WriteAllText(path, "TrialNumber,TrialType,Score,Match,Miss,FalseAlarm,ReactionTime,CurrentLetter,NBack1,NBack2\n");
        }
        else
        {
            int i = 1;
            while (File.Exists(path))
            {
                path = "Assets/Resources/Files/NBack_Files/" + fileName + "_" + i.ToString() + ".txt";
                i++;
            }
            File.WriteAllText(path, "TrialNumber,TrialType,Score,Match,Miss,FalseAlarm,ReactionTime,CurrentLetter,NBack1,NBack2\n");
        }


        CultureInfo cult = CultureInfo.InvariantCulture;
        for (int i = 0; i < trialNumber.Count; i++)
        {
            File.AppendAllText(path, String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}\n",
                trialNumber[i].ToString(cult), trialType[i].ToString(cult), score[i].ToString(cult), match[i].ToString(cult), miss[i].ToString(cult),
                falseAlarm[i].ToString(cult), reactionTime[i].ToString(cult), currentLetter[i].ToString(cult), nBack1[i].ToString(cult), nBack2[i].ToString(cult)));
        }
    }
}

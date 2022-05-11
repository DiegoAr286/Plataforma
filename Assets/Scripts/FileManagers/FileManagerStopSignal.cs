using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.IO;
using UnityEngine;

public class FileManagerStopSignal : MonoBehaviour
{
    private List<int> trialNumber;
    private List<int> trialType;
    private List<int> requiredResponse; // 0 -> derecha, 1-> izquierda
    private List<int> stopSignalTime; // Tiempo hasta que aparece la stop sign, -1 cuando no es un trial con stop sign [ms]
    private List<int> match;
    private List<int> miss;
    private List<int> wrongKey;
    private List<double> reactionTime;

    public string fileName = "StopSignal_File";
    private string path;

    // Start is called before the first frame update
    void Start()
    {
        trialNumber = new List<int>();
        trialType = new List<int>();
        requiredResponse = new List<int>();
        stopSignalTime = new List<int>();
        match = new List<int>();
        miss = new List<int>();
        wrongKey = new List<int>();
        reactionTime = new List<double>();
    }

    
    public void StoreDataInBuffer(int trialN, int trialT, int reqResponse, int stopS, int matched, int missed, int falseA, double reactionT)
    {
        trialNumber.Add(trialN);
        trialType.Add(trialT);
        requiredResponse.Add(reqResponse);
        stopSignalTime.Add(stopS);
        match.Add(matched);
        miss.Add(missed);
        wrongKey.Add(falseA);
        reactionTime.Add(reactionT);
    }

    public void WriteData()
    {
        path = "Assets/Resources/Files/StopSignal_Files/" + fileName + ".txt";
        if (!File.Exists(path))
        {
            Debug.Log(path);
            File.WriteAllText(path, "TrialNumber,TrialType,RequiredResponse,StopSignalTime,Match,Miss,WrongArrow,ReactionTime\n");
        }
        else
        {
            int i = 1;
            while (File.Exists(path))
            {
                path = "Assets/Resources/Files/StopSignal_Files/" + fileName + "_" + i.ToString() + ".txt";
                i++;
            }
            File.WriteAllText(path, "TrialNumber,TrialType,RequiredResponse,StopSignalTime,Match,Miss,WrongArrow,ReactionTime\n");
        }


        CultureInfo cult = CultureInfo.InvariantCulture;
        for (int i = 0; i < trialNumber.Count; i++)
        {
            File.AppendAllText(path, String.Format("{0},{1},{2},{3},{4},{5},{6},{7}\n",
                trialNumber[i].ToString(cult), trialType[i].ToString(cult), requiredResponse[i].ToString(cult), stopSignalTime[i].ToString(cult), match[i].ToString(cult),
                miss[i].ToString(cult), wrongKey[i].ToString(cult), reactionTime[i].ToString(cult)));
        }
    }
}


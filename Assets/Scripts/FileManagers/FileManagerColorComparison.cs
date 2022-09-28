using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.IO;
using UnityEngine;

public class FileManagerColorComparison : MonoBehaviour
{
    private List<int> trialNumber;
    private List<int> squareQuantity;
    private List<int> side; // 1 -> izquierda; 0 -> derecha
    private List<int> matches; // 1 -> coincide; 0 -> no coincide
    private List<int> scores; // 1 -> correcto; 0 -> incorrecto


    public string fileName = "ColorComparison_File";
    private string path;

    // Start is called before the first frame update
    void Start()
    {
        fileName = PlayerPrefs.GetString("Name") + "_" + fileName;

        trialNumber = new List<int>();
        squareQuantity = new List<int>();
        side = new List<int>();
        matches = new List<int>();     
        scores = new List<int>();
    }


    public void StoreDataInBuffer(int trialN, int squareQ, int sides, int match, int score)
    {
        trialNumber.Add(trialN);
        squareQuantity.Add(squareQ);
        side.Add(sides);
        matches.Add(match);
        scores.Add(score);
    }

    public void WriteData()
    {
        path = "Assets/Resources/Files/ColorComparison_Files/" + fileName + ".txt";
        if (!File.Exists(path))
        {
            Debug.Log(path);
            File.WriteAllText(path, "TrialNumber,SquareQuantity,Side-L1,Matches,Score\n");
        }
        else
        {
            int i = 1;
            while (File.Exists(path))
            {
                path = "Assets/Resources/Files/ColorComparison_Files/" + fileName + "_" + i.ToString() + ".txt";
                i++;
            }
            File.WriteAllText(path, "TrialNumber,SquareQuantity,Side-L1,Matches,Score\n");
        }


        CultureInfo cult = CultureInfo.InvariantCulture;
        for (int i = 0; i < trialNumber.Count; i++)
        {
            File.AppendAllText(path, String.Format("{0},{1},{2},{3},{4}\n",
                trialNumber[i].ToString(cult), squareQuantity[i].ToString(cult), side[i].ToString(cult), matches[i].ToString(cult), scores[i].ToString(cult)));
        }
    }
}


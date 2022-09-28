using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.IO;
using UnityEngine;

public class FileManagerColorChangeDetection : MonoBehaviour
{
    private List<int> trialNumber;
    private List<int> squareQuantity;
    private List<int> side; // 1 -> izquierda; 0 -> derecha
    private List<int> matches;
    private List<int> mistakes;

    public string fileName = "ColorChangeDetection_File";
    private string path;

    // Start is called before the first frame update
    void Start()
    {
        fileName = PlayerPrefs.GetString("Name") + "_" + fileName;

        trialNumber = new List<int>();
        squareQuantity = new List<int>();
        side = new List<int>();
        matches = new List<int>();
        mistakes = new List<int>();
        
    }


    public void StoreDataInBuffer(int trialN, int squareQ, int sides, int match, int mistake)
    {
        trialNumber.Add(trialN);
        squareQuantity.Add(squareQ);
        side.Add(sides);
        matches.Add(match);
        mistakes.Add(mistake);
    }

    public void WriteData()
    {
        path = "Assets/Resources/Files/ColorChangeDetection_Files/" + fileName + ".txt";
        if (!File.Exists(path))
        {
            Debug.Log(path);
            File.WriteAllText(path, "TrialNumber,SquareQuantity,Side-L1,Matches,Mistakes\n");
        }
        else
        {
            int i = 1;
            while (File.Exists(path))
            {
                path = "Assets/Resources/Files/ColorChangeDetection_Files/" + fileName + "_" + i.ToString() + ".txt";
                i++;
            }
            File.WriteAllText(path, "TrialNumber,SquareQuantity,Side-L1,Matches,Mistakes\n");
        }


        CultureInfo cult = CultureInfo.InvariantCulture;
        for (int i = 0; i < trialNumber.Count; i++)
        {
            File.AppendAllText(path, String.Format("{0},{1},{2},{3},{4}\n",
                trialNumber[i].ToString(cult), squareQuantity[i].ToString(cult), side[i].ToString(cult), matches[i].ToString(cult), mistakes[i].ToString(cult)));
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.IO;
using UnityEngine;
using System.Drawing;

public class FileManagerFN : MonoBehaviour
{
    public HapticPlugin omni;

    private List<Vector3> stylusPosition;
    private List<Vector3> stylusVelocity;

    private List<float> timeVector;

    private Dictionary<int, int> triggerVector;

    private Dictionary<int, int> trialVector;

    private Dictionary<int, int> scoreVector;

    private Dictionary<int, int> cutVector;

    private Dictionary<int, string> fruitVector;

    private int rightHand;

    //private int frameCounter = 0;
    [Range(0, 100)] public int frameCounterLimit = 0;

    private float currentTime;

    private string path;

    private int trialN = 0;

    // Start is called before the first frame update
    void Start()
    {
        path = PlayerPrefs.GetString("Name");

        rightHand = PlayerPrefs.GetInt("RightHand");

        stylusPosition = new List<Vector3>();
        stylusVelocity = new List<Vector3>();
        timeVector = new List<float>();

        triggerVector = new Dictionary<int, int>();
        trialVector = new Dictionary<int, int>();
        scoreVector = new Dictionary<int, int>();
        cutVector = new Dictionary<int, int>();

        fruitVector = new Dictionary<int, string>();
    }

    // Update is called once per frame
    void Update()
    {
        currentTime = Time.timeSinceLevelLoad;

        StoreDataInBuffer();
    }

    void StoreDataInBuffer()
    {
        //stylusPosition.Add(omni.CurrentPosition);
        //stylusVelocity.Add(omni.CurrentVelocity);
        timeVector.Add(currentTime);
    }

    public void StoreTrigger(int trigger)
    {
        if (triggerVector.ContainsKey(stylusPosition.Count - 1))
            return;
        triggerVector.Add(stylusPosition.Count - 1, trigger);
    }

    public void StoreTrial()
    {
        if (trialVector.ContainsKey(stylusPosition.Count - 1))
            return;
        trialVector.Add(stylusPosition.Count - 1, trialN);
        trialN++;
    }

    public void StoreFruit(string fruit)
    {
        if (fruitVector.ContainsKey(stylusPosition.Count - 1))
            return;
        fruitVector.Add(stylusPosition.Count - 1, fruit);
    }
    public void StoreScore(int score)
    {
        if (fruitVector.ContainsKey(stylusPosition.Count - 1))
            return;

        scoreVector.Add(stylusPosition.Count - 1, score);
    }

    public void StoreCut()
    {
        if (cutVector.ContainsKey(stylusPosition.Count - 1))
            return;
        cutVector.Add(stylusPosition.Count - 1, 1);
    }


    public void WriteData()
    {
        File.WriteAllText(path, "Position_x,Position_y,Position_z,Velocity_x,Velocity_y,Velocity_z,Time,Trigger,Trial,Score,Fruit,RightHand\n");
        
        trialN = 0;
        int trigger;
        int trial;
        int score;
        int cut;
        string fruit;

        CultureInfo cult = CultureInfo.InvariantCulture;
        for (int i = 0; i < stylusPosition.Count; i++)
        {
            if (triggerVector.ContainsKey(i))
                trigger = triggerVector[i];
            else
                trigger = -1;

            if (trialVector.ContainsKey(i))
                trial = trialVector[i];
            else
                trial = -1;

            if (scoreVector.ContainsKey(i))
                score = scoreVector[i];
            else
                score = -1;

            if (fruitVector.ContainsKey(i))
                fruit = fruitVector[i];
            else
                fruit = "-1";

            if (cutVector.ContainsKey(i))
                cut = cutVector[i];
            else
                cut = -1;


            File.AppendAllText(path, String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}\n",
                stylusPosition[i].x.ToString(cult), stylusPosition[i].y.ToString(cult), stylusPosition[i].z.ToString(cult),
                stylusVelocity[i].x.ToString(cult), stylusVelocity[i].y.ToString(cult), stylusVelocity[i].z.ToString(cult),
                timeVector[i].ToString(cult), trigger.ToString(), trial.ToString(), score.ToString(), fruit.ToString(), rightHand.ToString()));
        }
    }
}

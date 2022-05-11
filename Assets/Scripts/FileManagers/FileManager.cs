using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.IO;
using UnityEngine;

public class FileManager : MonoBehaviour
{
    public HapticPlugin omni;

    private List<Vector3> stylusPosition;
    private List<Vector3> stylusVelocity;

    private List<float> timeVector;
    private Dictionary<int, string> triggerVector;

    private Dictionary<int, int> trialVector;


    //private int frameCounter = 0;
    [Range(0, 100)] public int frameCounterLimit = 0;

    private float currentTime;

    public string fileName = "NHPT_File";
    private string path;

    // Start is called before the first frame update
    void Start()
    {
        stylusPosition = new List<Vector3>();
        stylusVelocity = new List<Vector3>();
        timeVector = new List<float>();
        triggerVector = new Dictionary<int, string>();
        trialVector = new Dictionary<int, int>();
    }

    // Update is called once per frame
    void Update()
    {
        //currentTime = Time.realtimeSinceStartup;
        currentTime = Time.timeSinceLevelLoad;
        //currentTime = cueManager.totalCurrentTime;

        //Debug.Log(proxyPosition);
        //if (frameCounter == frameCounterLimit)
        //{
        StoreDataInBuffer();
        //frameCounter = 0;
        //}
        //frameCounter++;
    }

    void StoreDataInBuffer()
    {
        stylusPosition.Add(omni.stylusPositionRaw);
        stylusVelocity.Add(omni.stylusVelocityRaw);
        timeVector.Add(currentTime);
    }

    public void StoreTrigger(int trigger)
    {
        switch (trigger)
        {
            case 1:
                triggerVector.Add(stylusPosition.Count - 1, "T1");
                break;
            case 2:
                triggerVector.Add(stylusPosition.Count - 1, "T2");
                break;
        }
    }

    public void StoreTrialOutcome(int trial)
    {
        trialVector.Add(stylusPosition.Count - 1, trial);
    }

    public void WriteData()
    {
        path = "Assets/Resources/Files/" + fileName + ".txt";
        if (!File.Exists(path))
        {
            Debug.Log(path);
            File.WriteAllText(path, "Position_x,Position_y,Position_z,Velocity_x,Velocity_y,Velocity_z,Time,Trigger,Trial\n");
        }
        else
        {
            int i = 1;
            while (File.Exists(path))
            {
                path = "Assets/Resources/Files/" + fileName + "_" + i.ToString() + ".txt";
                i++;
            }
            File.WriteAllText(path, "Position_x,Position_y,Position_z,Velocity_x,Velocity_y,Velocity_z,Time,Trigger,Trial\n");
        }


        int noTrigger = 0;
        int noTrial = 0;
        CultureInfo cult = CultureInfo.InvariantCulture;
        for (int i = 0; i < stylusPosition.Count; i++)
        {
            if (triggerVector.ContainsKey(i))
            {
                File.AppendAllText(path, String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}\n",
                    stylusPosition[i].x.ToString(cult), stylusPosition[i].y.ToString(cult), stylusPosition[i].z.ToString(cult),
                    stylusVelocity[i].x.ToString(cult), stylusVelocity[i].y.ToString(cult), stylusVelocity[i].z.ToString(cult),
                    timeVector[i].ToString(cult), triggerVector[i], noTrial.ToString()));
            }

            if (trialVector.ContainsKey(i))
            {
                File.AppendAllText(path, String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}\n",
                    stylusPosition[i].x.ToString(cult), stylusPosition[i].y.ToString(cult), stylusPosition[i].z.ToString(cult),
                    stylusVelocity[i].x.ToString(cult), stylusVelocity[i].y.ToString(cult), stylusVelocity[i].z.ToString(cult),
                    timeVector[i].ToString(cult), noTrigger.ToString(), trialVector[i]));
            }

            if (!triggerVector.ContainsKey(i) && !trialVector.ContainsKey(i))
            {
                File.AppendAllText(path, String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}\n",
                    stylusPosition[i].x.ToString(cult), stylusPosition[i].y.ToString(cult), stylusPosition[i].z.ToString(cult),
                    stylusVelocity[i].x.ToString(cult), stylusVelocity[i].y.ToString(cult), stylusVelocity[i].z.ToString(cult),
                    timeVector[i].ToString(cult), noTrigger.ToString(), noTrial.ToString()));
            }

            //if (trialVector.ContainsKey(i))
            //    File.AppendAllText(path, String.Format("{0}\n", trialVector[i]));
            //else
            //    File.AppendAllText(path, String.Format("{0}\n", noTrial.ToString()));
        }
    }
}

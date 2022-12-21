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
    private Dictionary<int, int> triggerVector;

    private Dictionary<int, int> trialVector;

    private Dictionary<int, int> grabVector;

    //private int frameCounter = 0;
    [Range(0, 100)] public int frameCounterLimit = 0;

    private float currentTime;

    private string path;

    // Start is called before the first frame update
    void Start()
    {
        path = PlayerPrefs.GetString("Name");

        stylusPosition = new List<Vector3>();
        stylusVelocity = new List<Vector3>();
        timeVector = new List<float>();
        triggerVector = new Dictionary<int, int>();
        trialVector = new Dictionary<int, int>();
        grabVector = new Dictionary<int, int>();
    }

    // Update is called once per frame
    void Update()
    {
        currentTime = Time.timeSinceLevelLoad;

        StoreDataInBuffer();

    }

    void StoreDataInBuffer()
    {
        stylusPosition.Add(omni.stylusPositionRaw);
        stylusVelocity.Add(omni.stylusVelocityRaw);
        timeVector.Add(currentTime);
    }

    public void StoreTrigger(int trigger)
    {
        triggerVector.Add(stylusPosition.Count - 1, trigger);
    }

    public void StoreTrialOutcome(int trial)
    {
        trialVector.Add(stylusPosition.Count - 1, trial);
    }

    public void StoreGrabMoment()
    {
        grabVector.Add(stylusPosition.Count - 1, 1);
    }

    public void WriteData()
    {
        File.WriteAllText(path, "Position_x,Position_y,Position_z,Velocity_x,Velocity_y,Velocity_z,Time,Trigger,Trial,Grab\n");

        //int noTrigger = -1;
        //int noTrial = -1;
        //int noGrab = -1;

        int trigger;
        int trial;
        int grab;

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

            if (grabVector.ContainsKey(i))
                grab = grabVector[i];
            else
                grab = -1;


            File.AppendAllText(path, String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}\n",
                stylusPosition[i].x.ToString(cult), stylusPosition[i].y.ToString(cult), stylusPosition[i].z.ToString(cult),
                stylusVelocity[i].x.ToString(cult), stylusVelocity[i].y.ToString(cult), stylusVelocity[i].z.ToString(cult),
                timeVector[i].ToString(cult), trigger.ToString(), trial.ToString(), grab.ToString()));
        }
    }
}

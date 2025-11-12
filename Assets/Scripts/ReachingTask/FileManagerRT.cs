using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.IO;
using UnityEngine;
using System.Drawing;

public class FileManagerRT : MonoBehaviour
{
    public HapticPlugin omni;

    private List<Vector3> stylusPosition;
    private List<Vector3> stylusVelocity;

    private List<float> timeVector;

    private Dictionary<int, int> triggerVector;

    private Dictionary<int, int> trialVector;

    private Dictionary<int, int> sphereVector;

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
        Debug.Log(path);

        rightHand = PlayerPrefs.GetInt("RightHand");

        stylusPosition = new List<Vector3>();
        stylusVelocity = new List<Vector3>();
        timeVector = new List<float>();

        triggerVector = new Dictionary<int, int>();
        sphereVector = new Dictionary<int, int>();
        trialVector = new Dictionary<int, int>();
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
        if (triggerVector.ContainsKey(stylusPosition.Count - 1))
            return;
        triggerVector.Add(stylusPosition.Count - 1, trigger);
    }

    public void StoreSphere(int sphere)
    {
        if (sphereVector.ContainsKey(stylusPosition.Count - 1))
            return;
        sphereVector.Add(stylusPosition.Count - 1, sphere);
    }

    public void StoreTrial()
    {
        if (trialVector.ContainsKey(stylusPosition.Count - 1))
            return;
        trialVector.Add(stylusPosition.Count - 1, trialN);
        trialN++;
    }

    public void WriteData()
    {
        File.WriteAllText(path, "Position_x,Position_y,Position_z,Velocity_x,Velocity_y,Velocity_z,Time,Trigger,Trial,Sphere,RightHand\n");
        
        trialN = 0;
        int trigger;
        int trial;
        int sphere;

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

            if (sphereVector.ContainsKey(i))
                sphere = sphereVector[i];
            else
                sphere = -1;

            File.AppendAllText(path, String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}\n",
                stylusPosition[i].x.ToString(cult), stylusPosition[i].y.ToString(cult), stylusPosition[i].z.ToString(cult),
                stylusVelocity[i].x.ToString(cult), stylusVelocity[i].y.ToString(cult), stylusVelocity[i].z.ToString(cult),
                timeVector[i].ToString(cult), trigger.ToString(), trial.ToString(), sphere.ToString(), rightHand.ToString()));
        }
    }
}

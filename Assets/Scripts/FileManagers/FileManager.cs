using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.IO;
using UnityEngine;
using System.Drawing;

public class FileManager : MonoBehaviour
{
    public HapticPlugin omni;

    private List<Vector3> stylusPosition;
    private List<Vector3> stylusVelocity;

    private List<float> timeVector;

    private List<string> angleData;
    private List<float> angleTimeVector;

    private Dictionary<int, int> triggerVector;

    private Dictionary<int, int> trialVector;

    private Dictionary<int, int> grabVector;

    private Dictionary<int, int> pegVector;

    private Dictionary<int, int> holeVector;

    private int rightHand;

    //private int frameCounter = 0;
    [Range(0, 100)] public int frameCounterLimit = 0;

    private float currentTime;

    private string path;

    // Start is called before the first frame update
    void Start()
    {
        path = PlayerPrefs.GetString("Name");

        rightHand = PlayerPrefs.GetInt("RightHand_NHPT");

        stylusPosition = new List<Vector3>();
        stylusVelocity = new List<Vector3>();
        timeVector = new List<float>();

        angleData = new List<string>();
        angleTimeVector = new List<float>();

        triggerVector = new Dictionary<int, int>();
        trialVector = new Dictionary<int, int>();
        grabVector = new Dictionary<int, int>();

        pegVector = new Dictionary<int, int>();
        holeVector = new Dictionary<int, int>();
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

    public void StoreAngleData(string angle)
    {
        angleData.Add(angle);
        angleTimeVector.Add(currentTime);
    }

    public void StoreTrigger(int trigger)
    {
        triggerVector.Add(stylusPosition.Count - 1, trigger);
    }

    public void StoreTrialOutcome(int trial)
    {
        trialVector.Add(stylusPosition.Count - 1, trial);
    }

    public void StoreGrabMoment(int grab = 1)
    {
        grabVector.Add(stylusPosition.Count - 1, grab);
    }

    public void StorePeg(int peg)
    {
        pegVector.Add(stylusPosition.Count - 1, peg);
    }

    public void StoreHole(int hole)
    {
        holeVector.Add(stylusPosition.Count - 1, hole);
    }

    public void WriteData()
    {
        File.WriteAllText(path, "Position_x,Position_y,Position_z,Velocity_x,Velocity_y,Velocity_z,Time,Trigger,Trial,Grab,Angle,AngleTime,Peg,Hole,RightHand\n");

        int trigger;
        int trial;
        int grab;

        int peg;
        int hole;

        string angle;
        float angleTime;

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

            if (holeVector.ContainsKey(i))
                hole = holeVector[i];
            else
                hole = -1;

            if (pegVector.ContainsKey(i))
                peg = pegVector[i];
            else
                peg = -1;

            if (angleData.Count > i)
            {
                angle = angleData[i];
                angleTime = angleTimeVector[i];
            }
            else
            {
                angle = "null";
                angleTime = -1;
            }


            File.AppendAllText(path, String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}\n",
                stylusPosition[i].x.ToString(cult), stylusPosition[i].y.ToString(cult), stylusPosition[i].z.ToString(cult),
                stylusVelocity[i].x.ToString(cult), stylusVelocity[i].y.ToString(cult), stylusVelocity[i].z.ToString(cult),
                timeVector[i].ToString(cult), trigger.ToString(), trial.ToString(), grab.ToString(), angle, angleTime.ToString(cult),
                peg.ToString(), hole.ToString(), rightHand.ToString()));
        }
    }
}

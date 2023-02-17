using System.Collections;
using UnityEngine;

public class MessageListener : MonoBehaviour
{
    [HideInInspector] public FileManager fileManager;

    private void Start()
    {
        fileManager = GameObject.Find("FileManager").GetComponent<FileManager>();
    }

    // Invoked when a line of data is received from the serial device.
    void OnMessageArrived(string msg)
    {
        fileManager.StoreAngleData(msg);
    }

    // Invoked when a connect/disconnect event occurs. The parameter 'success'
    // will be 'true' upon connection, and 'false' upon disconnection or
    // failure to connect.
    void OnConnectionEvent(bool success)
    {
        if (success)
            Debug.Log("Connection established");
        else
            Debug.Log("Connection attempt failed or disconnection detected");
    }
}

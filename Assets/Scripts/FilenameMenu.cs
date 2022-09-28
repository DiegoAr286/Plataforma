using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilenameMenu : MonoBehaviour
{
    private string volunteerName = "";

    public void GetVolunteerName(string newName)
    {
        volunteerName = newName;
    }

    public void SetVolunteerName()
    {
        PlayerPrefs.SetString("Name", volunteerName);
    }

}

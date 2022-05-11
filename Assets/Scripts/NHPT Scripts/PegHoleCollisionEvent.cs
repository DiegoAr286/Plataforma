using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
 
 public class PegHoleCollisionEvent : MonoBehaviour
{
    [HideInInspector] public UnityEvent<Collider> onTriggerEnter;
    [HideInInspector] public UnityEvent<Collider> onTriggerExit;

    void OnTriggerEnter(Collider col)
    {
        if (onTriggerEnter != null && col.gameObject.tag != "Player") onTriggerEnter.Invoke(col);
    }

    void OnTriggerExit(Collider col)
    {
        if (onTriggerExit != null && col.gameObject.tag != "Player") onTriggerExit.Invoke(col);
    }
}
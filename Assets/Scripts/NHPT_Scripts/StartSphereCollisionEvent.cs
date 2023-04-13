using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StartSphereCollisionEvent : MonoBehaviour
{
    [HideInInspector] public UnityEvent<Collider> onTriggerEnter;

    void OnTriggerEnter(Collider col)
    {
        if (onTriggerEnter != null && col.gameObject.tag == "Player") onTriggerEnter.Invoke(col);
    }

}

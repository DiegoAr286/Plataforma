using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using HapticPlugin;


//! This object can be applied to the stylus of a haptic device. 
//! It allows you to pick up simulated objects and feel the involved physics.
//! Optionally, it can also turn off physics interaction when nothing is being held.
public class HapticGrabberHandSP : MonoBehaviour 
{
	public int buttonID = 0;		//!< index of the button assigned to grabbing.  Defaults to the first button
	public bool ButtonActsAsToggle = false;	//!< Toggle button? as opposed to a press-and-hold setup?  Defaults to off.
	public enum PhysicsToggleStyle{ none, onTouch, onGrab };
	public PhysicsToggleStyle physicsToggleStyle = PhysicsToggleStyle.none;   //!< Should the grabber script toggle the physics forces on the stylus? 

	public bool DisableUnityCollisionsWithTouchableObjects = true;

	private GameObject hapticDevice = null;   //!< Reference to the GameObject representing the Haptic Device
	private bool buttonStatus = false;			//!< Is the button currently pressed?
	private GameObject touching = null;			//!< Reference to the object currently touched
	private GameObject grabbing = null;			//!< Reference to the object currently grabbed
	private FixedJoint joint = null;            //!< The Unity physics joint created between the stylus and the object being grabbed.

	public GameObject grabberParent; // Used to change the gripper (hand) model
	private int openHand; // When =1, activates the model of the open hand, when =2, activates the closed hand
	private bool hand = false; // true = right hand


	//! Automatically called for initialization
	void Start () 
	{
		if (hapticDevice == null)
		{

			HapticPlugin[] HPs = (HapticPlugin[])Object.FindObjectsOfType(typeof(HapticPlugin));
			foreach (HapticPlugin HP in HPs)
			{
				if (HP.hapticManipulator == this.gameObject)
				{
					hapticDevice = HP.gameObject;
				}
			}

		}

		//if ( physicsToggleStyle != PhysicsToggleStyle.none)
			hapticDevice.GetComponent<HapticPlugin>().PhysicsManipulationEnabled = false;

		if (DisableUnityCollisionsWithTouchableObjects)
			disableUnityCollisions();

		openHand = 1; // Mano derecha abierta inicialmente
		ModelSwitch(0);
		//grabberParent.transform.GetChild(1).gameObject.SetActive(false); // Desactiva los demás modelos
		//grabberParent.transform.GetChild(2).gameObject.SetActive(false);
		//grabberParent.transform.GetChild(3).gameObject.SetActive(false);

	}

	void Update()
	{

		// Change hand
		//if (Input.GetKeyDown("s"))
		//{
		//	ChangeHand();
		//}

	}

	void disableUnityCollisions()
	{
		GameObject[] touchableObjects;
		touchableObjects =  GameObject.FindGameObjectsWithTag("Touchable") as GameObject[];  //FIXME  Does this fail gracefully?

        // Ignore my collider
        Collider myC = gameObject.GetComponent<Collider>();
        if (myC != null)
            foreach (GameObject T in touchableObjects)
            {
                Collider CT = T.GetComponent<Collider>();
                if (CT != null)
                    Physics.IgnoreCollision(myC, CT);
            }

        // Ignore colliders in children.
        Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();
        foreach (Collider C in colliders)
            foreach (GameObject T in touchableObjects)
            {
                Collider CT = T.GetComponent<Collider>();
                if (CT != null)
                    Physics.IgnoreCollision(C, CT);
            }

    }

	
	//! Update is called once per frame
	void FixedUpdate () 
	{
		bool newButtonStatus = hapticDevice.GetComponent<HapticPlugin>().Buttons [buttonID] == 1;
		bool oldButtonStatus = buttonStatus;
		buttonStatus = newButtonStatus;


		if (oldButtonStatus == false && newButtonStatus == true)
		{
			if (ButtonActsAsToggle)
			{
				if (grabbing)
				{
					release();
					ModelSwitch(1);
				}
				else
				{
					grab();
					ModelSwitch(2);
				}
			} else
			{
				grab();
				ModelSwitch(2);
			}
		}
		if (oldButtonStatus == true && newButtonStatus == false)
		{
			if (ButtonActsAsToggle)
			{
				//Do Nothing
			} else
			{
				release();
				ModelSwitch(1);
			}
		}

		// Make sure haptics is ON if we're grabbing
		if( grabbing && physicsToggleStyle != PhysicsToggleStyle.none)
			hapticDevice.GetComponent<HapticPlugin>().PhysicsManipulationEnabled = true;
		if (!grabbing && physicsToggleStyle == PhysicsToggleStyle.onGrab)
			hapticDevice.GetComponent<HapticPlugin>().PhysicsManipulationEnabled = false;

		/*
		if (grabbing)
			hapticDevice.GetComponent<HapticPlugin>().shapesEnabled = false;
		else
			hapticDevice.GetComponent<HapticPlugin>().shapesEnabled = true;
			*/
			
	}


	public void ChangeHand()
    {
		hand = !hand; // Togglea entre mano izquierda y derecha
		ModelSwitch(0);
	}

	private void ModelSwitch(int modelN)
	{
		if (openHand != modelN && modelN != 0) // Si modelN es diferente (cambió de cerrada a abierta o viceversa) se setea openHand con su valor
		{
			openHand = modelN;
		}
		if (openHand == 1 && hand) // Mano derecha abierta
		{
			grabberParent.transform.GetChild(0).gameObject.SetActive(true);
			grabberParent.transform.GetChild(1).gameObject.SetActive(false);
			grabberParent.transform.GetChild(2).gameObject.SetActive(false);
			grabberParent.transform.GetChild(3).gameObject.SetActive(false);
		}
		else if (openHand == 2 && hand) // Mano derecha cerrada
		{
			grabberParent.transform.GetChild(0).gameObject.SetActive(false);
			grabberParent.transform.GetChild(1).gameObject.SetActive(true);
			grabberParent.transform.GetChild(2).gameObject.SetActive(false);
			grabberParent.transform.GetChild(3).gameObject.SetActive(false);
		}
		else if (openHand == 1 && !hand) // Mano izquierda abierta
		{
			grabberParent.transform.GetChild(0).gameObject.SetActive(false);
			grabberParent.transform.GetChild(1).gameObject.SetActive(false);
			grabberParent.transform.GetChild(2).gameObject.SetActive(true);
			grabberParent.transform.GetChild(3).gameObject.SetActive(false);
		}
		else if (openHand == 2 && !hand) // Mano izquierda cerrada
		{
			grabberParent.transform.GetChild(0).gameObject.SetActive(false);
			grabberParent.transform.GetChild(1).gameObject.SetActive(false);
			grabberParent.transform.GetChild(2).gameObject.SetActive(false);
			grabberParent.transform.GetChild(3).gameObject.SetActive(true);
		}
	}


	private void hapticTouchEvent( bool isTouch )
	{
		if (physicsToggleStyle == PhysicsToggleStyle.onGrab)
		{
			if (isTouch)
				hapticDevice.GetComponent<HapticPlugin>().PhysicsManipulationEnabled = true;
			else			
				return; // Don't release haptics while we're holding something.
		}
			
		if( physicsToggleStyle == PhysicsToggleStyle.onTouch )
		{
			hapticDevice.GetComponent<HapticPlugin>().PhysicsManipulationEnabled = isTouch;
			GetComponentInParent<Rigidbody>().velocity = Vector3.zero;
			GetComponentInParent<Rigidbody>().angularVelocity = Vector3.zero;

		}
	}

	void OnCollisionEnter(Collision collisionInfo)
	{
		Collider other = collisionInfo.collider;
		//Debug.unityLogger.Log("OnCollisionEnter : " + other.name);
		GameObject that = other.gameObject;
		Rigidbody thatBody = that.GetComponent<Rigidbody>();

		// If this doesn't have a rigidbody, walk up the tree. 
		// It may be PART of a larger physics object.
		while (thatBody == null)
		{
			//Debug.logger.Log("Touching : " + that.name + " Has no body. Finding Parent. ");
			if (that.transform == null || that.transform.parent == null)
				break;
			GameObject parent = that.transform.parent.gameObject;
			if (parent == null)
				break;
			that = parent;
			thatBody = that.GetComponent<Rigidbody>();
		}

		if( collisionInfo.rigidbody != null )
			hapticTouchEvent(true);

		if (thatBody == null)
			return;

		if (thatBody.isKinematic)
			return;
	
		touching = that;
	}
	void OnCollisionExit(Collision collisionInfo)
	{
		Collider other = collisionInfo.collider;
		//Debug.unityLogger.Log("onCollisionrExit : " + other.name);

		if( collisionInfo.rigidbody != null )
			hapticTouchEvent( false );

		if (touching == null)
			return; // Do nothing

		if (other == null ||
		    other.gameObject == null || other.gameObject.transform == null)
			return; // Other has no transform? Then we couldn't have grabbed it.

		if( touching == other.gameObject || other.gameObject.transform.IsChildOf(touching.transform))
		{
			touching = null;
		}
	}
		
	//! Begin grabbing an object. (Like closing a claw.) Normally called when the button is pressed. 
	void grab()
	{
		GameObject touchedObject = touching;
		if (touchedObject == null) // No Unity Collision? 
		{
			// Maybe there's a Haptic Collision
			touchedObject = hapticDevice.GetComponent<HapticPlugin>().touching;
		}

		if (grabbing != null) // Already grabbing
			return;
		if (touchedObject == null) // Nothing to grab
			return;

		// Grabbing a grabber is bad news.
		if (touchedObject.tag =="Gripper")
			return;

		Debug.Log( " Object : " + touchedObject.name + "  Tag : " + touchedObject.tag );

		grabbing = touchedObject;

		//Debug.logger.Log("Grabbing Object : " + grabbing.name);
		Rigidbody body = grabbing.GetComponent<Rigidbody>();

		// If this doesn't have a rigidbody, walk up the tree. 
		// It may be PART of a larger physics object.
		while (body == null)
		{
			//Debug.logger.Log("Grabbing : " + grabbing.name + " Has no body. Finding Parent. ");
			if (grabbing.transform.parent == null)
			{
				grabbing = null;
				return;
			}
			GameObject parent = grabbing.transform.parent.gameObject;
			if (parent == null)
			{
				grabbing = null;
				return;
			}
			grabbing = parent;
			body = grabbing.GetComponent<Rigidbody>();
		}

		joint = (FixedJoint)gameObject.AddComponent(typeof(FixedJoint));
		joint.connectedBody = body;
	}
	//! changes the layer of an object, and every child of that object.
	static void SetLayerRecursively(GameObject go, int layerNumber )
	{
		if( go == null ) return;
		foreach(Transform trans in go.GetComponentsInChildren<Transform>(true))
			trans.gameObject.layer = layerNumber;
	}

	//! Stop grabbing an object. (Like opening a claw.) Normally called when the button is released. 
	void release()
	{
		if( grabbing == null ) //Nothing to release
			return;


		Debug.Assert(joint != null);

		joint.connectedBody = null;
		Destroy(joint);



		grabbing = null;

		if (physicsToggleStyle != PhysicsToggleStyle.none)
			hapticDevice.GetComponent<HapticPlugin>().PhysicsManipulationEnabled = false;
	}

	//! Returns true if there is a current object. 
	public bool isGrabbing()
	{
		return (grabbing != null);
	}
}

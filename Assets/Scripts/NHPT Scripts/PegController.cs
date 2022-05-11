using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PegController : MonoBehaviour {

	public GameObject[] Pegs = {null, null, null, null};
	private Vector3[] PegPosition;
	private Quaternion[] PegRotation;


	// Remember the original positions of the pegs.
	void Start () 
	{
		PegPosition = new Vector3[Pegs.Length];
		PegRotation = new Quaternion[Pegs.Length];
		for (int ii = 0; ii < Pegs.Length; ii++)
		{
			PegPosition [ii] = Pegs [ii].transform.position;
			PegRotation [ii] = Pegs [ii].transform.rotation;
		}
	}

	// Return the blocks to their original position.
	void ResetPegs()
	{
		if (PegPosition.Length != Pegs.Length) return;

		for (int ii = 0; ii < Pegs.Length; ii++)
		{
			Pegs [ii].transform.SetPositionAndRotation(PegPosition[ii], PegRotation[ii]);
			Rigidbody RB = (Rigidbody)Pegs[ii].GetComponent(typeof(Rigidbody));
			if (RB)	RB.velocity = Vector3.zero;
			if (RB) RB.angularVelocity = Vector3.zero;
		}
	}


	// Update is called once per frame
	void Update () 
	{

		// Return to starting position?
		if (Input.GetKeyDown("space"))
		{
			ResetPegs();
			return;
		}

	}
}

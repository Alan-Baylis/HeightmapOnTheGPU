using UnityEngine;
using System.Collections;

public class CamShift : MonoBehaviour {

	Camera thisCam;
	float defaov = 0f;
	float lastShiftUp = 0f;
	// Use this for initialization
	void Start () 
	{
		thisCam = gameObject.GetComponent<Camera>();
		defaov = thisCam.fieldOfView;
	}
	
	// Update is called once per frame
	void Update () 
	{

		if(Input.GetKeyUp(KeyCode.LeftControl))
		{
			lastShiftUp = Time.time;
		}
		if(Input.GetKeyDown(KeyCode.LeftControl) && Time.time - lastShiftUp < 0.5f)
		{
			thisCam.fieldOfView = defaov;
		}


		if(Input.GetKey(KeyCode.LeftControl))
		{
			thisCam.fieldOfView =Mathf.Clamp(thisCam.fieldOfView + Input.mouseScrollDelta.y*3f, 1,179);
		}
		else if(!Input.GetKey(KeyCode.LeftShift))
		{
			transform.position += transform.forward*Input.mouseScrollDelta.y;
		}

	}
}

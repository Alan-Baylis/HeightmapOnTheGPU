using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class gravObjectController : MonoBehaviour 
{
	BufferHandlerPS1 buffHandler;
	Transform gravTran;
	MeshRenderer gravRend;
	public Text frameRate;
	// Use this for initialization
	void Start () 
	{
		buffHandler = Object.FindObjectOfType<BufferHandlerPS1>();
		if(buffHandler.gravityObject == null)
			Destroy(this);
		else
		{
			gravTran = buffHandler.gravityObject;
			gravRend = gravTran.gameObject.GetComponent<MeshRenderer>();
			gravRend.enabled = false;
		}
	}

	bool wasVisable = false;
	float distFromCam;

	List<int> lastFewFrames = new List<int>() {60,60,60 ,60,60,60 ,60,60,60 ,60,60,60 ,60,60,60 ,60,60,60 ,60,60,60};
	// Update is called once per frame
	void Update() 
	{
		if(Input.GetKeyDown(KeyCode.LeftShift))
		{
			wasVisable = gravRend.enabled;
			gravRend.enabled = true;
			distFromCam = Camera.main.WorldToScreenPoint(gravTran.position).z;
		}

		if(Input.GetKeyUp(KeyCode.LeftShift))
		{
			gravRend.enabled = wasVisable;
		}

		if(Input.GetKey(KeyCode.LeftShift))
		{
			distFromCam += Input.mouseScrollDelta.y*2.5f;
			Vector3 mousePos = Input.mousePosition;
			gravTran.position = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, distFromCam));
		}

		if(Input.GetKeyDown(KeyCode.Tab))
		{
			gravRend.enabled = !gravRend.enabled;
		}

		if(Input.GetKeyDown(KeyCode.Return))
		{
			Application.LoadLevel(Application.loadedLevel);
		}

		if(Input.GetKeyDown(KeyCode.PageUp))
		{
			Debug.Log("U");
			ParticleCountItterator.multiplier ++;
			Application.LoadLevel(Application.loadedLevel);
		}
		else
		if(Input.GetKeyDown(KeyCode.PageDown))
		{
			Debug.Log("D");
			ParticleCountItterator.multiplier --;
			if(ParticleCountItterator.multiplier < -14)
			{
				ParticleCountItterator.multiplier = -14;
			}
			Application.LoadLevel(Application.loadedLevel);
		}
		lastFewFrames.Add((int)(1.0f/Time.deltaTime));
		lastFewFrames.RemoveAt(0);
		int average = 0;
		foreach(int i in lastFewFrames)
			average += i;

		average /= lastFewFrames.Count;

		frameRate.text = average + "fps";
	}
}

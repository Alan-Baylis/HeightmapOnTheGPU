using UnityEngine;
using System.Collections;

public class PlayerHeightmapHandler : MonoBehaviour 
{
	public ImpactHandler imphand;
	
	public bool isCheckingHeight = false;
	public ComputeBuffer heightCheckBuffer;
	public float moveforce = 10;

	public float jumpForce = 50;
	public float jumpExplosionDuration = 2;
	float timeleft;
	Vector3 jumpStart;
	public float jumpExplosionRadius = 4;
	// Use this for initialization
	void Start () 
	{
		Cursor.lockState = CursorLockMode.Locked;
		heightCheckBuffer = new ComputeBuffer(1, sizeof(float)*3, ComputeBufferType.Default);

	}
	public bool isAboveGround
	{
		get{
			return (this.transform.position.y - curHeight - this.transform.localScale.y*0.5f) > 0;
		}
	}
	public bool isOnGround
	{
		get{
			return (this.transform.position.y - (this.transform.localScale.y*0.5f - 0.07f)) <= curHeight;
		}
	}

	public float curHeight;
	
	// Update is called once per frame
	void Update () 
	{
		Cursor.visible = false;

		if(this.isCheckingHeight)
		{
			imphand.compShader.SetTexture(imphand.compShader.FindKernel("CSHeightCheck"), "_CheckThisTexture", imphand.hTex[ImpactHandler.READ]);
			float[] checkedPosition = new float[3];
			checkedPosition[0] = this.transform.position.x;
			checkedPosition[1] = this.transform.position.z;
			checkedPosition[2] = -11;
			imphand.compShader.SetFloats("_CheckPosition", checkedPosition);
			imphand.compShader.SetBuffer(imphand.compShader.FindKernel("CSHeightCheck"), "_CheckedHeightData", heightCheckBuffer);
			imphand.compShader.Dispatch(imphand.compShader.FindKernel("CSHeightCheck"), 1, 1, 1);
			heightCheckBuffer.GetData(checkedPosition);

			curHeight = (checkedPosition[1] <= 1 && checkedPosition[1] >= 0 && checkedPosition[0] <= 1 && checkedPosition[0] >= 0 ) ? checkedPosition[2] : 6;
		}


		this.DoControls();

	}

	void DoControls()
	{
		float horz = Input.GetAxis("Horizontal");
		float vert = Input.GetAxis("Vertical");

		this.transform.position = Vector3.Lerp(this.transform.position, new Vector3(this.transform.position.x, curHeight + this.transform.localScale.y*0.5f-0.02f, this.transform.position.z), 0.5f);
	
		Vector3 moveInput = moveforce* horz*Vector3.Scale(Camera.main.transform.right, Vector3.one - Vector3.up).normalized +
							moveforce* vert*Vector3.Scale(Camera.main.transform.forward, Vector3.one - Vector3.up).normalized ;
		moveInput.y = 0;
		this.transform.position += moveInput;

	}

	void LateUpdate()
	{

	}


	void OnDestroy()
	{

		heightCheckBuffer.Release();
	}
}

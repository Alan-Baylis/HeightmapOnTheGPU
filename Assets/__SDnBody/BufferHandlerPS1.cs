using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BufferHandlerPS1 : MonoBehaviour
{
	public const int READ = 1;
	public const int WRITE = 0;
	public Material material;
	ComputeBuffer[] PosBuffer = new ComputeBuffer[2];
	ComputeBuffer[] VelBuffer = new ComputeBuffer[2];
	public ComputeShader cShade;
	public Transform gravityObject;
	


	public int count = 65536;//131072//262144//524288//1048576
	public float size = 5.0f;
	public float zSize = 20;
	public float gravity = 1;
	public float downDampening = 0.01f;
	public float upDampening = .02f;
	public float sphereRadius = 1;
	int sqrtCount;

	public const string _KernelName = "CSMain";//"CSTwirl";
	public float singularityStickDistance = 10f;

	public GameObject particleCountDisplayerTextObj;
	public static BufferHandlerPS1 inst;

	public float velModBase = 1f;
	float lastVelMod;
	bool isPause = true;
	//TODO: DO THE TWIST!!! FLIP A READ AND WRITE BUFFER
	void Start ()
	{
		//Application.targetFrameRate = 60;
		BufferHandlerPS1.inst = this;

		ParticleCountItterator.Init(this);
		particleCountDisplayerTextObj.GetComponent<Text>().text = this.count + "";

		int sqrtCount = (int)Mathf.Sqrt(count);
		PosBuffer[READ] = new ComputeBuffer (count, sizeof(float) * 3, ComputeBufferType.Default);
		VelBuffer[READ] = new ComputeBuffer (count, sizeof(float) * 3, ComputeBufferType.Default);
		PosBuffer[WRITE] = new ComputeBuffer (count, sizeof(float) * 3, ComputeBufferType.Default);
		VelBuffer[WRITE] = new ComputeBuffer (count, sizeof(float) * 3, ComputeBufferType.Default);

		Vector3[] points = new Vector3[count];
		Vector3[] velocities = new Vector3[count];

		Random.seed = 0;
		for (int i = 0; i < count; i++)
		{
			points[i] = new Vector3();
			points[i].x = Random.Range (-size, size);
			points[i].y = Random.Range (-size, size);
			points[i].z = Random.Range (-zSize, zSize);;
			velocities[i] = Vector3.zero;
			/*points[i].color = new Vector3();
			points[i].color.x = Random.value > 0.5f ? 0.0f : 1.0f;
			points[i].color.y = Random.value > 0.5f ? 0.0f : 1.0f;
			points[i].color.z = Random.value > 0.5f ? 0.0f : 1.0f;
*/

		}
		PosBuffer[READ].SetData (points);
		PosBuffer[WRITE].SetData (points);
		VelBuffer[READ].SetData (velocities);
		VelBuffer[WRITE].SetData (velocities);

		/*cShade.SetBuffer(cShade.FindKernel("CSMain"), "RvertPos", PosBuffer[READ]);
		cShade.SetBuffer(cShade.FindKernel("CSMain"), "RvertVel", VelBuffer[READ]);
		cShade.SetBuffer(cShade.FindKernel("CSMain"), "WvertPos", PosBuffer[WRITE]);
		cShade.SetBuffer(cShade.FindKernel("CSMain"), "WvertVel", VelBuffer[WRITE]);

		cShade.SetVector("_GravDampSphereDown", new Vector4(gravity, upDampening,sphereRadius, 0f ));

		cShade.Dispatch(cShade.FindKernel("CSMain"), sqrCount/8, sqrCount/8, 1);
*/
		lastVelMod = velModBase;
	}

	void Update()
	{
		Vector2 screnpont = Input.mousePosition;
		Vector3 singularity = Camera.main.ScreenToWorldPoint(new Vector3(screnpont.x, screnpont.y, singularityStickDistance));
		if(gravityObject != null)
			singularity = gravityObject.position;


		cShade.SetVector("_SingularityPosANDdt", new Vector4(singularity.x, singularity.y, singularity.z, Time.deltaTime));
		cShade.SetFloat("_Time",  Time.time);
		

		
		if(lastVelMod != velModBase && !isPause)
		{//catches if the velocity mod has been changed, then if it isn't paused sets the new data on the shader.
			lastVelMod = velModBase;
			cShade.SetVector("_GravDampSphereDown", new Vector4(gravity, downDampening,sphereRadius, velModBase ));//ISDOWN
		}
		//allows for starting and stopping with Space button, also controls the object's force of gravity
		if(Input.GetKeyDown(KeyCode.Space))
		{
			if(isPause)
			{// if it IS already paused and space is hit
				cShade.SetVector("_GravDampSphereDown", new Vector4(gravity, downDampening,sphereRadius, velModBase ));//ISDOWN
			}
			else
			{//if it is not pause and space is hit; pause all velocities
				cShade.SetVector("_GravDampSphereDown", new Vector4(gravity, upDampening,sphereRadius, 0f ));
			}
			isPause = !isPause;
		}
		


		cShade.SetBuffer(cShade.FindKernel(_KernelName), "RvertPos", PosBuffer[READ]);
		cShade.SetBuffer(cShade.FindKernel(_KernelName), "RvertVel", VelBuffer[READ]);
		cShade.SetBuffer(cShade.FindKernel(_KernelName), "WvertPos", PosBuffer[WRITE]);
		cShade.SetBuffer(cShade.FindKernel(_KernelName), "WvertVel", VelBuffer[WRITE]);
		cShade.Dispatch(cShade.FindKernel(_KernelName), count/32, 1, 1);

		Swap(PosBuffer);
		Swap(VelBuffer);



		if(!Input.GetKey(KeyCode.LeftShift))
		{
			this.singularityStickDistance -= Input.mouseScrollDelta.y;
//			Debug.Log(-Input.mouseScrollDelta.y +" singularity: " + this.singularityStickDistance);
		}
	}

	void Swap(ComputeBuffer[] buffer) 
	{
		ComputeBuffer tmp = buffer[READ];
		buffer[READ] = buffer[WRITE];
		buffer[WRITE] = tmp;
	}


	void OnRenderObject()
	{
		
		material.SetPass(0);
		material.SetBuffer("_VertPos", PosBuffer[READ]);
		Graphics.DrawProcedural(MeshTopology.Points, count, 1);
	}
	
	void OnDestroy()
	{
		PosBuffer[READ].Release();
		VelBuffer[READ].Release();
		PosBuffer[WRITE].Release();
		VelBuffer[WRITE].Release();

	}


	Vector3 spawnParticleSphere(float parSizeXY, float parSizeZ)
	{
				Vector3 newPont = new Vector3();
				newPont.x = Random.Range (-size, size);
				newPont.y = Random.Range (-size, size);
				newPont.z = Random.Range (-zSize, zSize);
				return newPont;
	}
/*
	Vector3 spawnParticleSquare(float parSizeXY, float parSizeZ)
	{

	}
*/
}
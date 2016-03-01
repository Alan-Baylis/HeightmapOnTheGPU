using UnityEngine;
using System.Collections;

public class ImageParticulate : MonoBehaviour 
{
	public Material m_particleMat;
	public ComputeShader cShader;
	ComputeBuffer buffer;
	pointVelUV[] data;
	public int particlesOnImageWidth = 8;
	int numParticles;

	public float MouseCastSize;

	void Start()
	{
		numParticles = particlesOnImageWidth*particlesOnImageWidth;
		numParticles = (numParticles/64)*numParticles;
		RunShader();
	}
	void RunShader()
	{

		//pointVelUV[] output = new pointVelUV[5];

		//INITIALIZE DATA HERE
		data = InitializeDataArray();
		//*8 because there are 8 bytes in the array of posveluv
		buffer = new ComputeBuffer(data.Length, sizeof(float)*8);
		int kernel = cShader.FindKernel("CSMain");
		cShader.SetBuffer(kernel, "dataBuffer", buffer);
		cShader.Dispatch(kernel, data.Length, 1,1);

	

	}

	pointVelUV[] InitializeDataArray()
	{
		pointVelUV[] pdata = new pointVelUV[numParticles];
		float ux,uy;
		for(int i =0; i < numParticles; i++)
		{
			ux = i%particlesOnImageWidth;
			uy = i/particlesOnImageWidth;
			ux /=(float)particlesOnImageWidth;
			uy /=(float)particlesOnImageWidth;

			pdata[i].uv = new Vector2(ux, uy);
			pdata[i].point = new Vector3(pdata[i].uv.x*2-1, pdata[i].uv.y*2-1, Random.value-0.5f);
			pdata[i].velocity = Vector3.zero;
			Debug.Log(pdata[i].point);
			//particlegrid
		}

		return pdata;
	}


	void Update()
	{
		if(Input.GetMouseButton(0))
		{
			Ray ra = Camera.main.ViewportPointToRay(Input.mousePosition);
			cShader.SetFloats("_MouseOrigin", new float[]{ra.origin.x, ra.origin.y, ra.origin.z});
			cShader.SetFloats("_MouseRay", new float[]{ra.direction.x, ra.direction.y, ra.direction.z});
		}
		else
		{
			cShader.SetFloats("_MouseOrigin", new float[]{-10000,-10000,-10000});
			cShader.SetFloats("_MouseRay", new float[]{-1,-1,-1});
		}
		int kernel = cShader.FindKernel("CSMain");

		cShader.SetFloat("_DeltaTime", Time.deltaTime);

		cShader.Dispatch(kernel, numParticles, 1, 1);
	}

	void OnPostRender () 
	{
		//buffer.GetData(data);

		m_particleMat.SetPass(0);
		m_particleMat.SetBuffer("_Data", buffer);
		
		Graphics.DrawProcedural(MeshTopology.Points, numParticles);
	}

	struct pointVelUV
	{
		public Vector3 point;
		public Vector3 velocity;
		public Vector2 uv;
	}
	void OnDestroy()
	{
		buffer.Release();
	}
}

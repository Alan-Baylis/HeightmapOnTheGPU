using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ParticleSystemsManager : MonoBehaviour
{
	public const int READ = 1;
	public const int WRITE = 0;
	ComputeBuffer[] DataBuffer = new ComputeBuffer[2];
	ComputeBuffer ParticlePropertyBuffer;

	ComputeBuffer ArgBufferTrue;
	ComputeBuffer AppendRenderBuffer;

	List<Material> particleSystemMaterials = new List<Material>();
	List<PartSysProp> particleSystemProperties = new List<PartSysProp>();

	public ComputeShader cShade;


	public int maxParticleCount = 256;//65536;//131072//262144//524288//1048576
	public int maxParticleTypeCount = 4096;

	int sqrtCount;

	public const string _MainKernalName = "CSMarch";//"CSMain";

	public static ParticleSystemsManager inst;


	//TODO: DO THE TWIST!!! FLIP A READ AND WRITE BUFFER
	void Start ()
	{
		//Application.targetFrameRate = 60;
		ParticleSystemsManager.inst = this;
		particleSystemProperties.AddRange(this.gameObject.GetComponents<PartSysProp>());


		int sqrtCount = (int)Mathf.Sqrt(maxParticleCount);
		DataBuffer[READ] = new ComputeBuffer (maxParticleCount, ParticleData.stride, ComputeBufferType.Default);
		DataBuffer[WRITE] = new ComputeBuffer (maxParticleCount, ParticleData.stride , ComputeBufferType.Default);

		ArgBufferTrue = new ComputeBuffer(4, sizeof(uint), ComputeBufferType.DrawIndirect);
		AppendRenderBuffer = new ComputeBuffer(maxParticleCount, ParticleData.stride, ComputeBufferType.Append);



		ParticlePropertyBuffer = new ComputeBuffer(maxParticleTypeCount, PartSysPropComputeModel.stride, ComputeBufferType.Default);
		PartSysPropComputeModel[] propList = new PartSysPropComputeModel[maxParticleTypeCount];
		for(int j =0; j < particleSystemProperties.Count; j++)
		{
			propList[j] = particleSystemProperties[j].toCompute;
		}
		ParticlePropertyBuffer.SetData(propList);



		ParticleData[] pdatas = new ParticleData[maxParticleCount];

		int IDitterator = 0;
		for (int i = 0; i < maxParticleCount; i++)
		{
			pdatas[i].typeIDnum = -1;
		}

		DataBuffer[READ].SetData (pdatas);
		DataBuffer[WRITE].SetData (pdatas);


		cShade.SetBuffer(cShade.FindKernel("CSAddNewParticles"), "RaddPartData", DataBuffer[READ]);
		cShade.SetBuffer(cShade.FindKernel("CSAddNewParticles"), "WaddPartData", DataBuffer[WRITE]);
		cShade.SetBuffer(cShade.FindKernel("CSAddNewParticles"), "RaddPartSysProperties", ParticlePropertyBuffer);
		cShade.SetInt("totalMaxParticles",maxParticleCount);

		cShade.SetInt("typeIDtoAdd",1);
		cShade.Dispatch(cShade.FindKernel("CSAddNewParticles"),10000,1,1);
		Swap(DataBuffer);
		cShade.SetBuffer(cShade.FindKernel("CSAddNewParticles"), "RaddPartData", DataBuffer[READ]);
		cShade.SetBuffer(cShade.FindKernel("CSAddNewParticles"), "WaddPartData", DataBuffer[WRITE]);

		cShade.SetInt("typeIDtoAdd",0);
		cShade.Dispatch(cShade.FindKernel("CSAddNewParticles"),4096,1,1);
		Swap(DataBuffer);

	}

	void Update()
	{
		cShade.SetFloat("_DeltaTime",  Time.deltaTime);
		cShade.SetFloat("_CurTime",  Time.time);

		cShade.SetBuffer(cShade.FindKernel(_MainKernalName), "RpartData", DataBuffer[READ]);
		cShade.SetBuffer(cShade.FindKernel(_MainKernalName), "WpartData", DataBuffer[WRITE]);

		cShade.Dispatch(cShade.FindKernel(_MainKernalName), maxParticleCount/64, 1, 1);

		Swap(DataBuffer);
	}

	void Swap(ComputeBuffer[] buffer) 
	{
		ComputeBuffer tmp = buffer[READ];
		buffer[READ] = buffer[WRITE];
		buffer[WRITE] = tmp;
	}



	void OnPostRender()
	{
		int[] argdata = new int[]{0,1,0,0};
		ArgBufferTrue.SetData(argdata);


		for(int i = 0; i < 2; i++)
		{
			int materialId = i;
			
			cShade.SetBuffer(cShade.FindKernel("CSGetRenderReady"), "RendRpartData", DataBuffer[READ]);
			cShade.SetBuffer(cShade.FindKernel("CSGetRenderReady"), "RendRpartSysProperties", ParticlePropertyBuffer);
			cShade.SetBuffer(cShade.FindKernel("CSGetRenderReady"), "WappendData", AppendRenderBuffer);
			cShade.SetInt("_MatIDToCheck", materialId);
			cShade.Dispatch(cShade.FindKernel("CSGetRenderReady"),  maxParticleCount/64, 1, 1);
			
			
			ComputeBuffer.CopyCount(AppendRenderBuffer, ArgBufferTrue, 0);
			ArgBufferTrue.GetData(argdata);
			
			
			Debug.Log(argdata[0] + " , " + argdata[1] + " , " + argdata[2] + " , " + argdata[3]);
			this.particleSystemMaterials[materialId].SetPass(0);
			this.particleSystemMaterials[materialId].SetBuffer("_PartDat", AppendRenderBuffer);
			Graphics.DrawProceduralIndirect(MeshTopology.Points, ArgBufferTrue);
			
			
			cShade.SetBuffer(cShade.FindKernel("CSConsumeBuffer"), "ConsRpartData", DataBuffer[READ]);
			cShade.SetBuffer(cShade.FindKernel("CSConsumeBuffer"), "ConsRpartSysProperties", ParticlePropertyBuffer);
			cShade.SetBuffer(cShade.FindKernel("CSConsumeBuffer"), "ConsumeData", AppendRenderBuffer);
			cShade.Dispatch(cShade.FindKernel("CSConsumeBuffer"), maxParticleCount/64, 1, 1);

		}
	}
	
	void OnDestroy()
	{
		DataBuffer[READ].Release();
		DataBuffer[WRITE].Release();
		ParticlePropertyBuffer.Release();
		ArgBufferTrue.Release();
		AppendRenderBuffer.Release();
	}

	public int GetMaterialIDFromMaterial(Material parMat)
	{
		int sol = this.particleSystemMaterials.IndexOf(parMat);
		if(sol == -1)
		{
			this.particleSystemMaterials.Add(parMat);
			return this.particleSystemMaterials.Count - 1;
		}
		return sol;
	}
}
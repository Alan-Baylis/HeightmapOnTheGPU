using UnityEngine;
using System.Collections;

public class ProceduralGenHandler : MonoBehaviour 
{
	public ComputeShader compShade;
	ImprovedPerlinNoise m_perlin;
	public string TEXtoSet = "_MainTex";
	public bool doTheRandomz = false;
	bool phaseTwo = false;
	public int m_seed = 0;
	public float m_frequency = 1.0f;
	public float m_lacunarity = 2.0f;
	public float m_gain = 0.5f;
	int powerOfTwoSize = 8;
	int twoPowDimensionWidth;
	public ComputeBuffer[] buffer = new ComputeBuffer[2];

	public const int READ = 1;
	public const int WRITE = 0;

	// Use this for initialization
	void Start () 
	{
		twoPowDimensionWidth = (int)Mathf.Pow(2, powerOfTwoSize);
		m_perlin = new ImprovedPerlinNoise(m_seed);
		
		m_perlin.LoadResourcesFor2DNoise();
		compShade.SetTexture(compShade.FindKernel("CSProcGen"),"_Gradient2D", m_perlin.GetGradient2D());
		compShade.SetTexture(compShade.FindKernel("CSProcGen"),"_PermTable1D", m_perlin.GetPermutationTable1D());
	
	
		buffer[READ] = new ComputeBuffer (twoPowDimensionWidth*twoPowDimensionWidth, sizeof(float), ComputeBufferType.Default);
		buffer[WRITE] = new ComputeBuffer (twoPowDimensionWidth*twoPowDimensionWidth, sizeof(float), ComputeBufferType.Default);
	}

	void makeNewRandomTexture()
	{
		compShade.SetInt("_ProcSize", twoPowDimensionWidth);
		compShade.SetFloat("_Frequency", m_frequency);
		compShade.SetFloat("_Lacunarity",m_lacunarity);
		compShade.SetFloat("_Gain", m_gain);
		//RenderTexture rend = new RenderTexture(256,256,0);
		int size = twoPowDimensionWidth*twoPowDimensionWidth;
		compShade.SetBuffer(compShade.FindKernel("CSProcGen"), "WProcGenBuff", buffer[WRITE]);
		compShade.SetBuffer(compShade.FindKernel("CSProcGen"), "RProcGenBuff", buffer[READ]);

		compShade.Dispatch(compShade.FindKernel("CSProcGen"),size/32, 1,1);

	}
	void Swap(ComputeBuffer[] buffer) 
	{
		ComputeBuffer tmp = buffer[READ];
		buffer[READ] = buffer[WRITE];
		buffer[WRITE] = tmp;
	}

	Texture2D makeTextureFromArray()
	{
		float[] datums = new float[twoPowDimensionWidth*twoPowDimensionWidth]; 
		buffer[READ].GetData(datums);

		Texture2D newTex = new Texture2D(twoPowDimensionWidth,twoPowDimensionWidth, TextureFormat.ARGB32/*RFloat*/,false);
		Color32[] colArray = new Color32[datums.Length];

		//string data = "";
		for(int i =0; i < datums.Length; i++)
		{
			byte B = F2B(/*0.5f+0.5f**/0.5f+ datums[i]);
			//data += datums[i] + " > " + B;
			colArray[i] = new Color32(B,B,B,B);
		}
		//Debug.Log(data);
		newTex.SetPixels32(colArray);
		newTex.Apply();
		return newTex;
	}

	byte F2B(float parF)
	{
		parF = parF % 1;
		return (byte)(parF*256);
	}

	
	// Update is called once per frame
	void Update ()
	{
		if(phaseTwo)
		{
			phaseTwo = false;
			Texture2D texas = makeTextureFromArray();
			this.gameObject.GetComponent<ImpactHandler>().baseHeightMapTexture = texas;
			//this.gameObject.GetComponent<MeshRenderer>().material.SetTexture(TEXtoSet,texas);
		}

		if(doTheRandomz)
		{
			doTheRandomz = false;
			makeNewRandomTexture();
			phaseTwo = true;
			Swap(buffer);
		}

	}

	void OnDestroy()
	{
		buffer[READ].Release();
		buffer[WRITE].Release();


	}
}

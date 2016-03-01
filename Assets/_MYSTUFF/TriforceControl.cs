using UnityEngine;
using System.Collections;

public class TriforceControl : MonoBehaviour 
{

	public ComputeShader shader;
	void Start()
	{
		RunShader();
	}
	void RunShader()
	{
		int kernelHandle = shader.FindKernel("CSMain");
		
		RenderTexture tex = new RenderTexture(256,256,24);
		tex.enableRandomWrite = true;
		tex.Create();

		gameObject.GetComponent<MeshRenderer>().material.mainTexture = tex;
		
		shader.SetTexture(kernelHandle, "Result", tex);
		shader.Dispatch(kernelHandle, 256/8, 256/8, 1);
	}



}

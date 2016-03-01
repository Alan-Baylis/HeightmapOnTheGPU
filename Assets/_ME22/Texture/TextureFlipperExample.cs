using UnityEngine;
using System.Collections;

public class TextureFlipperExample : MonoBehaviour
{
	public ComputeShader shader, shaderCopy;
	
	RenderTexture tex, texCopy;
	
	void Start ()
	{
		CSMainT4();
	}


	void CSMainT3()
	{
		int kernelHashOri = shader.FindKernel("CSMainT1");
		int kernelHashCpy = shader.FindKernel("CSMainT3");
		
		tex = new RenderTexture(64, 64, 0);
		tex.enableRandomWrite = true;
		tex.useMipMap = true;
		tex.generateMips = true;
		tex.Create();
		
		texCopy = new RenderTexture(64, 64, 0);
		texCopy.enableRandomWrite = true;
		texCopy.Create();
		
		shader.SetTexture(kernelHashOri, "tex1", tex);
		shader.Dispatch(kernelHashOri, tex.width/8, tex.height/8, 1);
		
		shaderCopy.SetTexture(kernelHashCpy, "tex3", tex);
		shaderCopy.SetTexture(kernelHashCpy, "tex3Copy", texCopy);
		shaderCopy.Dispatch(kernelHashCpy, texCopy.width/8, texCopy.height/8, 1);

	}

	void CSMainT4()
	{
		int kernelHashOri = shader.FindKernel("CSMainT1");
		int kernelHashCpy = shader.FindKernel("CSMainT4");
		
		tex = new RenderTexture(8, 8, 0);
		tex.enableRandomWrite = true;
		tex.useMipMap = true;
		tex.generateMips = true;
		tex.Create();
		
		texCopy = new RenderTexture(64,64, 0);
		texCopy.enableRandomWrite = true;
		texCopy.Create();
		
		shader.SetTexture(kernelHashOri, "tex1", tex);
		shader.Dispatch(kernelHashOri, tex.width/8, tex.height/8, 1);
		
		shaderCopy.SetTexture(kernelHashCpy, "tex4", tex);
		shaderCopy.SetTexture(kernelHashCpy, "tex4Copy", texCopy);
		shaderCopy.Dispatch(kernelHashCpy, texCopy.width/8, texCopy.height/8, 1);
		
	}

	
	void OnGUI()
	{
		int w = Screen.width/2;
		int h = Screen.height/2;
		int s = 512;
		
		GUI.DrawTexture(new Rect(w-s/2,h-s/2,s,s), texCopy);
	}
	
	void OnDestroy()
	{
		tex.Release();
		texCopy.Release();
	}
}
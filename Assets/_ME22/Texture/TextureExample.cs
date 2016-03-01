using UnityEngine;
using System.Collections;

public class TextureExample : MonoBehaviour
{
	public ComputeShader shader;
	
	RenderTexture tex1;

	void Start ()
	{
		MainT2();
	}

	void MainT1()
	{
		tex1 = new RenderTexture(8, 8, 0);
		tex1.enableRandomWrite = true;
		tex1.filterMode = FilterMode.Point;
		tex1.Create();

		shader.SetTexture(shader.FindKernel("CSMainT1"), "tex1", tex1);

		shader.Dispatch(shader.FindKernel("CSMainT1"), tex1.width/8, tex1.height/8, 1);
		/* the divide by 'N' (in this case N = 8)
		 * is divide by the number of threads in each group in each direction, 
		 * ie: since CSMainT1 has [8,8,1] threads per group, then divide by those numbers*/

	}


	void MainT2()
	{
		tex1 = new RenderTexture(32, 32, 0);
		tex1.enableRandomWrite = true;
		tex1.filterMode = FilterMode.Point;
		tex1.Create();
		
		shader.SetTexture(shader.FindKernel("CSMainT2"), "tex2", tex1);
		shader.Dispatch(shader.FindKernel("CSMainT2"), tex1.width/8, tex1.height/8, 1);
		
	}



	
	void OnGUI()
	{
		int w = Screen.width/2;
		int h = Screen.height/2;
		int s = 512;

		GUI.DrawTexture(new Rect(w-s/2,h-s/2,s,s), tex1);
	}
	
	void OnDestroy()
	{
		tex1.Release();
	}
}
using UnityEngine;
using System.Collections;

public class KernelTest : MonoBehaviour
{
	
	public ComputeShader shader;
	int size = 4;
	void Start ()
	{    
		Kernel3();
	}

	void Kernel1()
	{
		ComputeBuffer buffer = new ComputeBuffer(size, sizeof(int));
		
		shader.SetBuffer(shader.FindKernel("CSMain1"), "buffer1", buffer);
		
		shader.Dispatch(shader.FindKernel("CSMain1"), 1, 1, 1);
		
		int[] data = new int[size];
		
		buffer.GetData(data);
		
		for(int i = 0; i < size; i++)
			Debug.Log(data[i]);
		
		buffer.Release();
	}

	void Kernel2()
	{
		ComputeBuffer buffer = new ComputeBuffer(size*2, sizeof(int));
		
		shader.SetBuffer(shader.FindKernel("CSMain2"), "buffer2", buffer);
		
		shader.Dispatch(shader.FindKernel("CSMain2"), 2, 1, 1);
		
		int[] data = new int[size*2];
		
		buffer.GetData(data);
		
		for(int i = 0; i < size*2; i++)
			Debug.Log(data[i]);
		
		buffer.Release();

	}

	void Kernel3()
	{
		ComputeBuffer buffer = new ComputeBuffer(size*2, sizeof(int));
		
		shader.SetBuffer(shader.FindKernel("CSMainK3"), "buffer3", buffer);
		
		shader.Dispatch(shader.FindKernel("CSMainK3"), 2, 1, 1);
		
		int[] data = new int[size*2];
		
		buffer.GetData(data);
		
		for(int i = 0; i < size*2; i++)
			Debug.Log(data[i]);
		
		buffer.Release();
		
	}
}
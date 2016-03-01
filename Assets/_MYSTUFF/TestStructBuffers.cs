using UnityEngine;
using System.Collections;

public class TestStructBuffers : MonoBehaviour 
{

	public ComputeShader shader;
	void Start()
	{
		RunShader();
	}
	void RunShader()
	{

		VecMatPair[] data = new VecMatPair[5];
		VecMatPair[] output = new VecMatPair[5];

		//INITIALIZE DATA HERE
		
		ComputeBuffer buffer = new ComputeBuffer(data.Length, 76);
		int kernel = shader.FindKernel("Multiply");
		shader.SetBuffer(kernel, "dataBuffer", buffer);
		shader.Dispatch(kernel, data.Length, 1,1);
		buffer.GetData(output);



	}


	struct VecMatPair
	{
		public Vector3 point;
		public Matrix4x4 matrix;
	}


}

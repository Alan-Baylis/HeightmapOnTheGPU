using UnityEngine;
using System.Collections;

public class BufferExample : MonoBehaviour
{
	public Material material;
	ComputeBuffer buffer;
	
	const int count = 1024;
	const float size = 5.0f;



	struct Vert
	{
		public Vector3 position;
		public Color color;
	}

	void Start ()
	{

		buffer = new ComputeBuffer (count, sizeof(float) * 6, ComputeBufferType.Default);
		
		Vert[] points = new Vert[count];
		
		Random.seed = 0;
		for (int i = 0; i < count; i++)
		{
			points[i] = new Vert();
			
			points[i].position = new Vector3();
			points[i].position.x = Random.Range (-size, size);
			points[i].position.y = Random.Range (-size, size);
			points[i].position.z = Random.Range (-size/5f, size/5f);;
			
			/*points[i].color = new Vector3();
			points[i].color.x = Random.value > 0.5f ? 0.0f : 1.0f;
			points[i].color.y = Random.value > 0.5f ? 0.0f : 1.0f;
			points[i].color.z = Random.value > 0.5f ? 0.0f : 1.0f;
*/

			points[i].color = new Color();
			points[i].color.r = Random.value > 0.5f ? 0.0f : 1.0f;
			points[i].color.g = Random.value > 0.5f ? 0.0f : 1.0f;
			points[i].color.b = Random.value > 0.5f ? 0.0f : 1.0f;

		}
		
		buffer.SetData (points);
	}



	void OnRenderObject()
	{
		material.SetPass(0);
		material.SetBuffer("_Vertz", buffer);
		Graphics.DrawProcedural(MeshTopology.Points, count, 1);
	}
	
	void OnDestroy()
	{
		buffer.Release();
	}
}
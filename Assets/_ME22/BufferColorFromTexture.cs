using UnityEngine;
using System.Collections;

public class BufferColorFromTexture : MonoBehaviour
{
	public Material material;
	public Texture2D textureSample;
	ComputeBuffer buffer;

	public bool classicPosGen = true;
	public bool classicColGenR = true;
	public bool classicColGenG = true;
	public bool classicColGenB = true;
	public Vector3 colorFinalMult = Vector3.one;


	public Vector2 center = new Vector2(0.7f, 0.7f);
	
	const int count = 1024;//32768;//4096;//1024; 
	const float size = 4f;
	public int sed = 0;


	struct Vert
	{
		public Vector3 position;
		public Vector3 uv3s;
		public Vector3 color;
	}

	void Start ()
	{
		material.mainTexture = this.textureSample;

		buffer = new ComputeBuffer (count, sizeof(float) * 9, ComputeBufferType.Default);
		
		Vert[] points = new Vert[count];

		if(sed == 0)
			Random.seed = sed =(int)(Random.value*(float)int.MaxValue);
		else
			Random.seed = sed;

		for (int i = 0; i < count; i++)
		{
			points[i] = new Vert();
			
			points[i].position = new Vector3();
			if(classicPosGen == true)
			{
				points[i].position.x = Random.Range (-size, size);
				points[i].position.y = Random.Range (-size, size);
				points[i].position.z = Random.Range (-size/5f, size/5f);
			}
			else
			{
				points[i].position.x = Random.Range (-size, size);
				points[i].position.y = Random.Range (-size, size);
				points[i].position.z = Random.Range (-size/5f, size/5f);

			}
			
			/*points[i].color = new Vector3();
			points[i].color.x = Random.value > 0.5f ? 0.0f : 1.0f;
			points[i].color.y = Random.value > 0.5f ? 0.0f : 1.0f;
			points[i].color.z = Random.value > 0.5f ? 0.0f : 1.0f;
*/

			points[i].uv3s = new Vector3();
			points[i].uv3s.x = points[i].position.x/(size*2f) + 0.5f;
			points[i].uv3s.y = points[i].position.y/(size*2f) + 0.5f;
			points[i].uv3s.z = points[i].position.z/(size*2f/5f) + 0.5f;

			points[i].color = new Vector3();
			float r1 = Random.value;
			float r2 = r1 > 0.5f ? Random.value : 0.0f;
			float r3 = r2 > 0.5f ? Random.value : 0.0f;

			float rr = r1 > 0.4f ? 1.0f : 0.0f;
			float gr = r2 > 0.5f ? 1.0f : 0.0f;
			float br = r3 > 0.5f ? 1.0f : 0.0f;

			float rc = Random.value >= 0.5f ? 1.0f : 0.0f;
			float gc = Random.value >= 0.5f ? 1.0f : 0.0f;
			float bc = Random.value >= 0.5f ? 1.0f : 0.0f;

			points[i].color.x = classicColGenR ? rc : rr;
			points[i].color.y = classicColGenG ? gc : gr;
			points[i].color.z = classicColGenB ? bc : br;


			points[i].color.x *= colorFinalMult.x;
			points[i].color.y *= colorFinalMult.y;
			points[i].color.z *= colorFinalMult.z;

		}
		
		buffer.SetData (points);
	}


	Vector3 RandomVec(float scalex = 1, float scaley = 1, float scalez = 1)
	{
		return new Vector3(Random.value*scalex, Random.value*scaley,Random.value*scalez);
	}
	Vector3 RandomVec()
	{
		return new Vector3(Random.value, Random.value,Random.value);
	}
	Vector3 RandomVec(Vector3 parScale)
	{
		return RandomVec(parScale.x, parScale.y,parScale.z);
	}


	Vector3 StripColor(Color parCol)
	{
		return new Vector3(parCol.r, parCol.g, parCol.b);
	}

	void OnRenderObject()
	{
		material.SetPass(0);
		material.SetBuffer("buffer", buffer);
		Graphics.DrawProcedural(MeshTopology.Quads, count, 1);
	}
	
	void OnDestroy()
	{
		buffer.Release();
	}
}
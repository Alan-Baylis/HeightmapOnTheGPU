using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ImpactHandler : MonoBehaviour
{
	public ComputeShader compShader;
	public Texture2D baseHeightMapTexture;

	public int powerOfTwoTextureDimension;
	private int twoPow;
	public ComputeBuffer[] buffer = new ComputeBuffer[2];
	[HideInInspector]
	public RenderTexture[] hTex = new RenderTexture[2];


	public float radius;
	public Transform  _InterceptObjectTransform;
	public Transform  _InterceptObjectTransformB;
	public Transform  _InterceptObjectTransformC;

	public Transform _TerrainPlane;
	public Vector3 _HeightMapDimensions;
	public float _BaseHeightMapHeight;

	public Vector2 maxAgeANDlerpOutTime = new Vector2(100, 10);


	public Transform _InterceptCube;
	public Vector2 _CubeFallOffSlopeAndOffset;


	public const int READ = 1;
	public const int WRITE = 0;


	void Start ()
	{
		MainT2();
		_TerrainPlane.gameObject.GetComponent<MeshRenderer>().material.SetTexture("_HeightTex", hTex[READ]);
	}


	void MainT2()
	{
		this._HeightMapDimensions = new Vector3(this.transform.localScale.x*10,this._HeightMapDimensions.y,this.transform.localScale.z*10);
		twoPow = (int)Mathf.Pow(2, powerOfTwoTextureDimension);


		hTex[READ] = new RenderTexture(twoPow, twoPow, 0);
		hTex[READ].enableRandomWrite = true;
		hTex[READ].filterMode = FilterMode.Bilinear;
		hTex[READ].Create();

		hTex[WRITE] = new RenderTexture(twoPow, twoPow, 0);
		hTex[WRITE].enableRandomWrite = true;
		hTex[WRITE].filterMode = FilterMode.Bilinear;
		hTex[WRITE].Create();

		//hTex = new RenderTexture(twoPow, twoPow, 0);
		//hTex.enableRandomWrite = true;
		//hTex.filterMode = FilterMode.Bilinear;
		//hTex.Create();



		buffer[READ] = new ComputeBuffer (twoPow*twoPow, sizeof(float)*2, ComputeBufferType.Default);
		buffer[WRITE] = new ComputeBuffer (twoPow*twoPow, sizeof(float)*2, ComputeBufferType.Default);



/*		float[] flatArray = new float[twoPow*twoPow];
		for(int i =0; i < flatArray.Length; i++)
		{
			flatArray[i] = 1;
		}
		buffer[READ].SetData(flatArray);*/

		compShader.SetBuffer(compShader.FindKernel("CSWhipe"), "WriteBuffer", buffer[WRITE]);
		compShader.Dispatch(compShader.FindKernel("CSWhipe"), (twoPow*twoPow)/32, 1, 1);
		Swap(buffer);
	}

	void Update()
	{
		compShader.SetBuffer(compShader.FindKernel("CSImpactDetect"), "ReadBuffer", buffer[READ] );
		compShader.SetBuffer(compShader.FindKernel("CSImpactDetect"), "WriteBuffer", buffer[WRITE] );
		compShader.SetTexture(compShader.FindKernel("CSImpactDetect"), "outHeightMap", hTex[WRITE]);
		compShader.SetTexture(compShader.FindKernel("CSImpactDetect"), "existingMap", baseHeightMapTexture);
		radius = _InterceptObjectTransform.localScale.x*0.5f;
		compShader.SetVector("_ObjectPosition", new Vector4(_InterceptObjectTransform.position.x, _InterceptObjectTransform.position.y, _InterceptObjectTransform.position.z, radius));
		radius = _InterceptObjectTransformB.localScale.x*0.5f;
		compShader.SetVector("_ObjectPositionB", new Vector4(_InterceptObjectTransformB.position.x, _InterceptObjectTransformB.position.y, _InterceptObjectTransformB.position.z, radius));
		radius = _InterceptObjectTransformC.localScale.x*0.5f;
		compShader.SetVector("_ObjectPositionC", new Vector4(_InterceptObjectTransformC.position.x, _InterceptObjectTransformC.position.y, _InterceptObjectTransformC.position.z, radius));



		compShader.SetVector("_FallOffSteepnessOffset", _CubeFallOffSlopeAndOffset);
		compShader.SetFloats("_CubeTransformMatrix", MatrixToArray(_InterceptCube.worldToLocalMatrix));


		//distance from plane's pivot to the offset pivot (the plane is 10 units wide at base, so we subtract half that times the scale to get the corner where we measure the offset)
		Vector3 scalePivotOffset = new Vector3(_TerrainPlane.localScale.x*5, 0,_TerrainPlane.localScale.z*5);

		compShader.SetVector("_TimeAndDeltaTime", new Vector4(Time.time, Time.deltaTime));
		compShader.SetVector("_Offset", _TerrainPlane.position-scalePivotOffset);
		compShader.SetVector("_HeightmapDims", new Vector4(_HeightMapDimensions.x, _HeightMapDimensions.y, _HeightMapDimensions.z, _BaseHeightMapHeight));
		compShader.SetFloat("maxAge",maxAgeANDlerpOutTime.x);
		compShader.SetFloat("lerpOutTime",maxAgeANDlerpOutTime.y);

		compShader.Dispatch(compShader.FindKernel("CSImpactDetect"), (twoPow*twoPow)/32, 1, 1);




		compShader.SetTexture(compShader.FindKernel("CSCopyBufferToRenderTexture"), "existingMap", baseHeightMapTexture);
		compShader.SetBuffer(compShader.FindKernel("CSCopyBufferToRenderTexture"), "ReadBuffer", buffer[READ]);
		compShader.SetTexture(compShader.FindKernel("CSCopyBufferToRenderTexture"), "outHeightMap", hTex[WRITE]);
		compShader.Dispatch(compShader.FindKernel("CSCopyBufferToRenderTexture"), hTex[READ].width/16, hTex[READ].height/16, 1);


		_TerrainPlane.gameObject.GetComponent<MeshRenderer>().material.SetTexture("_HeightTex", hTex[READ]);
		_TerrainPlane.gameObject.GetComponent<MeshRenderer>().material.SetVector("_HeightTexDims", new Vector4(256,256, _BaseHeightMapHeight, 2));
		_TerrainPlane.gameObject.GetComponent<MeshRenderer>().material.SetVector("_Mod", new Vector4(15,40, _BaseHeightMapHeight, 1));


		Swap(buffer);
	}


	public static float[] MatrixToArray(Matrix4x4 parMatrix)
	{
		return  new float[] {parMatrix.m00, parMatrix.m10, parMatrix.m20, parMatrix.m30,
							 parMatrix.m01, parMatrix.m11, parMatrix.m21, parMatrix.m31,	
							 parMatrix.m02, parMatrix.m12, parMatrix.m22, parMatrix.m32,
							 parMatrix.m03, parMatrix.m13, parMatrix.m23, parMatrix.m33
							 };
		/*return  new float[] {parMatrix.m00, parMatrix.m01, parMatrix.m02, parMatrix.m03,
							 parMatrix.m10, parMatrix.m11, parMatrix.m12, parMatrix.m13,	
							 parMatrix.m20, parMatrix.m21, parMatrix.m22, parMatrix.m23,
							 parMatrix.m30, parMatrix.m31, parMatrix.m32, parMatrix.m33
							};*/
	}
	
	void Swap(ComputeBuffer[] buffer) 
	{
		ComputeBuffer tmp = buffer[READ];
		buffer[READ] = buffer[WRITE];
		buffer[WRITE] = tmp;

		RenderTexture tmpRT = hTex[READ];
		hTex[READ] = hTex[WRITE];
		hTex[WRITE] = tmpRT;
	}


	
	void OnGUI()
	{
		int w = Screen.width/2;
		int h = Screen.height/2;
		int s = 512;

		//GUI.DrawTexture(new Rect(w-s/2,h-s/2,s,s), hTex[READ]);

	}

	void OnDestroy()
	{
		//hTex.Release();
		hTex[READ].Release();
		hTex[WRITE].Release();
		buffer[READ].Release();
		buffer[WRITE].Release();

	}
}
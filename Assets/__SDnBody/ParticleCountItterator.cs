using UnityEngine;
using System.Collections;

public static class ParticleCountItterator 
{
	public static int multiplier = 0;
	public static void Init(BufferHandlerPS1 bffHndl)
	{
		bffHndl.count = 65536* (int)Mathf.Pow(2, multiplier);

	}
}

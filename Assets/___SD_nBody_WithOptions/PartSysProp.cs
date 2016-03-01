using UnityEngine;
using System.Collections;

public class PartSysProp : MonoBehaviour 
{
	public Material renderMat;
	private int materialID
	{
		get{
			return ParticleSystemsManager.inst.GetMaterialIDFromMaterial(this.renderMat);
		}
	}

	public Vector3 initialVelocityObjective;
	public Vector3 initialVelocityTangent;

	public Vector3 constantForceObjective;
	public Vector3 constantForceTangent;

	public float initialLife;
	
	public byte emitModel;
	public Vector3 emitPosition;
	public Vector4 emitDirectionSize;

	public void Init(Material parMat, Vector3 parInitVelObj, Vector3 parInitVelTan, Vector3 parConstForceObj, Vector3 parConstForceTan, float parInitLife, Vector3 parEmitPos, Vector4 parEmitDirSize, byte parEmitModel = 0)
	{


		renderMat = parMat;
		initialVelocityObjective = parInitVelObj;
		initialVelocityTangent	 = parInitVelTan;
		constantForceObjective = parConstForceObj;
		constantForceTangent = parConstForceTan;
		initialLife = parInitLife;

		emitPosition = parEmitPos;
		emitDirectionSize = parEmitDirSize;
		emitModel = parEmitModel;
	}

	public PartSysPropComputeModel toCompute
	{
		get
		{
			PartSysPropComputeModel toReturn = new PartSysPropComputeModel();

			toReturn.matID = this.materialID;
			toReturn.emitModel = this.emitModel;
			toReturn.emitPos = this.emitPosition;
			toReturn.emitDirSize    = this.emitDirectionSize;
			toReturn.initVelObj 	= this.initialVelocityObjective;
			toReturn.initVelTan 	= this.initialVelocityTangent;

			toReturn.constForceObj = this.constantForceObjective;
			toReturn.constForceTan = this.constantForceTangent;

			toReturn.initLife	= this.initialLife;

			return toReturn;
		}
	}
}

public struct PartSysPropComputeModel
{
	public static int stride = sizeof(int)*2 + sizeof(float)*20;
	public int matID;
	public int emitModel;
	public Vector3 emitPos;
	public Vector4 emitDirSize;

	public Vector3 initVelObj;
	public Vector3 initVelTan;

	public Vector3 constForceObj;
	public Vector3 constForceTan;
	public float initLife;
}

public struct ParticleData
{
	public static int stride = sizeof(int) + sizeof(float)*7;
	public Vector3 position;
	public Vector3 velocity;
	public float age;
	public int typeIDnum;
}


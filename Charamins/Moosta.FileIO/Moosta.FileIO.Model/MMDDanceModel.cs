using CTUtilCLR;
using Moosta.Common;
using Moosta.Common.DanceMotion;
using Moosta.Common.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
namespace Moosta.FileIO.Model
{
	public class MMDDanceModel : IDanceModel
	{
		public readonly CtMeshContainer Mesh;
		public readonly Dictionary<string, BoneAssigner> BoneTable = new Dictionary<string, BoneAssigner>();
		private static readonly string mmdBoneNameOfROOT = Moosta.Common.BoneNames.TryConvertToMmdBone("OMP.ROOT");
		private static readonly string mmdBoneNameOfLOWERBODY = Moosta.Common.BoneNames.TryConvertToMmdBone("OMP.LOWER_BODY");
		public string FileName
		{
			get;
			set;
		}
		public string[] BoneNames
		{
			get;
			private set;
		}
		public float AnkleHeight
		{
			get;
			private set;
		}
		public MMDDanceModel(CtMeshContainer mesh, Dictionary<string, BoneAssigner> bones)
		{
			this.Mesh = mesh;
			this.BoneTable = bones;
			this.BoneNames = (
				from name in bones.Keys
				orderby name
				select name).ToArray<string>();
			mesh.UpdateBoneMatrices();
			this.AnkleHeight = mesh.MatrixPallet.TransformedMatrices[(int)((UIntPtr)bones["LeftAnkle"].Bone.MatrixIndex)].GetPosition().Y;
		}
		public void SetPose(DancePose pose)
		{
			if (pose.FootState == DancePoseFootState.Auto)
			{
				CtMatrix4x4[] transformedMatrices = this.Mesh.MatrixPallet.TransformedMatrices;
				uint matrixIndex = this.BoneTable["LeftAnkle"].Bone.MatrixIndex;
				uint matrixIndex2 = this.BoneTable["RightAnkle"].Bone.MatrixIndex;
				CtVector3D position = transformedMatrices[(int)((UIntPtr)matrixIndex)].GetPosition();
				CtVector3D position2 = transformedMatrices[(int)((UIntPtr)matrixIndex2)].GetPosition();
				bool flag = position.Y < position2.Y;
				CtVector3D a = flag ? position : position2;
				a.Y = this.AnkleHeight;
				this.SetPoseWithoutPivoting(pose);
				this.Mesh.UpdateBoneMatrices();
				CtVector3D position3 = (flag ? transformedMatrices[(int)((UIntPtr)matrixIndex)] : transformedMatrices[(int)((UIntPtr)matrixIndex2)]).GetPosition();
				CtBone bone = this.BoneTable["Hips"].Bone;
				bone.TransformMatrix = CtMatrix4x4.MulM4xM4(bone.TransformMatrix, CtMatrix4x4.Translate(a - position3));
				this.Mesh.UpdateBoneMatrices();
				return;
			}
			this.SetPoseWithoutPivoting(pose);
			CtBone bone2 = this.Mesh.MatrixPallet.GetBone(MMDDanceModel.mmdBoneNameOfROOT);
			CtBone bone3 = this.Mesh.MatrixPallet.GetBone(MMDDanceModel.mmdBoneNameOfLOWERBODY);
			float f = -bone3.OffsetMatrix.GetPosition().Y;
			CtVector3D position4 = bone2.TransformedMatrix.GetPosition();
			CtVector3D position5 = bone3.TransformedMatrix.GetPosition();
			CtVector3D b = position4 - position5 + bone2.OffsetMatrix.GetPosition();
			bone2.TransformMatrix.TranslateFromRight(f * pose.BodyPosition + b);
			this.Mesh.UpdateBoneMatrices();
		}
		private void SetPoseWithoutPivoting(DancePose pose)
		{
			string[] boneNames = pose.BoneNames;
			for (int i = 0; i < boneNames.Length; i++)
			{
				string boneName = boneNames[i];
				BoneState boneState = pose[boneName];
				if (!this.setBoneAsNativeBoneName(boneName, boneState) && !this.setBoneViaMMDBoneName(boneName, boneState))
				{
					this.setBoneAsBVHBoneName(boneName, boneState);
				}
			}
		}
		private bool setBoneAsBVHBoneName(string boneName, BoneState boneState)
		{
			BoneAssigner bone;
			if (this.BoneTable.TryGetValue(boneName, out bone))
			{
				MMDDanceModel.SetBoneRotation(bone, CtMatrix4x4.Rotate(boneState.Rotate));
				return true;
			}
			return false;
		}
		private bool setBoneAsNativeBoneName(string boneName, BoneState boneState)
		{
			CtBone bone = this.Mesh.MatrixPallet.GetBone(boneName);
			if (bone != null)
			{
				bone.TransformMatrix = CtMatrix4x4.Rotate(boneState.Rotate);
			}
			return bone != null;
		}
		private bool setBoneViaMMDBoneName(string boneName, BoneState boneState)
		{
			string text = Moosta.Common.BoneNames.TryConvertToMmdBone(boneName);
			return !string.IsNullOrEmpty(text) && this.setBoneAsNativeBoneName(text, boneState);
		}
		public void UpdateMatrices()
		{
			this.Mesh.UpdateBoneMatrices();
		}
		public CtVector3D GetBonePosition(string boneName)
		{
			return this.BoneTable[boneName].Bone.TransformedMatrix.GetPosition();
		}
		public CtVector3D RotateVectorByBone(CtVector3D dir, string boneName)
		{
			return dir;
		}
		public void CleanPose()
		{
			for (int i = 0; i < this.Mesh.MatrixPallet.GetBoneCount(); i++)
			{
				CtBone bone = this.Mesh.MatrixPallet.GetBone(i);
				bone.TransformMatrix = CtMatrix4x4.UnitMatrix();
			}
			this.Mesh.UpdateBoneMatrices();
		}
		public void RenderModel(CtGraphicsInterface gi)
		{
			this.Mesh.UpdateSoftwareBlendMesh();
			gi.RenderMeshContainer(this.Mesh);
		}
		private static void SetBoneRotation(BoneAssigner bone, CtMatrix4x4 tm)
		{
			CtMatrix4x4 ctMatrix4x = CtMatrix4x4.UnitMatrix();
			for (CtBone ctBone = bone.Bone; ctBone != null; ctBone = ctBone.Parent)
			{
				ctMatrix4x *= ctBone.InitMatrix;
			}
			CtMatrix4x4 ctMatrix4x2 = CtMatrix4x4.UnitMatrix();
			CtVector3D[] array = new CtVector3D[]
			{
				CtMatrix4x4.Mulv3xM4N(new CtVector3D(1f, 0f, 0f), ctMatrix4x),
				CtMatrix4x4.Mulv3xM4N(new CtVector3D(0f, 1f, 0f), ctMatrix4x),
				CtMatrix4x4.Mulv3xM4N(new CtVector3D(0f, 0f, 1f), ctMatrix4x)
			};
			array[0].Normalize();
			array[1].Normalize();
			array[2].Normalize();
			ctMatrix4x2.SetAxis3(array[0], array[1], array[2]);
			ctMatrix4x2.Transpose();
			CtMatrix4x4 ctMatrix4x3 = new CtMatrix4x4(ctMatrix4x2);
			CtMatrix4x4.Inverse(ctMatrix4x3);
			CtMatrix4x4 leftMatrix = bone.LeftMatrix;
			CtMatrix4x4 rightMatrix = bone.RightMatrix;
			bone.Bone.TransformMatrix = ctMatrix4x2 * leftMatrix * tm * rightMatrix * ctMatrix4x3;
		}
		public static MMDDanceModel LoadMMDModel(CtGraphicsInterface gi, string fileName)
		{
			CtMeshContainer ctMeshContainer = null;
			string text = Path.GetExtension(fileName).ToLower();
			if (text.Equals(".ctmm"))
			{
				ctMeshContainer = CtModelFileLoader.LoadCTMM(fileName, gi);
			}
			else if (text.Equals(".pmd") || text.Equals(".pmx"))
			{
				ctMeshContainer = CtModelFileLoader.LoadPMD(fileName, gi);
			}
			Dictionary<string, BoneAssigner> dictionary = new Dictionary<string, BoneAssigner>();
			CtMatrixPallet matrixPallet = ctMeshContainer.MatrixPallet;
			BoneAssigner boneAssigner = new BoneAssigner(matrixPallet.GetBone("下半身"));
			BoneAssigner value = new BoneAssigner(matrixPallet.GetBone("上半身"), boneAssigner);
			dictionary.Add("Hips", new BoneAssigner(matrixPallet.GetBone("センター")));
			dictionary.Add("Chest", boneAssigner);
			dictionary.Add("Chest2", value);
			dictionary.Add("Neck", new BoneAssigner(matrixPallet.GetBone("首")));
			dictionary.Add("Head", new BoneAssigner(matrixPallet.GetBone("頭")));
			double num = CtMath.DegToRad(40.0);
			dictionary.Add("LeftCollar", new BoneAssigner(matrixPallet.GetBone("左肩")));
			dictionary.Add("LeftShoulder", new BoneAssigner(matrixPallet.GetBone("左腕"), CtMatrix4x4.Rotate(new CtVector3D(0f, 0f, 1f), num), null));
			dictionary.Add("LeftElbow", new BoneAssigner(matrixPallet.GetBone("左ひじ"), CtMatrix4x4.Rotate(new CtVector3D(0f, 0f, 1f), num), CtMatrix4x4.Rotate(new CtVector3D(0f, 0f, 1f), -num)));
			dictionary.Add("LeftWrist", new BoneAssigner(matrixPallet.GetBone("左手首"), CtMatrix4x4.Rotate(new CtVector3D(0f, 0f, 1f), num), CtMatrix4x4.Rotate(new CtVector3D(0f, 0f, 1f), -num)));
			dictionary.Add("RightCollar", new BoneAssigner(matrixPallet.GetBone("右肩")));
			dictionary.Add("RightShoulder", new BoneAssigner(matrixPallet.GetBone("右腕"), CtMatrix4x4.Rotate(new CtVector3D(0f, 0f, 1f), -num), null));
			dictionary.Add("RightElbow", new BoneAssigner(matrixPallet.GetBone("右ひじ"), CtMatrix4x4.Rotate(new CtVector3D(0f, 0f, 1f), -num), CtMatrix4x4.Rotate(new CtVector3D(0f, 0f, 1f), num)));
			dictionary.Add("RightWrist", new BoneAssigner(matrixPallet.GetBone("右手首"), CtMatrix4x4.Rotate(new CtVector3D(0f, 0f, 1f), -num), CtMatrix4x4.Rotate(new CtVector3D(0f, 0f, 1f), num)));
			dictionary.Add("LeftHip", new BoneAssigner(matrixPallet.GetBone("左足")));
			dictionary.Add("LeftKnee", new BoneAssigner(matrixPallet.GetBone("左ひざ")));
			dictionary.Add("LeftAnkle", new BoneAssigner(matrixPallet.GetBone("左足首")));
			dictionary.Add("RightHip", new BoneAssigner(matrixPallet.GetBone("右足")));
			dictionary.Add("RightKnee", new BoneAssigner(matrixPallet.GetBone("右ひざ")));
			dictionary.Add("RightAnkle", new BoneAssigner(matrixPallet.GetBone("右足首")));
			return new MMDDanceModel(ctMeshContainer, dictionary);
		}
		public void SetMorphParam(string name, float level)
		{
		}
		public void UpdateMorph()
		{
		}
		public bool IsHasMorph(string name)
		{
			return false;
		}
		public void ShowOutputWarningDialog(IWin32Window owner)
		{
		}
	}
}

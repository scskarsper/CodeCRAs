using CTUtilCLR;
using Moosta.Common.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
namespace Moosta.FileIO.Stage
{
	public class StageModelFromOBJ : AbstractStageModel
	{
		protected class MsFace
		{
			public CtVector3D[] Position;
			public CtVector3D[] Normal;
			public CtVector2D[] UV;
			public CtMaterial Material;
			public MsFace(int polygonCount, CtMaterial material)
			{
				this.Position = new CtVector3D[polygonCount];
				this.Normal = new CtVector3D[polygonCount];
				this.UV = new CtVector2D[polygonCount];
				this.Material = material;
			}
		}
		protected class MsRenderMesh
		{
			public List<CtVector3D> renderPosition = new List<CtVector3D>();
			public List<CtVector3D> renderNormal = new List<CtVector3D>();
			public List<CtVector2D> renderUV = new List<CtVector2D>();
			public CtMaterial material = new CtMaterial();
		}
		private int thumbnailTextureIndex = -1;
		private Stopwatch renderStageSW = new Stopwatch();
		private string objFileName = "";
		protected Dictionary<string, CtMaterial> materials = new Dictionary<string, CtMaterial>();
		protected Dictionary<string, StageModelFromOBJ.MsRenderMesh> renderMeshes = new Dictionary<string, StageModelFromOBJ.MsRenderMesh>();
		public override bool IsUserSettable
		{
			get
			{
				return false;
			}
		}
		public override int ThumbnailTextureIndex
		{
			get
			{
				return this.thumbnailTextureIndex;
			}
		}
		public override string ThumbnailFileName
		{
			get
			{
				return "";
			}
		}
		public string ObjFileName
		{
			get
			{
				return this.objFileName;
			}
			set
			{
				this.objFileName = value;
			}
		}
		public override string ContentFileName
		{
			get
			{
				return this.ObjFileName;
			}
		}
		public override void RenderModel(CtGraphicsInterface gi)
		{
			this.renderStageSW.Reset();
			this.renderStageSW.Start();
			foreach (StageModelFromOBJ.MsRenderMesh current in this.renderMeshes.Values)
			{
				gi.SetRenderMaterial(current.material);
				gi.RenderTriangles(current.renderPosition.ToArray(), current.renderNormal.ToArray(), current.renderUV.ToArray());
			}
			this.renderStageSW.Stop();
		}
		public override void RenderLightEffect(CtGraphicsInterface gi)
		{
		}
		public override void AnimateStage()
		{
		}
		public StageModelFromOBJ(string fileName)
		{
			this.objFileName = fileName;
		}
		public override void LoadResource(CtGraphicsInterface gi)
		{
			using (StreamReader streamReader = new StreamReader(this.ObjFileName, Encoding.GetEncoding("Shift_JIS")))
			{
				this.ReadObj(streamReader, Path.GetDirectoryName(this.ObjFileName), gi);
			}
			this.LoadTextures(gi, Path.GetDirectoryName(this.ObjFileName));
		}
		public override void ReleaseResource()
		{
			foreach (CtMaterial current in this.materials.Values)
			{
				if (current.TextureIdNumber >= 0)
				{
					CtTextureManager.ReleaseTexture((uint)current.TextureIdNumber);
				}
			}
			foreach (StageBackGround current2 in this.stageBgList)
			{
				if (current2.BackGroundTextureIDNumber >= 0)
				{
					CtTextureManager.ReleaseTexture((uint)current2.BackGroundTextureIDNumber);
				}
			}
			if (this.dropObject != null && this.dropObject.ObjectTextureIndex >= 0)
			{
				CtTextureManager.ReleaseTexture((uint)this.dropObject.ObjectTextureIndex);
			}
			this.materials.Clear();
			this.renderMeshes.Clear();
			GC.Collect();
		}
		public void ReadObj(TextReader objTr, string directory, CtGraphicsInterface gi)
		{
			this.materials.Clear();
			List<CtVector3D> list = new List<CtVector3D>();
			List<CtVector3D> list2 = new List<CtVector3D>();
			List<CtVector2D> list3 = new List<CtVector2D>();
			string text = "";
			float f = 0.1f;
			CtVector3D b = new CtVector3D(0f, 0f, -16f);
			string path = Path.Combine(directory, "setting.txt");
			this.stageBgList.Clear();
			try
			{
				using (StreamReader streamReader = new StreamReader(path))
				{
					while (streamReader.Peek() > -1)
					{
						try
						{
							string text2 = streamReader.ReadLine();
							if (text2.Length <= 0 || text2[0] != '#')
							{
								string[] array = text2.Split(new char[]
								{
									':'
								});
								if (array[0].Equals("Scale") && array.Length == 2)
								{
									f = float.Parse(array[1]);
								}
								else if (array[0].Equals("Diff") && array.Length == 2)
								{
									string[] array2 = array[1].Split(new char[]
									{
										','
									});
									if (array2.Length == 3)
									{
										b.X = float.Parse(array2[0]);
										b.Y = float.Parse(array2[1]);
										b.Z = float.Parse(array2[2]);
									}
								}
								else if (array[0].Equals("Lighting") && array.Length == 2)
								{
									base.LightingModeName = array[1];
								}
								else if (array[0].Equals("BackGroundImage") && array.Length == 3)
								{
									StageBackGround stageBackGround = new StageBackGround();
									stageBackGround.TranslateScale = float.Parse(array[1]);
									stageBackGround.FileName = array[2];
									string fileName = Path.Combine(Path.GetDirectoryName(this.ObjFileName), stageBackGround.FileName);
									stageBackGround.BackGroundTextureIDNumber = CtTextureManager.CreateTexture(gi, fileName);
									CtTexture texture = CtTextureManager.GetTexture(stageBackGround.BackGroundTextureIDNumber);
									if (texture != null)
									{
										texture.DeleteBitmapResource();
									}
									this.stageBgList.Add(stageBackGround);
								}
								else if (array[0].Equals("DropObjectImage") && array.Length == 4)
								{
									this.dropObject = new StageDropObject();
									this.dropObject.ObjectCount = int.Parse(array[1]);
									this.dropObject.ObjectSize = float.Parse(array[2]);
									this.dropObject.FileName = array[3];
									string fileName2 = Path.Combine(Path.GetDirectoryName(this.ObjFileName), this.dropObject.FileName);
									this.dropObject.ObjectTextureIndex = CtTextureManager.CreateTexture(gi, fileName2);
									CtTexture texture2 = CtTextureManager.GetTexture(this.dropObject.ObjectTextureIndex);
									if (texture2 != null)
									{
										texture2.DeleteBitmapResource();
									}
								}
								else if (array[0].Equals("DropObjectSpeedRange") && array.Length == 7)
								{
									if (this.dropObject != null)
									{
										CtVector3D minSpeedRange = default(CtVector3D);
										CtVector3D maxSpeedRange = default(CtVector3D);
										minSpeedRange.X = float.Parse(array[1]);
										maxSpeedRange.X = float.Parse(array[2]);
										minSpeedRange.Y = float.Parse(array[3]);
										maxSpeedRange.Y = float.Parse(array[4]);
										minSpeedRange.Z = float.Parse(array[5]);
										maxSpeedRange.Z = float.Parse(array[6]);
										this.dropObject.MinSpeedRange = minSpeedRange;
										this.dropObject.MaxSpeedRange = maxSpeedRange;
									}
								}
								else if (array[0].Equals("DropObjectSpace") && array.Length == 5 && this.dropObject != null)
								{
									this.dropObject.ObjectFieldScale = float.Parse(array[1]);
									this.dropObject.ObjectCreateHeight = float.Parse(array[2]);
									this.dropObject.ObjectDeleteHeight = float.Parse(array[3]);
									this.dropObject.ObjectGroundHeight = float.Parse(array[4]);
								}
							}
						}
						catch
						{
						}
					}
				}
			}
			catch
			{
			}
			CtMaterial material = null;
			StageModelFromOBJ.MsRenderMesh msRenderMesh = null;
			while (objTr.Peek() > -1)
			{
				string text3 = objTr.ReadLine().Trim();
				if (!string.IsNullOrEmpty(text3))
				{
					try
					{
						if (text3[0] != '#')
						{
							string[] array3 = text3.Split(new char[]
							{
								' '
							});
							if (array3[0].Equals("v"))
							{
								CtVector3D ctVector3D = new CtVector3D(float.Parse(array3[1]), float.Parse(array3[2]), float.Parse(array3[3]));
								ctVector3D *= f;
								ctVector3D += b;
								list.Add(ctVector3D);
							}
							else if (array3[0].Equals("vt"))
							{
								CtVector2D item = new CtVector2D(float.Parse(array3[1]), float.Parse(array3[2]));
								list3.Add(item);
							}
							else if (array3[0].Equals("vn"))
							{
								CtVector3D item2 = new CtVector3D(float.Parse(array3[1]), float.Parse(array3[2]), float.Parse(array3[3]));
								list2.Add(item2);
							}
							else if (array3[0].Equals("f"))
							{
								StageModelFromOBJ.MsFace msFace = new StageModelFromOBJ.MsFace(array3.Length - 1, material);
								for (int i = 1; i < array3.Length; i++)
								{
									string[] array4 = array3[i].Split(new char[]
									{
										'/'
									});
									if (array4.Length == 3)
									{
										msFace.Position[i - 1] = list[int.Parse(array4[0]) - 1];
										msFace.UV[i - 1] = list3[int.Parse(array4[1]) - 1];
										msFace.Normal[i - 1] = list2[int.Parse(array4[2]) - 1];
									}
									else
									{
										int arg_5C9_0 = array4.Length;
									}
								}
								if (msFace.Position.Length == 3)
								{
									msRenderMesh.renderPosition.Add(msFace.Position[0]);
									msRenderMesh.renderNormal.Add(msFace.Normal[0]);
									msRenderMesh.renderUV.Add(msFace.UV[0]);
									msRenderMesh.renderPosition.Add(msFace.Position[2]);
									msRenderMesh.renderNormal.Add(msFace.Normal[2]);
									msRenderMesh.renderUV.Add(msFace.UV[2]);
									msRenderMesh.renderPosition.Add(msFace.Position[1]);
									msRenderMesh.renderNormal.Add(msFace.Normal[1]);
									msRenderMesh.renderUV.Add(msFace.UV[1]);
								}
								else if (msFace.Position.Length == 4)
								{
									msRenderMesh.renderPosition.Add(msFace.Position[0]);
									msRenderMesh.renderNormal.Add(msFace.Normal[0]);
									msRenderMesh.renderUV.Add(msFace.UV[0]);
									msRenderMesh.renderPosition.Add(msFace.Position[2]);
									msRenderMesh.renderNormal.Add(msFace.Normal[2]);
									msRenderMesh.renderUV.Add(msFace.UV[2]);
									msRenderMesh.renderPosition.Add(msFace.Position[1]);
									msRenderMesh.renderNormal.Add(msFace.Normal[1]);
									msRenderMesh.renderUV.Add(msFace.UV[1]);
									msRenderMesh.renderPosition.Add(msFace.Position[3]);
									msRenderMesh.renderNormal.Add(msFace.Normal[3]);
									msRenderMesh.renderUV.Add(msFace.UV[3]);
									msRenderMesh.renderPosition.Add(msFace.Position[2]);
									msRenderMesh.renderNormal.Add(msFace.Normal[2]);
									msRenderMesh.renderUV.Add(msFace.UV[2]);
									msRenderMesh.renderPosition.Add(msFace.Position[0]);
									msRenderMesh.renderNormal.Add(msFace.Normal[0]);
									msRenderMesh.renderUV.Add(msFace.UV[0]);
								}
							}
							else if (array3[0].Equals("usemtl"))
							{
								CtMaterial ctMaterial;
								if (!this.materials.TryGetValue(array3[1], out ctMaterial))
								{
									ctMaterial = new CtMaterial();
									ctMaterial.Name = array3[1];
									this.materials.Add(ctMaterial.Name, ctMaterial);
									ctMaterial.TextureIdNumber = -1;
								}
								material = ctMaterial;
								StageModelFromOBJ.MsRenderMesh msRenderMesh2 = null;
								if (!this.renderMeshes.TryGetValue(array3[1], out msRenderMesh2))
								{
									msRenderMesh2 = new StageModelFromOBJ.MsRenderMesh();
									this.renderMeshes.Add(ctMaterial.Name, msRenderMesh2);
								}
								msRenderMesh = msRenderMesh2;
								msRenderMesh.material = material;
							}
							else if (array3[0].Equals("mtllib"))
							{
								text = array3[1];
							}
						}
					}
					catch (Exception)
					{
					}
				}
			}
			if (File.Exists(directory + "\\" + text) && !string.IsNullOrEmpty(text))
			{
				using (StreamReader streamReader2 = new StreamReader(directory + "\\" + text, Encoding.GetEncoding("Shift_JIS")))
				{
					this.readMaterial(streamReader2);
				}
			}
		}
		private void readMaterial(TextReader objTr)
		{
			CtMaterial ctMaterial = null;
			while (objTr.Peek() > -1)
			{
				string text = objTr.ReadLine().Trim();
				if (!string.IsNullOrEmpty(text))
				{
					try
					{
						if (text[0] != '#')
						{
							string[] array = text.Split(new char[]
							{
								' '
							});
							if (array[0].Equals("newmtl"))
							{
								CtMaterial ctMaterial2 = null;
								if (this.materials.TryGetValue(array[1], out ctMaterial2))
								{
									ctMaterial = ctMaterial2;
								}
							}
							else if (array[0].Equals("Ka"))
							{
								if (ctMaterial != null)
								{
									ctMaterial.Ambient = Color.FromArgb((int)CtMath.Clamp(float.Parse(array[1]) * 255f, 0f, 255f), (int)CtMath.Clamp(float.Parse(array[2]) * 255f, 0f, 255f), (int)CtMath.Clamp(float.Parse(array[3]) * 255f, 0f, 255f));
								}
							}
							else if (array[0].Equals("Kd"))
							{
								if (ctMaterial != null)
								{
									ctMaterial.Diffuse = Color.FromArgb((int)CtMath.Clamp(float.Parse(array[1]) * 255f, 0f, 255f), (int)CtMath.Clamp(float.Parse(array[2]) * 255f, 0f, 255f), (int)CtMath.Clamp(float.Parse(array[3]) * 255f, 0f, 255f));
								}
							}
							else if (array[0].Equals("Ks"))
							{
								if (ctMaterial != null)
								{
									ctMaterial.Specular = Color.FromArgb((int)CtMath.Clamp(float.Parse(array[1]) * 255f, 0f, 255f), (int)CtMath.Clamp(float.Parse(array[2]) * 255f, 0f, 255f), (int)CtMath.Clamp(float.Parse(array[3]) * 255f, 0f, 255f));
								}
							}
							else if (array[0].Equals("Ns"))
							{
								if (ctMaterial != null)
								{
									ctMaterial.SpecularPower = float.Parse(array[1]);
								}
							}
							else if (array[0].Equals("map_Kd") && ctMaterial != null)
							{
								ctMaterial.TextureFile = array[1];
							}
						}
					}
					catch
					{
					}
				}
			}
		}
		public void LoadTextures(CtGraphicsInterface gi, string textureDirectory)
		{
			foreach (CtMaterial current in this.materials.Values)
			{
				string text = textureDirectory + "\\" + current.TextureFile;
				if (File.Exists(text))
				{
					current.TextureIdNumber = CtTextureManager.CreateTexture(gi, text);
					CtTexture texture = CtTextureManager.GetTexture(current.TextureIdNumber);
					if (texture != null)
					{
						texture.DeleteBitmapResource();
					}
				}
			}
		}
		public void LoadThumbnail(CtGraphicsInterface gi, string fileName)
		{
			if (File.Exists(fileName))
			{
				this.thumbnailTextureIndex = CtTextureManager.CreateTexture(gi, fileName);
			}
		}
	}
}

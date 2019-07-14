using UnityEngine;
using System.Collections.Generic;
using Anima2D;

public class SkinnedMeshCombiner : MonoBehaviour
{
	[SerializeField]
	private SpriteMeshInstance[] m_SpriteMeshInstances;
	private MaterialPropertyBlock m_MaterialPropertyBlock;
	private SkinnedMeshRenderer m_CachedSkinnedRenderer;

	public SpriteMeshInstance[] spriteMeshInstances
	{
		get { return m_SpriteMeshInstances; }
		set { m_SpriteMeshInstances = value; }
	}

	private MaterialPropertyBlock materialPropertyBlock
	{
		get {
			if(m_MaterialPropertyBlock == null)
			{
				m_MaterialPropertyBlock = new MaterialPropertyBlock();
			}
			
			return m_MaterialPropertyBlock;
		}
	}

	public SkinnedMeshRenderer cachedSkinnedRenderer
	{
		get
		{
			if(!m_CachedSkinnedRenderer)
			{
				m_CachedSkinnedRenderer = GetComponent<SkinnedMeshRenderer>();
			}
			
			return m_CachedSkinnedRenderer;
		}
	}

	void Start()
	{        
		var l_position = transform.position;
		var l_rotation = transform.rotation;
		var l_scale = transform.localScale;

		transform.position = Vector3.zero;
		transform.rotation = Quaternion.identity;
		transform.localScale = Vector3.one;

		var bones = new List<Transform>();        
		var boneWeights = new List<BoneWeight>();        
		var combineInstances = new List<CombineInstance>();

		var numSubmeshes = 0;
		
		for (var i = 0; i < spriteMeshInstances.Length; i++)
		{
			var spriteMeshInstance = spriteMeshInstances[i];

			if(spriteMeshInstance.cachedSkinnedRenderer)
			{
				numSubmeshes += spriteMeshInstance.mesh.subMeshCount;
			}
		}
		
		var meshIndex = new int[numSubmeshes];
		var boneOffset = 0;
		for( var i = 0; i < m_SpriteMeshInstances.Length; ++i)
		{
			var spriteMeshInstance = spriteMeshInstances[i];

			if(spriteMeshInstance.cachedSkinnedRenderer)
			{
				var skinnedMeshRenderer = spriteMeshInstance.cachedSkinnedRenderer;          

				var meshBoneweight = spriteMeshInstance.sharedMesh.boneWeights;
				
				// May want to modify this if the renderer shares bones as unnecessary bones will get added.
				for (var j = 0; j < meshBoneweight.Length; ++j)
				{
					var bw = meshBoneweight[j];
					var bWeight = bw;
					bWeight.boneIndex0 += boneOffset;
					bWeight.boneIndex1 += boneOffset;
					bWeight.boneIndex2 += boneOffset;
					bWeight.boneIndex3 += boneOffset;
					boneWeights.Add (bWeight);
				}

				boneOffset += spriteMeshInstance.bones.Count;
				
				var meshBones = skinnedMeshRenderer.bones;
				for (var j = 0; j < meshBones.Length; j++)
				{
					var bone = meshBones[j];
					bones.Add (bone);
				}

				var combineInstance = new CombineInstance();
				var mesh = new Mesh();
				skinnedMeshRenderer.BakeMesh(mesh);
				mesh.uv = spriteMeshInstance.spriteMesh.sprite.uv;
				combineInstance.mesh = mesh;
				meshIndex[i] = combineInstance.mesh.vertexCount;
				combineInstance.transform = skinnedMeshRenderer.localToWorldMatrix;
				combineInstances.Add(combineInstance);
				
				skinnedMeshRenderer.gameObject.SetActive(false);
			}
		}
		
		var bindposes = new List<Matrix4x4>();
		
		for( var b = 0; b < bones.Count; b++ ) {
			bindposes.Add( bones[b].worldToLocalMatrix * transform.worldToLocalMatrix );
		}
		
		var combinedSkinnedRenderer = gameObject.AddComponent<SkinnedMeshRenderer>();
		var combinedMesh = new Mesh();
		combinedMesh.CombineMeshes( combineInstances.ToArray(), true, true );
		combinedSkinnedRenderer.sharedMesh = combinedMesh;
		combinedSkinnedRenderer.bones = bones.ToArray();
		combinedSkinnedRenderer.sharedMesh.boneWeights = boneWeights.ToArray();
		combinedSkinnedRenderer.sharedMesh.bindposes = bindposes.ToArray();
		combinedSkinnedRenderer.sharedMesh.RecalculateBounds();

		combinedSkinnedRenderer.materials = spriteMeshInstances[0].sharedMaterials;

		transform.position = l_position;
		transform.rotation = l_rotation;
		transform.localScale = l_scale;
	}

	void OnWillRenderObject()
	{
		if(cachedSkinnedRenderer)
		{
			if(materialPropertyBlock != null)
			{
				materialPropertyBlock.SetTexture("_MainTex", spriteMeshInstances[0].spriteMesh.sprite.texture);
				
				cachedSkinnedRenderer.SetPropertyBlock(materialPropertyBlock);
			}
		}
	}
}
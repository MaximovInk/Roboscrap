using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Anima2D
{
	[Serializable]
	public class BlendShape : ScriptableObject
	{
		public BlendShapeFrame[] frames = new BlendShapeFrame[0];
		
		public static BlendShape Create(string name)
		{
			var blendShape = ScriptableObject.CreateInstance<BlendShape>();
			blendShape.name = name;
			return blendShape;
		}
	}
}

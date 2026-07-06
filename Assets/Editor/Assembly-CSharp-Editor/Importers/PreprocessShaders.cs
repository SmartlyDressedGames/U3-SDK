////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;

namespace SDG.Unturned
{
	public class PreprocessShaders : IPreprocessShaders
	{
		public int callbackOrder => 0;

		public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
		{
#if DEDICATED_SERVER
			data.Clear();
#endif // DEDICATED_SERVER
		}
	}
}

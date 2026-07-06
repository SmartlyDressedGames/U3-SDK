////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public static class ShaderConsolidator
	{
		/// <summary>
		/// Apply shader name redirects until a final name is found,
		/// and then load shader for compatible version of Unity.
		/// </summary>
		public static Shader findConsolidatedShader(Shader originalShader)
		{
			if (originalShader == null)
				return null;

			string originalShaderName = originalShader.name;
			if (string.IsNullOrEmpty(originalShaderName))
				return null;

			if (Dedicator.IsDedicatedServer)
				throw new System.Exception(string.Format("Dedicated server trying to consolidate '{0}' shader", originalShaderName));

			string consolidatedShaderName = redirectShaderName(originalShaderName);
			return Shader.Find(consolidatedShaderName);
		}

		/// <summary>
		/// Apply shader name redirects until a final name is found.
		/// Used to fix renamed shaders loaded from old asset bundles.
		/// </summary>
		public static string redirectShaderName(string shaderName)
		{
			string redirectedName;
			if (SHADER_REDIRECTS.TryGetValue(shaderName, out redirectedName))
			{
				// Recursively test for redirects until shaderName is valid.
				return redirectShaderName(redirectedName);
			}
			else
			{
				return shaderName;
			}
		}

		/// <summary>
		/// Names of older shaders mapped to their renamed counterparts.
		/// Used to fix shaders loaded from old asset bundles.
		/// </summary>
		private static readonly Dictionary<string, string> SHADER_REDIRECTS = new Dictionary<string, string>
		{
			{ "Particles/Additive", "Legacy Shaders/Particles/Additive" }, // Particle Add
			{ "Particles/Additive (Soft)", "Legacy Shaders/Particles/Additive (Soft)" }, // Particle AddSmooth
			{ "Particles/Alpha Blended", "Legacy Shaders/Particles/Alpha Blended" }, // Particle Alpha Blend
			{ "Particles/Anim Alpha Blended", "Legacy Shaders/Particles/Anim Alpha Blended" }, // Particle Anim Alpha Blend
			{ "Particles/Alpha Blended Premultiply", "Legacy Shaders/Particles/Alpha Blended Premultiply" }, // Particle Premultiply Blend
		};
	}
}

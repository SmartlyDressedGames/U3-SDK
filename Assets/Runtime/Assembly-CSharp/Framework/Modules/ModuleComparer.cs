////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Framework.Modules
{
	/// <summary>
	/// Sorts modules by dependencies.
	/// </summary>
	public class ModuleComparer : IComparer<ModuleConfig>
	{
		public int Compare(ModuleConfig x, ModuleConfig y)
		{
			// if Y depends on X then X should load before 
			for (int index = 0; index < y.Dependencies.Count; index++)
			{
				ModuleDependency dependency = y.Dependencies[index];

				if (dependency.Name == x.Name)
				{
					return -1;
				}
			}

			// if X depends on Y then Y should load before 
			for (int index = 0; index < x.Dependencies.Count; index++)
			{
				ModuleDependency dependency = x.Dependencies[index];

				if (dependency.Name == y.Name)
				{
					return 1;
				}
			}

			// whichever has the least dependencies should load before
			return x.Dependencies.Count - y.Dependencies.Count;
		}
	}
}
////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.Events;

namespace Unturned.UnityEx
{
	public static class UnityEventEx
	{
		public static void TryInvoke(this UnityEvent unityEvent, Object context)
		{
			try
			{
				unityEvent.Invoke();
			}
			catch (System.Exception e)
			{
				Debug.LogException(e, context);
			}
		}
	}
}

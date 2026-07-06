////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR
using UnityEngine;
using Unturned.SystemEx;
using Unturned.UnityEx;

/// <summary>
/// Used to capture promotional images.
///
/// Unity does not allow components in the editor assembly, so this component is in the game assembly but only compiled in the editor.
/// </summary>
public class EditorScreenCaptureComponent : MonoBehaviour
{
	public int superSizeFactor = 2;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			string filename = PathEx.Join(UnityPaths.TempDirectory, "EditorScreenCapture.png");
			ScreenCapture.CaptureScreenshot(filename, superSizeFactor);
		}
	}

	private void Start()
	{
		QualitySettings.lodBias = 100.0f;
	}
}
#endif // UNITY_EDITOR

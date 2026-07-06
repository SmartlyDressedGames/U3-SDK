////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_STANDALONE_WIN

namespace SDG.Unturned
{
	/// <summary>
	/// Windows-specific extensions of console input.
	/// Uses the Win32 API to force a console to be created and destroyed.
	/// </summary>
	public class WindowsConsoleInputOutput : ConsoleInputOutput
	{
		public override void initialize(CommandWindow commandWindow)
		{
			WindowsConsole.conditionalAlloc();
			WindowsConsole.setCodePageToUTF8();
			WindowsConsole.RegisterCtrlHandler(OnWindowsQuitEvent);

			base.initialize(commandWindow);

			WindowsConsole.ApplyAssertionExperimentalFix();
		}

		public override void shutdown(CommandWindow commandWindow)
		{
			base.shutdown(commandWindow);

			WindowsConsole.conditionalFree();
		}

		private void OnWindowsQuitEvent(WindowsConsole.ECtrlType ctrlType)
		{
			// This is called on another thread!
			System.Threading.Interlocked.Exchange(ref wantsToTerminate, 1);
		}
	}
}

#endif // UNITY_STANDALONE_WIN

////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public delegate void CommandInputHandler(string input);

	/// <summary>
	/// Interface between the dedicated server command I/O and per-platform console.
	/// </summary>
	public interface ICommandInputOutput
	{
		/// <summary>
		/// Called when this implementation is setup by command window.
		/// </summary>
		void initialize(CommandWindow commandWindow);

		/// <summary>
		/// Called when this implementation is deleted or application quits.
		/// </summary>
		void shutdown(CommandWindow commandWindow);

		/// <summary>
		/// Called each Unity update.
		/// </summary>
		void update();

		/// <summary>
		/// Broadcasts when the enter key is pressed.
		/// </summary>
		event CommandInputHandler inputCommitted;

		/// <summary>
		/// Print white message.
		/// </summary>
		void outputInformation(string information);

		/// <summary>
		/// Print yellow message.
		/// </summary>
		void outputWarning(string warning);

		/// <summary>
		/// Print red message.
		/// </summary>
		void outputError(string error);
	}
}

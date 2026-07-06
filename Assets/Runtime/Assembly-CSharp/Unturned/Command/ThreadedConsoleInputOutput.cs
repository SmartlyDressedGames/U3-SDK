////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace SDG.Unturned
{
	public class ThreadedConsoleInputOutput : ConsoleInputOutputBase
	{
		public override void initialize(CommandWindow commandWindow)
		{
			base.initialize(commandWindow);

			inputThread = new Thread(new ThreadStart(consoleMain));
			inputThread.Start();
		}

		public override void shutdown(CommandWindow commandWindow)
		{
			base.shutdown(commandWindow);

			if (inputThread != null)
			{
				inputThread.Abort();
				inputThread = null;
			}
		}

		public override void update()
		{
			base.update();

			string input;
			while (pendingInputs.TryDequeue(out input))
			{
				notifyInputCommitted(input);
			}
		}

		protected override void outputToConsole(string value, ConsoleColor color)
		{
			PendingOutput output = new PendingOutput()
			{
				value = value,
				color = color
			};
			pendingOutputs.Enqueue(output);
		}

		private void consoleMain()
		{
			while (true)
			{
				// We check IsEmpty because repeatedly restoring color over SSH uses bandwidth.
				if (pendingOutputs.IsEmpty == false)
				{
					ConsoleColor restoreColor = Console.ForegroundColor;

					PendingOutput output;
					while (pendingOutputs.TryDequeue(out output))
					{
						Console.ForegroundColor = output.color;
						Console.WriteLine(output.value);
					}

					Console.ForegroundColor = restoreColor;
				}

				if (Console.KeyAvailable)
				{
					string input = Console.ReadLine();
					if (string.IsNullOrWhiteSpace(input) == false)
					{
						pendingInputs.Enqueue(input);
					}
				}

				// Sleep to avoid using 100% CPU.
				Thread.Sleep(10);
			}
		}

		private struct PendingOutput
		{
			public string value;
			public ConsoleColor color;
		}

		private Thread inputThread;
		private ConcurrentQueue<string> pendingInputs = new ConcurrentQueue<string>();
		private ConcurrentQueue<PendingOutput> pendingOutputs = new ConcurrentQueue<PendingOutput>();
	}
}

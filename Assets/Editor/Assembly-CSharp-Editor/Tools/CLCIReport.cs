////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Command Line Continuous Integration Report
/// Log information directly to the continuous integration console.
/// Allows us to display only logs relevant to the game (Unity's log is pretty verbose)
/// </summary>
public static class CLCIReport
{
	private static string getReportPath()
	{
		return Unturned.UnityEx.UnityPaths.ProjectDirectory.FullName + "/Build_Scripts/CI_Report.txt";
	}

	private static void writeLine(string text)
	{
		File.AppendAllText(getReportPath(), text + Environment.NewLine);
	}

	public static void log(string text)
	{
		writeLine(text);
		Debug.Log(text);
	}

	public static void logWarning(string text)
	{
		writeLine(text);
		Debug.LogWarning(text);
	}

	public static void logError(string text)
	{
		writeLine(text);
		Debug.LogError(text);
	}

	public static void logTestFailure(string text)
	{
		writeLine(text);
	}

	static CLCIReport()
	{
		// Ensure we have an empty report no matter where we start logging from.
		string reportPath = getReportPath();
		if (File.Exists(reportPath))
		{
			File.Delete(reportPath);
		}
	}
}

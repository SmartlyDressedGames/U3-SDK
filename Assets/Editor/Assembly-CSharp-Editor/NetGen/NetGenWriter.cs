////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.IO;

public class NetGenWriter
{
	public void Indent()
	{
		indentationPrefix += '\t';
	}

	public void Outdent()
	{
		indentationPrefix = indentationPrefix.Remove(indentationPrefix.Length - 1, 1);
	}

	public void WriteLine(char value)
	{
		writer.WriteLine(indentationPrefix + value);
	}

	public void WriteLine(string value)
	{
		writer.WriteLine(indentationPrefix + value);
	}

	public void WritePreprocessorLine(string value)
	{
		writer.WriteLine(value);
	}

	public NetGenWriter(TextWriter writer)
	{
		this.writer = writer;
	}

	private TextWriter writer;
	private string indentationPrefix;
}

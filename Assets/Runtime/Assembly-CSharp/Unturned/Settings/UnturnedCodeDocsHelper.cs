////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.IO;
using System.Xml;
using Unturned.SystemEx;
using Unturned.UnityEx;

namespace SDG.Unturned
{
	internal class UnturnedCodeDocsHelper
	{
		public string GetSummary(string className, string fieldName)
		{
			if (documentation == null)
			{
				return null;
			}

			string xpath = $"//member[@name='F:SDG.Unturned.{className}.{fieldName}']/summary";
			XmlNode memberNode = documentation.SelectSingleNode(xpath);
			if (memberNode == null || string.IsNullOrEmpty(memberNode.InnerText))
			{
				return null;
			}
			return memberNode.InnerText.Trim('\r', '\n');
		}

		public UnturnedCodeDocsHelper()
		{
			string docPath;
#if !UNITY_EDITOR
			docPath = PathEx.Join(UnityPaths.GameDataDirectory, "Managed", "Assembly-CSharp.xml");
#else
			docPath = PathEx.Join(UnityPaths.ProjectDirectory, "Builds", "Windows64", "Unturned_Data", "Managed", "Assembly-CSharp.xml");
#endif
			try
			{
				if (File.Exists(docPath))
				{
					documentation = new XmlDocument();
					documentation.Load(docPath);
				}
			}
			catch (System.Exception exception)
			{
				documentation = null;
				UnturnedLog.exception(exception, "Caught exception loading code documentation:");
			}
		}

		private XmlDocument documentation;
	}
}

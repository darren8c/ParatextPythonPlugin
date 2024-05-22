using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Paratext.PluginInterfaces;

namespace ParatextPythonPlugin
{
    /// <summary>
    /// This plugin will show multiple ways to have python run from c#, including passing data
	/// between python and c# methods
    /// 
    /// For this plugin, python scripts can be placed in the scripts folder.  Set the file typ to
	/// "Embedded Resource" in the properties menu for the file by right clicking on the file.
	/// Also, set it to copy when newer.
	/// 
    /// The main work is done in MainControl.cs -> RunPython
    /// </summary>
    public class ParatextPythonPluginControl : IParatextWindowPlugin
	{
		public const string pluginName = "Python Plugin Tests";
		public string Name => pluginName;
		public string GetDescription(string locale) => "Show how a plugin developer can run python scripts in a plugin.";
		public Version Version => new Version(1, 0);
		public string VersionString => Version.ToString();
		public string Publisher => "SIL/UBS";

		public IEnumerable<WindowPluginMenuEntry> PluginMenuEntries
		{
			get
			{
				yield return new WindowPluginMenuEntry("Python Plugin...", Run, PluginMenuLocation.ScrTextProjectAdvanced);
			}
		}

		public IDataFileMerger GetMerger(IPluginHost host, string dataIdentifier)
		{
            return new MyMerger();
        }

		/// <summary>
		/// Called by Paratext when the menu item created for this plugin was clicked.
		/// </summary>
		private void Run(IWindowPluginHost host, IParatextChildState windowState)
		{
			host.ShowEmbeddedUi(new MainControl(), windowState.Project);
		}
	}

    public class MyMerger : IDataFileMerger
    {
        public string Merge(string theirs, string mine, string parent)
        {
            return mine;
        }
    }
}


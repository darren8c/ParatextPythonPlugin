using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using Paratext.PluginInterfaces;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting;
using System.IO;
using System.Reflection;

namespace ParatextPythonPlugin
{
    public partial class MainControl : EmbeddedPluginControl
    {
        private IVerseRef m_Reference;
        private IProject m_project;
        private List<IUSFMToken> m_Tokens;
        private IWriteLock m_WriteLock;

        public MainControl()
        {
            InitializeComponent();
            m_WriteLock = null;
        }

        public override void OnAddedToParent(IPluginChildWindow parent, IWindowPluginHost host, string state)
        {
            parent.SetTitle(ParatextPythonPluginControl.pluginName);

            SetProject(parent.CurrentState.Project);
            m_Reference = parent.CurrentState.VerseRef;
            m_WriteLock = null;

            parent.WindowClosing += WindowClosing;
            parent.ProjectChanged += ProjectChanged;
        }

        public override string GetState()
        {
            return null;
        }

        public override void DoLoad(IProgressInfo progressInfo)
        {
        }

        private void Unlock()
        {
            if (m_WriteLock != null)
            {
                IWriteLock temp = m_WriteLock;
                temp.Dispose();
                m_WriteLock = null;
            }
        }

        private void ReleaseRequested(IWriteLock writeLock)
        {
            Unlock();
        }

        private void WindowClosing(IPluginChildWindow sender, CancelEventArgs args)
        {
            Unlock();
        }

        private void ProjectChanged(IPluginChildWindow sender, IProject newProject)
        {
            // Save the old project first:
            Unlock();

            // Then remember the new project
            SetProject(newProject);
        }

        private void ScriptureDataChangedHandler(IProject sender, int bookNum, int chapterNum)
        {
            Unlock();
        }

        private void SetProject(IProject newProject)
        {
            if (m_project != null)
            {
                m_project.ScriptureDataChanged -= ScriptureDataChangedHandler;
            }

            m_project = newProject;
            if (newProject != null)
            {
                newProject.ScriptureDataChanged += ScriptureDataChangedHandler;
            }
        }

        public void RunPython(object sender, EventArgs e)
        {
            if (m_project == null)
            {
                MessageBox.Show("No project selected");
                return;
            }

            if (m_WriteLock != null)
            {
                MessageBox.Show("'Quit' to release current lock before getting more Scripture");
                return;
            }

            var engine = Python.CreateEngine();


            

            var theScript = @"def PrintMessage():
                                    print('This is a message!');
                                    PrintMessage()";

            // This is the simplest way to run python scripts, but there is no data exhange
            // beteen c# and python
            engine.Execute(theScript);

            dynamic scope = engine.CreateScope();
            scope.Add = new Func<int, int, int>((x, y) => x + y);
            engine.Execute(@"Add(2, 3)", scope);
            MessageBox.Show(scope.Add(2, 3).ToString()); // shows 5
            // This shows how to add a c# function to the scope, and you can access it from python

            //reset for the next example
            scope = engine.CreateScope();

            // The python scripts go in the scripts folder, and the type needs to change to "Embedded Resources" in the file properties.
            // The scripts folder is copied/installed with the plugin.  
            // This code reads the python file and exeutes it.
            engine.Execute(new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("ParatextPythonPlugin.scripts.test.py")).ReadToEnd(), scope);
            
            // test_func is a funtion defined in python.  Dynamic c# is unknown when compiled, but known at runtime.
            dynamic testFunction = scope.GetVariable("test_func");

            string var1 = "var1";
            string var2 = "var2";
            // testFunction is the variable for they python function defined above.  We can pass data into it.
            // and we can get data retuned from the Python method.
            var result = testFunction(var1, var2);

            // C# message box to show what was returned from python.
            MessageBox.Show(result);

            //if (m_WriteLock == null)
            //{
            //    Unlock();
            //    MessageBox.Show("Unable to get a Write Lock");
            //}
        }
    }

    class TextToken : IUSFMTextToken
    {
        public TextToken(IUSFMTextToken token)
        {
            Text = token.Text;
            VerseRef = token.VerseRef;
            VerseOffset = token.VerseOffset;
            IsSpecial = token.IsSpecial;
            IsFigure = token.IsFigure;
            IsFootnoteOrCrossReference = token.IsFootnoteOrCrossReference;
            IsScripture = token.IsScripture;
            IsMetadata = token.IsMetadata;
            IsPublishableVernacular = token.IsPublishableVernacular;
        }

        public string Text { get; set; }

        public IVerseRef VerseRef { get; set; }

        public int VerseOffset { get; set; }

        public bool IsSpecial { get; set; }

        public bool IsFigure { get; set; }

        public bool IsFootnoteOrCrossReference { get; set; }

        public bool IsScripture { get; set; }

        public bool IsMetadata { get; set; }

        public bool IsPublishableVernacular { get; set; }
    }
}

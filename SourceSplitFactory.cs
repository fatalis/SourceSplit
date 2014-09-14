using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using LiveSplit.SourceSplit;
using LiveSplit.UI.Components;
using System;
using LiveSplit.Model;

[assembly: ComponentFactory(typeof(SourceSplitFactory))]

namespace LiveSplit.SourceSplit
{
    public class SourceSplitFactory : IComponentFactory
    {
        private SourceSplitComponent _instance;

        public string ComponentName
        {
            get { return "SourceSplit"; }
        }

        public string Description
        {
            get { return "Game Time / Auto-splitting for Source engine games.";  }
        }

        public ComponentCategory Category
        {
            get { return ComponentCategory.Control; }
        }

        public IComponent Create(LiveSplitState state)
        {
            // hack to prevent double loading
            string caller = new StackFrame(1).GetMethod().Name;
            bool createAsLayoutComponent = (caller == "LoadLayoutComponent" || caller == "AddComponent");

            // if component is already loaded somewhere else
            if (_instance != null && !_instance.Disposed)
            {
                MessageBox.Show(
                    "SourceSplit is already loaded in the " +
                        (_instance.IsLayoutComponent ? "Layout Editor" : "Splits Editor") + "!",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

                throw new Exception("Component already loaded.");
            }

            return (_instance = new SourceSplitComponent(state, createAsLayoutComponent));
        }

        public string UpdateName
        {
            get { return this.ComponentName; }
        }

        public string UpdateURL
        {
            get { return "http://fatalis.hive.ai/livesplit/update/"; }
        }

        public Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public string XMLURL
        {
            get { return this.UpdateURL + "Components/update.LiveSplit.SourceSplit.xml"; }
        }
    }
}

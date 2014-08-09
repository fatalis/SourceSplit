using System.Reflection;
using LiveSplit.UI.Components;
using System;
using LiveSplit.Model;

namespace LiveSplit.SourceSplit
{
    public class SourceSplitFactory : IComponentFactory
    {
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
            return new SourceSplitComponent(state);
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

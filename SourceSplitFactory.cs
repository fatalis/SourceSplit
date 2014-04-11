using System.Reflection;
using LiveSplit.UI.Components;
using System;
using LiveSplit.Model;

namespace LiveSplit.SourceSplit
{
    public class SourceSplitFactory : IComponentFactory
    {
        private SourceSplitComponent _instance;

        public string ComponentName
        {
            get { return "SourceSplit"; }
        }

        public IComponent Create(LiveSplitState state)
        {
            // TODO: in LiveSplit 1.4, components will be IDisposable
            // this assumes the passed state is always the same one, until then
            return _instance ?? (_instance = new SourceSplitComponent(state));

            // return new SourceSplitComponent(state);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.Utils
{
    class TryMany
    {
        private Func<bool> _false;
        private List<Action> _actions;

        public TryMany(Func<bool> @false, params Action[] actions)
        {
            _false = @false;
            _actions = new List<Action>();
            actions.ToList().ForEach(x => _actions.Add(x));
        }

        public void Begin()
        {
            for (int i = 0; i < _actions.Count() && _false(); i++)
                _actions[i]();
        }
    }
}

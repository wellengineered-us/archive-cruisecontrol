using System.Collections.Generic;
using System.Xml.Linq;

namespace WellEngineered.CruiseControl.Core.Config.Preprocessor.ElementProcessors
{
    internal class ScopeProcessor : ElementProcessor
    {
        public ScopeProcessor(PreprocessorEnvironment env)
            : base(env._Settings.Namespace.GetName("scope"), env)
        {
        }

        public override IEnumerable<XNode> Process(XNode node)
        {
            // Scope causes a new stack frame for local definitions.
            return this._Env.Call(() =>
                                  {
                                      XElement element = _AssumeElement(node);
                                      /* Treat any attributes on the scope element as local symbol definitions */
                                      this._DefineFromAttributes(element);
                                      return this._ProcessNodes(element.Nodes());
                                  });
        }
    }
}

using System.Collections.Generic;
using System.Xml.Linq;

namespace WellEngineered.CruiseControl.Core.Config.Preprocessor.ElementProcessors
{
    internal class ConfigTemplateProcessor : ElementProcessor
    {
        public ConfigTemplateProcessor(PreprocessorEnvironment env)
            : base( env._Settings.Namespace.GetName("config-template"), env )
        {
        }

        public override IEnumerable< XNode > Process(XNode node)
        {
            return this._ProcessNodes( _AssumeElement( node ).Nodes() );
        }
    }
}

using System.Collections.Generic;
using System.Xml.Linq;

namespace WellEngineered.CruiseControl.Core.Config.Preprocessor.ElementProcessors
{
    internal class IfDefProcessor : ConditionalProcessor
    {
        public IfDefProcessor(PreprocessorEnvironment env)
            : this(env._Settings.Namespace.GetName("ifdef"), env)
        {
        }

        protected IfDefProcessor(XName elementName, PreprocessorEnvironment env)
            : base( elementName, env )
        {
        }

        public override IEnumerable< XNode > Process(XNode node)
        {
            XElement element = _AssumeElement( node );
            Validation.RequireAttributes( element, AttrName.Name );
            var name = ( string ) element.Attribute( AttrName.Name );
            bool defined = this._TestCondition( name );
            return this._ProcessConditional( element, defined );
        }

        protected virtual bool _TestCondition(string name)
        {
            return this._Env.IsDefined( name );
        }
    }
}

using System.Collections.Generic;
using System.Xml.Linq;

namespace WellEngineered.CruiseControl.Core.Config.Preprocessor.ElementProcessors
{
    internal class IfProcessor : ConditionalProcessor
    {
        public IfProcessor(PreprocessorEnvironment env)
            : base(env._Settings.Namespace.GetName("if"), env)
        {
        }

        public override IEnumerable< XNode > Process(XNode node)
        {
            XElement element = _AssumeElement( node );
            Validation.RequireAttributes( element, AttrName.Expr );
            var expr = ( string ) element.Attribute( AttrName.Expr );
            return this._ProcessConditional( element,
                                        this._Env.EvalBool( this._ProcessText( expr ).GetTextValue() ) );
        }
    }
}

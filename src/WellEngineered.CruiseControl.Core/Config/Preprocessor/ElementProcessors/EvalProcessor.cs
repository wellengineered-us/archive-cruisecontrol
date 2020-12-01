using System.Collections.Generic;
using System.Xml.Linq;

namespace WellEngineered.CruiseControl.Core.Config.Preprocessor.ElementProcessors
{
    internal class EvalProcessor : ElementProcessor
    {
        public EvalProcessor(PreprocessorEnvironment env)
            : base( env._Settings.Namespace.GetName("eval"), env)
        {
        }

        public override IEnumerable< XNode > Process(XNode node)
        {
            XElement element = _AssumeElement( node );
            Validation.RequireAttributes( element, AttrName.Expr );
            var expr = ( string ) element.Attribute( AttrName.Expr );
            /* Expand any preprocessor symbols before evaluating */
            string expanded_expr =
                this._ProcessText( expr ).GetTextValue().Trim();
            /* Evaluate JScript expression */
            IEnumerable< XNode > result_nodes = this._Env.EvalExpr( expanded_expr );
            return this._ProcessNodes( result_nodes );
        }
    }
}

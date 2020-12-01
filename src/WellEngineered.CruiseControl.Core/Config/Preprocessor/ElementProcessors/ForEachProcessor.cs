using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace WellEngineered.CruiseControl.Core.Config.Preprocessor.ElementProcessors
{
    internal class ForEachProcessor : ElementProcessor
    {
        public ForEachProcessor(PreprocessorEnvironment env)
            : base(env._Settings.Namespace.GetName("for-each"), env)
        {
        }

        public override IEnumerable< XNode > Process(XNode node)
        {
            XElement element = _AssumeElement( node );

            string iter_name =
                this._ProcessText( element.GetAttributeValue( AttrName.IteratorName ) ).GetTextValue();
            string iterator_expr =
                this._ProcessText( element.GetAttributeValue( AttrName.IteratorExpr ) ).GetTextValue();
            /* Compute the set of values to iterate over. */
            IEnumerable< XNode > iter_values = this._Env.EvalExpr( iterator_expr );
            /* Iterate over each value separately and collect the results */
            return
                iter_values.SelectMany(
                    iter_val =>
                    this._ProcessIteration( element, iter_val, iter_name ) );
        }

        private IEnumerable< XNode > _ProcessIteration(XElement element, XNode iterVal,
                                                       string iterName)
        {
            XNode val = iterVal;
            IEnumerable< XNode > results = this._Env.Call( () =>
                                                          {
                                                              /* Bind the iterator symbolic name to the current value */
                                                              if ( val is XContainer )
                                                                  /* Nodeset value */
                                                              {
                                                                  this._Env.DefineNodesetSymbol(
                                                                      iterName,
                                                                      ( ( XContainer ) val ).
                                                                          Nodes() );
                                                              }
                                                              else /* Text value */
                                                              {
                                                                  this._Env.DefineTextSymbol( iterName,
                                                                                         val.
                                                                                             ToString
                                                                                             () );
                                                              }
                                                              /* Process loop body */
                                                              return this._ProcessNodes( element.Nodes() );
                                                          } );
            return results;
        }
    }
}

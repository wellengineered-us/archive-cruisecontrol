using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace WellEngineered.CruiseControl.Core.Config.Preprocessor.ElementProcessors
{
    /// <summary>
    /// Processor for the "for" looping element
    /// </summary>
    internal class ForProcessor : ElementProcessor
    {
        public ForProcessor(PreprocessorEnvironment env)
            : base(env._Settings.Namespace.GetName("for"), env)
        {
        }

        public override IEnumerable< XNode > Process(XNode node)
        {
            XElement element = _AssumeElement( node );
            Validation.RequireAttributes( element, AttrName.CounterName, AttrName.InitExpr,
                                          AttrName.TestExpr, AttrName.CountExpr );
            // Get the attributes representing the counter name, initialization expression, termination test
            // expression, and counter-increment expression.  General, this translates to a traditional C-style
            // for-loop semantics: for( counter_name = init_expr; test_expr; count_expr );
            string counter_name =
                this._ProcessText( element.GetAttributeValue( AttrName.CounterName ) ).GetTextValue();
            string init_expr =
                this._ProcessText( element.GetAttributeValue( AttrName.InitExpr ) ).GetTextValue();
            string test_expr = element.GetAttributeValue( AttrName.TestExpr );
            string count_expr = element.GetAttributeValue( AttrName.CountExpr );
            var generated_nodes = new List< XNode >();

            string current_expr = init_expr;
            int count = this._ExprAsInt( current_expr );
            bool run = true;
            while ( run )
            {
                /* Necessary due to "modified closure" lambda interaction rules */
                int count1 = count;
                XNode[] nodes = this._Env.Call( () =>
                                               {
                                                   // Define the counter value in the environment
                                                   this._Env.DefineTextSymbol( counter_name,
                                                                          count1.ToString(
                                                                              NumberFormatInfo.
                                                                                  InvariantInfo ) );
                                                   // Test for loop termination condition
                                                   if (
                                                       !this._Env.EvalBool(
                                                           this._ProcessText( test_expr ).GetTextValue() ) )
                                                   {
                                                       /* terminate */
                                                       run = false;
                                                       return new XNode[] {};
                                                   }

                                                   // Evaluate the count expression (must be done in the current environment stack frame)
                                                   count1 =
                                                       this._ExprAsInt(
                                                           this._ProcessText( count_expr ).GetTextValue() );

                                                   // Process the loop body
                                                   return this._ProcessNodes( element.Nodes() ).ToArray();
                                               } );
                /* Necessary due to "modified closure" lambda interaction rules */
                count = count1;
                generated_nodes.AddRange( nodes );
            }
            return generated_nodes;
        }

        private int _ExprAsInt(string expr)
        {
            int val;
            if ( !Int32.TryParse( this._Env.EvalExprAsString( expr ), out val ) )
            {
                throw new InvalidCastException(
                    String.Format( CultureInfo.CurrentCulture, "Expression '{0}' does not evaluate to an integer", expr ) );
            }
            return val;
        }
    }
}

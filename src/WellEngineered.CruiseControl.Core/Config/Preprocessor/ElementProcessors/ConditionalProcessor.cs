using System.Collections.Generic;
using System.Xml.Linq;

namespace WellEngineered.CruiseControl.Core.Config.Preprocessor.ElementProcessors
{
    internal abstract class ConditionalProcessor : ElementProcessor
    {
        protected ConditionalProcessor(XName targetElementName, PreprocessorEnvironment env)
            : base( targetElementName, env )
        {
        }

        /// <summary>
        ///  Common logic for if/ifdef/ifndef/else constructs
        /// </summary>
        /// <param name="conditionalElement"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        protected IEnumerable< XNode > _ProcessConditional(XElement conditionalElement,
                                                           bool condition)
        {
            return condition
                       ? this._ProcessNodes( conditionalElement.Nodes() )
                       : this._ProcessNextElse( conditionalElement );
        }

        private IEnumerable< XNode > _ProcessNextElse(XElement element)
        {
            XElement next_element = element.NextSiblingElement();
            if ( next_element != null && next_element.Name == this._Env._Settings.Namespace.GetName("else" ) )
            {
                return this._ProcessNodes( next_element.Nodes() );
            }
            return new XNode[] {};
        }
    }
}

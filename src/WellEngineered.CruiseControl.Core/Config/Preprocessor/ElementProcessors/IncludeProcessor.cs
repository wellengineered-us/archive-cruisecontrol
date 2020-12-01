using System.Collections.Generic;
using System.Xml.Linq;

namespace WellEngineered.CruiseControl.Core.Config.Preprocessor.ElementProcessors
{
    /// <summary>
    /// Processes the preprocessor "include" directive.
    /// </summary>
    internal class IncludeProcessor : ElementProcessor
    {
        public IncludeProcessor(PreprocessorEnvironment env)
            : base(env._Settings.Namespace.GetName("include"), env)
        {
        }

        public override IEnumerable< XNode > Process(XNode node)
        {
            XElement element = _AssumeElement( node );
            Validation.RequireAttributes( element, AttrName.Href );
            // Extract the href, expanding any symbol references
            string href =
                this._ProcessText( ( string ) element.Attribute( AttrName.Href ) ).GetTextValue().Trim();
            // Ask the preprocessor to load the included file
            XContainer doc = this._Env.PushInclude( href );
            try
            {
                // Process the included document content
                return this._ProcessNodes( doc.Nodes() );
            }
            finally
            {
                this._Env.PopInclude();
            }
        }
    }
}

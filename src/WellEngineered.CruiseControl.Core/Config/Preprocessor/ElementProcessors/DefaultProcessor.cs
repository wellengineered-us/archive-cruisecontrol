using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace WellEngineered.CruiseControl.Core.Config.Preprocessor.ElementProcessors
{
    internal class DefaultProcessor : ElementProcessor
    {
        private readonly ExpandSymbolProcessor _expand_symbol_processor;
        private readonly Dictionary< XName, IElementProcessor > _processors;
        private IEnumerable< XNode > _emptyNodeSet = new XNode[] {};

        public DefaultProcessor(PreprocessorEnvironment env) : base( env._Settings.Namespace.GetName("define"), env )
        {
            this._processors = new Dictionary< XName, IElementProcessor >();
            this._expand_symbol_processor = new ExpandSymbolProcessor( env );
            this._LoadElementProcessors();
        }

        public override IEnumerable< XNode > Process(XNode node)
        {
            try
            {
                //Console.WriteLine("Processing {0}", node.ErrorContext());
                switch ( node.NodeType )
                {
                    case XmlNodeType.Element:
                        return this._ProcessElement( ( XElement ) node );
                        /* PI,CDATA nodes are copied as-is */
                    case XmlNodeType.ProcessingInstruction:
                        return new[] {new XProcessingInstruction( ( XProcessingInstruction ) node )};
                    case XmlNodeType.CDATA:
                        return new[] {new XCData( ( XCData ) node )};
                    case XmlNodeType.Text:
                        return this._ProcessTextNode( ( XText ) node );
                    case XmlNodeType.Comment:
                        return new[] {new XComment( ( XComment ) node )};
                    case XmlNodeType.DocumentType:                        
                    case XmlNodeType.Entity:
                    case XmlNodeType.EntityReference:
                        return new[] { node };
                        
                    default:
                        throw new InvalidOperationException( "Unhandled Xml Node Type: " +
                                                             node.NodeType );
                }
            }
            catch ( PreprocessorException )
            {
                throw;
            }
        }


        private IEnumerable< XNode > _ProcessElement(XElement element)
        {
            return element.Name.Namespace == XmlNs.PreProcessor
                       ? this._ProcessPpElement( element )
                       : this._ProcessNonPpElement( element );
        }


        protected IEnumerable< XNode > _ProcessTextNode(XText node)
        {
            if (this._Env._Settings.IgnoreWhitespace && node.Value.Trim().Length == 0)
                return this._emptyNodeSet;
            return this._Env.EvalTextSymbols( node.Value );
        }

        /// <summary>
        /// Process an element which is not in the preprocessor's namespace.
        /// Copies the element, attributes, and recursively processes the content
        /// nodes.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Cloned element with processed content</returns>
        protected IEnumerable< XNode > _ProcessNonPpElement(XElement element)
        {
            var copy = new XElement( element.Name );
            var ignoreWhitespace = this._Env._Settings.IgnoreWhitespace;
            try
            {
                // Always preserve whitespace in attributes
                this._Env._Settings.IgnoreWhitespace = false;
                /* Clone attributes */
                foreach (XAttribute attr in element.Attributes())
                {                    
                    var processedtext = this._ProcessText(attr.Value);
                    var textvalue = processedtext.GetTextValue();

                    copy.Add(new XAttribute(attr.Name, textvalue));
                }
            }
            finally
            {
                // Restore ignore whitespace
                this._Env._Settings.IgnoreWhitespace = ignoreWhitespace;
            }
            /* Process content nodes */
            foreach ( XNode node in element.Nodes() )
            {
                IEnumerable< XNode > add_node = this.Process( node );
                if ( add_node != null )
                    copy.Add( add_node );
            }
            return new[] {copy};
        }

        protected IEnumerable< XNode > _ProcessPpElement(XElement element)
        {
            IElementProcessor processor;
            if ( this._processors.TryGetValue( element.Name, out processor ) )
            {
                return processor.Process( element );
            }

            return this._expand_symbol_processor.Process( element );
        }

        private void _LoadElementProcessors()
        {
            this._RegisterElementProcessor( new ConfigTemplateProcessor( this._Env ) );
            this._RegisterElementProcessor( new DefineProcessor( this._Env ) );
            this._RegisterElementProcessor( new EvalProcessor( this._Env ) );
            this._RegisterElementProcessor( new IncludeProcessor( this._Env ) );
            this._RegisterElementProcessor( new CountProcessor( this._Env ) );
            this._RegisterElementProcessor( new ScopeProcessor( this._Env ) );
            this._RegisterElementProcessor( new IfProcessor( this._Env ) );
            this._RegisterElementProcessor( new IfDefProcessor( this._Env ) );
            this._RegisterElementProcessor( new IfNDefProcessor( this._Env ) );
            this._RegisterElementProcessor( new ImportProcessor( this._Env ) );
            this._RegisterElementProcessor( new CreateElementProcessor( this._Env ) );
            this._RegisterElementProcessor( new IgnoreProcessor( this._Env._Settings.Namespace.GetName("else"), this._Env ) );
            this._RegisterElementProcessor( new ProcessingInstructionProcessor( this._Env ) );
            this._RegisterElementProcessor( new ForProcessor( this._Env ) );
            this._RegisterElementProcessor( new ForEachProcessor( this._Env ) );
        }

        internal void _RegisterElementProcessor(IElementProcessor processor)
        {
            this._processors[ processor.TargetElementName ] = processor;
        }
    }
}

using System.Xml;
using System.Xml.Schema;

namespace WellEngineered.CruiseControl.Core.Config
{
    /// <summary>
    /// 	
    /// </summary>
    public class XmlValidatingLoader
    {
        private readonly XmlReader innerReader;
        private XmlReaderSettings xmlReaderSettings;
        private bool valid;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlValidatingLoader" /> class.	
        /// </summary>
        /// <param name="innerReader">The inner reader.</param>
        /// <remarks></remarks>
        public XmlValidatingLoader(XmlReader innerReader)
        {
            this.innerReader = innerReader;
            // This is a bit of a hack - Turn on DTD entity resolution if it is not already on.
            var dummy = innerReader as XmlTextReader;
            if ( dummy != null )
            {
                dummy.EntityHandling = EntityHandling.ExpandEntities;
            }
            this.xmlReaderSettings = new XmlReaderSettings();
            this.xmlReaderSettings.ValidationType = ValidationType.None;
            this.xmlReaderSettings.ProhibitDtd = false;
            this.xmlReaderSettings.XmlResolver = new XmlUrlResolver();
            this.xmlReaderSettings.ConformanceLevel = ConformanceLevel.Auto;
            this.xmlReaderSettings.ValidationEventHandler += this.ValidationHandler;
        }

        /// <summary>
        /// Occurs when [validation event handler].	
        /// </summary>
        /// <remarks></remarks>
        public event ValidationEventHandler ValidationEventHandler
        {
            add { this.xmlReaderSettings.ValidationEventHandler += value; }
            remove { this.xmlReaderSettings.ValidationEventHandler -= value; }
        }

        /// <summary>
        /// Adds the schema.	
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <remarks></remarks>
        public void AddSchema(XmlSchema schema)
        {
            this.xmlReaderSettings.Schemas.Add(schema);
            this.xmlReaderSettings.ValidationType = ValidationType.Schema;
        }

        /// <summary>
        /// Loads this instance.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public XmlDocument Load()
        {
            // lock in case this object is used in a multi-threaded situation
            lock (this)
            {
                // set the flag true
                this.valid = true;                

                using (XmlReader reader = XmlReader.Create(this.innerReader, this.xmlReaderSettings))
                {
                    try
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.XmlResolver = new XmlUrlResolver();
                        doc.Load(reader);

                        // if the load failed, our event handler will have set flag to false
                        return this.valid ? doc : null;
                    }
                    finally
                    {
                        this.valid = true;
                    }
                }
            }
        }

        private void ValidationHandler(object sender, ValidationEventArgs args)
        {
            this.valid = false;
        }
    }
}
using System.Xml;

using WellEngineered.CruiseControl.Core.Config;

namespace WellEngineered.CruiseControl.Core
{
	/// <summary>
    /// Performs a read of a config file using NetReflector.
    /// </summary>
    public interface INetReflectorConfigurationReader
    {
        /// <summary>
        /// Reads an XML config document.
        /// </summary>
        /// <param name="document">The document to read.</param>
        /// <param name="errorProcesser">The error processer to use (can be null).</param>
        /// <returns>The loaded configuration.</returns>
        IConfiguration Read(XmlDocument document, IConfigurationErrorProcesser errorProcesser);

        /// <summary>
        /// Parses an element.
        /// </summary>
        /// <param name="node">The element to parse.</param>
        /// <returns>The parsed element.</returns>
        object ParseElement(XmlNode node);
    }
}

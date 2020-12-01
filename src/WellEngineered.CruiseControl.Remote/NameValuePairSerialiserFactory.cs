using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Serialisers;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Util;

namespace WellEngineered.CruiseControl.Remote
{
    /// <summary>
    /// Generate a serialiser for deserialising name/value pairs.
    /// </summary>
    public class NameValuePairSerialiserFactory
        : ISerialiserFactory
    {
        #region Public methods
        #region Create()
        /// <summary>
        /// Create the serialiser.
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public IXmlMemberSerialiser Create(ReflectorMember memberInfo, ReflectorPropertyAttribute attribute)
        {
            return new NameValuePairSerialiser(memberInfo, attribute, false);
        }
        #endregion
        #endregion
    }
}

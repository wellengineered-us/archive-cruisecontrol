using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Serialisers;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Util;

namespace WellEngineered.CruiseControl.Core.Util
{
    /// <summary>
    /// 	
    /// </summary>
	public class TimeoutSerializerFactory : ISerialiserFactory
	{
        /// <summary>
        /// Creates the specified member info.	
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        /// <param name="attribute">The attribute.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public IXmlMemberSerialiser Create(ReflectorMember memberInfo, ReflectorPropertyAttribute attribute)
		{
			return new TimeoutSerializer(memberInfo, attribute);
		}
	}
}
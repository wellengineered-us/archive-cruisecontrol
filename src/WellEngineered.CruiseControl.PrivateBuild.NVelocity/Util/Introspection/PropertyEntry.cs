using System.Reflection;

namespace WellEngineered.CruiseControl.PrivateBuild.NVelocity.Util.Introspection
{
	/// <summary>
    /// 
    /// </summary>
    public sealed class PropertyEntry
    {
        /// <summary>
        /// 
        /// </summary>
        public PropertyInfo Property { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public IPropertyAccessor Accessor { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        public PropertyEntry(PropertyInfo property)
        {
            this.Property = property;
            this.Accessor = new PropertyAccessor(property);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <param name="accessor"></param>
        public PropertyEntry(PropertyInfo property, IPropertyAccessor accessor)
        {
            this.Property = property;
            this.Accessor = accessor;
        }
    }
}

using System.Reflection;

namespace WellEngineered.CruiseControl.PrivateBuild.NVelocity.Util.Introspection
{
	/// <summary>
    /// 
    /// </summary>
    public sealed class MethodEntry
    {
        /// <summary>
        /// 
        /// </summary>
        public MethodInfo Method { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public IMethodInvoker Invoker { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        public MethodEntry(MethodInfo method)
        {
            this.Method = method;
            this.Invoker = new MethodInvoker(method);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="invoker"></param>
        public MethodEntry(MethodInfo method, IMethodInvoker invoker)
        {
            this.Method = method;
            this.Invoker = invoker;
        }
    }
}

/*
* Licensed to the Apache Software Foundation (ASF) under one
* or more contributor license agreements.  See the NOTICE file
* distributed with this work for additional information
* regarding copyright ownership.  The ASF licenses this file
* to you under the Apache License, Version 2.0 (the
* "License"); you may not use this file except in compliance
* with the License.  You may obtain a copy of the License at
*
*   http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing,
* software distributed under the License is distributed on an
* "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
* KIND, either express or implied.  See the License for the
* specific language governing permissions and limitations
* under the License.    
*/

using System;
using System.Reflection;

using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Log;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Parser.Node;

namespace WellEngineered.CruiseControl.PrivateBuild.NVelocity.Util.Introspection
{
	/// <summary>  Implementation of Uberspect to provide the default introspective
    /// functionality of Velocity
    /// 
    /// </summary>
    /// <author>  <a href="mailto:geirm@optonline.net">Geir Magnusson Jr.</a>
    /// </author>
    /// <author>  <a href="mailto:henning@apache.org">Henning P. Schmiedehausen</a>
    /// </author>
    /// <version>  $Id: UberspectImpl.java 723123 2008-12-03 23:14:34Z nbubna $
    /// </version>
    public class UberspectImpl : IUberspect, IUberspectLoggable
    {
        /// <summary>  Sets the runtime logger - this must be called before anything
        /// else.
        /// 
        /// </summary>
        /// <param name="Log">The logger instance to use.
        /// </param>
        /// <since> 1.5
        /// </since>
        public virtual Log Log
        {
            set
            {
                this.log = value;
            }

        }

        /// <summary>  Our runtime logger.</summary>
        protected internal Log log;

        /// <summary>  the default Velocity introspector</summary>
        protected internal Introspector introspector;

        /// <summary>  Init - generates the Introspector. As the setup code
        /// makes sure that the Log gets set before this is called,
        /// we can Initialize the Introspector using the Log object.
        /// </summary>
        public virtual void Init()
        {
            this.introspector = new Introspector(this.log);
        }

        /// <summary>  To support iterative objects used in a <code>#foreach()</code>
        /// loop.
        /// 
        /// </summary>
        /// <param name="obj">The iterative object.
        /// </param>
        /// <param name="i">Info about the object's location.
        /// </param>
        /// <returns> An {@link Iterator} object.
        /// </returns>
        /// <throws>  Exception </throws>
        public virtual System.Collections.IEnumerator GetIterator(object obj, Info i)
        {
            if (obj.GetType().IsArray)
            {
                return ((Array)obj).GetEnumerator();
            }
            else if (obj is System.Collections.ICollection)
            {
                return ((System.Collections.ICollection)obj).GetEnumerator();
            }
            else if (obj is System.Collections.IDictionary)
            {
                return ((System.Collections.IDictionary)obj).Values.GetEnumerator();
            }
            else if (obj is System.Collections.IEnumerator)
            {
                if (this.log.DebugEnabled)
                {
                    this.log.Debug("The iterative object in the #foreach() loop at " + i + " is of type java.util.Iterator.  Because " + "it is not resettable, if used in more than once it " + "may lead to unexpected results.");
                }
                return ((System.Collections.IEnumerator)obj);
            }
            else if (obj is System.Collections.IEnumerable)
            {
                return ((System.Collections.IEnumerable)obj).GetEnumerator();
            }


            /*  we have no clue what this is  */
            this.log.Debug("Could not determine type of iterator in #foreach loop at " + i);

            return null;
        }

        /// <summary>  Method</summary>
        /// <param name="obj">
        /// </param>
        /// <param name="methodName">
        /// </param>
        /// <param name="args">
        /// </param>
        /// <param name="i">
        /// </param>
        /// <returns> A Velocity Method.
        /// </returns>
        /// <throws>  Exception </throws>
        public virtual IVelMethod GetMethod(object obj, string methodName, object[] args, Info i)
        {
            if (obj == null)
            {
                return null;
            }

            MethodEntry m = this.introspector.GetMethod(obj.GetType(), methodName, args);
            if (m != null)
            {
                return new VelMethodImpl(m);
            }

            System.Type cls = obj.GetType();
            // if it's an array
            if (cls.IsArray)
            {
                // check for support via our array->list wrapper
                m = this.introspector.GetMethod(typeof(ArrayListWrapper), methodName, args);
                if (m != null)
                {
                    // and create a method that knows to wrap the value
                    // before invoking the method
                    return new VelMethodImpl(m, true);
                }
            }
            // watch for classes, to allow calling their static methods (VELOCITY-102)
            else if (cls == typeof(System.Type))
            {
                m = this.introspector.GetMethod((System.Type)obj, methodName, args);
                if (m != null)
                {
                    return new VelMethodImpl(m);
                }
            }
            return null;
        }

        /// <summary> Property  getter</summary>
        /// <param name="obj">
        /// </param>
        /// <param name="identifier">
        /// </param>
        /// <param name="i">
        /// </param>
        /// <returns> A Velocity Getter Method.
        /// </returns>
        /// <throws>  Exception </throws>
        public virtual IVelPropertyGet GetPropertyGet(object obj, string identifier, Info i)
        {
            if (obj == null)
            {
                return null;
            }

            System.Type claz = obj.GetType();

            /*
            *  first try for a getFoo() type of property
            *  (also getfoo() )
            */
            AbstractExecutor executor = new PropertyExecutor(this.log, this.introspector, claz, identifier);

            /*
            * Let's see if we are a map...
            */
            if (!executor.Alive)
            {
                executor = new MapGetExecutor(this.log, claz, identifier);
            }

            /*
            *  if that didn't work, look for Get("foo")
            */

            if (!executor.Alive)
            {
                executor = new GetExecutor(this.log, this.introspector, claz, identifier);
            }

            /*
            *  finally, look for boolean isFoo()
            */

            if (!executor.Alive)
            {
                executor = new BooleanPropertyExecutor(this.log, this.introspector, claz, identifier);
            }

            return (executor.Alive) ? new VelGetterImpl(executor) : null;
        }

        /// <summary> Property setter</summary>
        /// <param name="obj">
        /// </param>
        /// <param name="identifier">
        /// </param>
        /// <param name="arg">
        /// </param>
        /// <param name="i">
        /// </param>
        /// <returns> A Velocity Setter method.
        /// </returns>
        /// <throws>  Exception </throws>
        public virtual IVelPropertySet GetPropertySet(object obj, string identifier, object arg, Info i)
        {
            if (obj == null)
            {
                return null;
            }

            System.Type claz = obj.GetType();

            /*
            *  first try for a setFoo() type of property
            *  (also setfoo() )
            */
            SetExecutor executor = new SetPropertyExecutor(this.log, this.introspector, claz, identifier, arg);

            /*
            * Let's see if we are a map...
            */
            if (!executor.Alive)
            {
                executor = new MapSetExecutor(this.log, claz, identifier);
            }

            /*
            *  if that didn't work, look for Put("foo", arg)
            */

            if (!executor.Alive)
            {
                executor = new PutExecutor(this.log, this.introspector, claz, arg, identifier);
            }

            return (executor.Alive) ? new VelSetterImpl(executor) : null;
        }

        /// <summary>  Implementation of VelMethod</summary>
        public class VelMethodImpl : IVelMethod
        {
            private IMethodInvoker methodInvoker;

            /// <returns> true if this method can accept a variable number of arguments
            /// </returns>
            /// <since> 1.6
            /// </since>
            public virtual bool VarArg
            {
                get
                {
                    if (this.isVarArg)
                    {
                        ParameterInfo[] formal = this.method.GetParameters();

                        if (formal == null || formal.Length == 0)
                        {
                            this.isVarArg = false;
                        }
                        else
                        {
                            System.Type last = formal[formal.Length - 1].GetType();
                            // if the last arg is an array, then
                            // we consider this a varargs method
                            this.isVarArg = last.IsArray;
                        }
                    }
                    return this.isVarArg;
                }

            }
            /// <seealso cref="org.apache.velocity.util.introspection.VelMethod.isCacheable()">
            /// </seealso>
            virtual public bool Cacheable
            {
                get
                {
                    return true;
                }

            }
            /// <seealso cref="org.apache.velocity.util.introspection.VelMethod.getMethodName()">
            /// </seealso>
            virtual public string MethodName
            {
                get
                {
                    return this.method.Name;
                }

            }
            /// <seealso cref="org.apache.velocity.util.introspection.VelMethod.getReturnType()">
            /// </seealso>
            virtual public System.Type ReturnType
            {
                get
                {
                    return this.method.ReturnType;
                }

            }

            internal MethodInfo method;
            internal bool isVarArg;
            internal bool wrapArray;

            /// <param name="m">
            /// </param>
            public VelMethodImpl(MethodEntry m)
                : this(m, false)
            {
            }

            /// <since> 1.6
            /// </since>
            public VelMethodImpl(MethodEntry method, bool wrapArray)
            {
                this.method = method.Method;
                this.wrapArray = wrapArray;
                this.methodInvoker = method.Invoker;
            }

            internal VelMethodImpl()
            {
                this.method = null;
            }

            /// <seealso cref="VelMethod.invoke(object, object[])">
            /// </seealso>
            public virtual object Invoke(object instance, object[] parameters)
            {
                // if we're pretending an array is a list...
                if (this.wrapArray)
                {
                    instance = new ArrayListWrapper(instance);
                }

                if (this.VarArg)
                {
                    ParameterInfo[] formal = this.method.GetParameters();
                    int index = formal.Length - 1;
                    if (parameters.Length >= index)
                    {
                        System.Type type = formal[index].GetType().GetElementType();
                        parameters = handleVarArg(type, index, parameters);
                    }
                }

                // call extension point invocation
                return this.DoInvoke(instance, parameters);
            }

            /// <summary> Offers an extension point for subclasses (in alternate Uberspects)
            /// to alter the invocation after any array wrapping or varargs handling
            /// has already been completed.
            /// </summary>
            /// <since> 1.6
            /// </since>
            protected internal virtual object DoInvoke(object instance, object[] parameter)
            {
                return this.methodInvoker.Invoke(instance, (object[])parameter);
            }


            /// <param name="type">The vararg class type (aka component type
            /// of the expected array arg)
            /// </param>
            /// <param name="index">The index of the vararg in the method declaration
            /// (This will always be one less than the number of
            /// expected arguments.)
            /// </param>
            /// <param name="parameters">The parameters parameters being passed to this method
            /// </param>
            /// <returns>s The parameters parameters adjusted for the varargs in order
            /// to fit the method declaration.
            /// </returns>
            private static object[] handleVarArg(System.Type type, int index, object[] actual)
            {
                // if no values are being passed into the vararg
                if (actual.Length == index)
                {
                    // copy existing args to new array
                    object[] newActual = new object[actual.Length + 1];
                    Array.Copy(actual, 0, newActual, 0, actual.Length);
                    // create an empty array of the expected type
                    newActual[index] = System.Array.CreateInstance(type, 0);
                    actual = newActual;
                }
                // if one value is being passed into the vararg
                else if (actual.Length == index + 1)
                {
                    // make sure the last arg is an array of the expected type
                    if (IntrospectionUtils.IsMethodInvocationConvertible(type, actual[index].GetType(), false))
                    {
                        // create a 1-length array to hold and replace the last param
                        object lastActual = System.Array.CreateInstance(type, 1);
                        ((System.Array)lastActual).SetValue(actual[index], 0);
                        actual[index] = lastActual;
                    }
                }
                // if multiple values are being passed into the vararg
                else if (actual.Length > index + 1)
                {
                    // Put the last and extra parameters in an array of the expected type
                    int size = actual.Length - index;
                    object lastActual = System.Array.CreateInstance(type, size);
                    for (int i = 0; i < size; i++)
                    {
                        ((System.Array)lastActual).SetValue(actual[index + i], i);
                    }

                    // Put all into a new parameters array of the appropriate size
                    object[] newActual = new object[index + 1];
                    for (int i = 0; i < index; i++)
                    {
                        newActual[i] = actual[i];
                    }
                    newActual[index] = lastActual;

                    // replace the old parameters array
                    actual = newActual;
                }
                return actual;
            }
        }

        /// <summary> 
        /// 
        /// </summary>
        public class VelGetterImpl : IVelPropertyGet
        {
            /// <seealso cref="org.apache.velocity.util.introspection.VelPropertyGet.isCacheable()">
            /// </seealso>
            virtual public bool Cacheable
            {
                get
                {
                    return true;
                }

            }
            /// <seealso cref="org.apache.velocity.util.introspection.VelPropertyGet.getMethodName()">
            /// </seealso>
            virtual public string MethodName
            {
                get
                {
                    return this.getExecutor.Alive ? this.getExecutor.Property.Property.Name : null;
                }

            }

            internal AbstractExecutor getExecutor;

            /// <param name="exec">
            /// </param>
            public VelGetterImpl(AbstractExecutor exec)
            {
                this.getExecutor = exec;
            }

            internal VelGetterImpl()
            {
                this.getExecutor = null;
            }

            /// <seealso cref="org.apache.velocity.util.introspection.VelPropertyGet.invoke(object)">
            /// </seealso>
            public virtual object Invoke(object o)
            {
                return this.getExecutor.Execute(o);
            }
        }

        /// <summary> </summary>
        public class VelSetterImpl : IVelPropertySet
        {
            /// <seealso cref="org.apache.velocity.util.introspection.VelPropertySet.isCacheable()">
            /// </seealso>
            virtual public bool Cacheable
            {
                get
                {
                    return true;
                }

            }
            /// <seealso cref="org.apache.velocity.util.introspection.VelPropertySet.getMethodName()">
            /// </seealso>
            virtual public string MethodName
            {
                get
                {
                    return this.setExecutor.Alive ? this.setExecutor.Method.Method.Name : null;
                }

            }

            private SetExecutor setExecutor;

            /// <param name="setExecutor">
            /// </param>
            public VelSetterImpl(SetExecutor setExecutor)
            {
                this.setExecutor = setExecutor;
            }

            internal VelSetterImpl()
            {
                
            }

            /// <summary> Invoke the found Set Executor.
            /// 
            /// </summary>
            /// <param name="instance">is the Object to invoke it on.
            /// </param>
            /// <param name="value">in the Value to set.
            /// </param>
            /// <returns> The resulting Object.
            /// </returns>
            /// <throws>  Exception </throws>
            public virtual object Invoke(object o, object arg)
            {
                return this.setExecutor.Execute(o, arg);
            }
        }
    }
}
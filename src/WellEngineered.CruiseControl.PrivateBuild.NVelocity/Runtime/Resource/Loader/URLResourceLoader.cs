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

using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Commons.Collections;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Exception;

namespace WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Resource.Loader
{
	/// <summary> This is a simple URL-based loader.
    /// 
    /// </summary>
    /// <author>  <a href="mailto:geirm@apache.org">Geir Magnusson Jr.</a>
    /// </author>
    /// <author>  <a href="mailto:nbubna@apache.org">Nathan Bubna</a>
    /// </author>
    /// <version>  $Id: URLResourceLoader.java 191743 2005-06-21 23:22:20Z dlr $
    /// </version>
    /// <since> 1.5
    /// </since>
    public class URLResourceLoader : ResourceLoader
    {
        /// <summary> Returns the current, custom timeout setting. If negative, there is no custom timeout.</summary>
        /// <since> 1.6
        /// </since>
        virtual public int Timeout
        {
            get
            {
                return this.timeout;
            }

        }
        private string[] roots = null;
 
        protected internal System.Collections.Hashtable templateRoots = null;
        private int timeout = -1;
        private System.Reflection.MethodInfo[] timeoutMethods;

        /// <seealso cref="ResourceLoader.Init(org.apache.commons.collections.ExtendedProperties)">
        /// </seealso>
        public override void Init(ExtendedProperties configuration)
        {
            this.log.Trace("URLResourceLoader : initialization starting.");

            this.roots = configuration.GetStringArray("root");
            if (this.log.DebugEnabled)
            {
                for (int i = 0; i < this.roots.Length; i++)
                {
                    this.log.Debug("URLResourceLoader : adding root '" + this.roots[i] + "'");
                }
            }

            this.timeout = configuration.GetInt("timeout", -1);
            if (this.timeout > 0)
            {
                try
                {
                    System.Type[] types = new System.Type[] { System.Type.GetType("System.Int32") };
                    System.Reflection.MethodInfo conn = typeof(System.Net.HttpWebRequest).GetMethod("setConnectTimeout", (types == null) ? new System.Type[0] : (System.Type[])types);
                    System.Reflection.MethodInfo read = typeof(System.Net.HttpWebRequest).GetMethod("setReadTimeout", (types == null) ? new System.Type[0] : (System.Type[])types);
                    this.timeoutMethods = new System.Reflection.MethodInfo[] { conn, read };
                    this.log.Debug("URLResourceLoader : timeout set to " + this.timeout);
                }
                catch (System.MethodAccessException nsme)
                {
                    this.log.Debug("URLResourceLoader : Java 1.5+ is required to customize timeout!", nsme);
                    this.timeout = -1;
                }
            }

            // Init the template paths map
            this.templateRoots = new System.Collections.Hashtable();

            this.log.Trace("URLResourceLoader : initialization complete.");
        }

        /// <summary> Get an InputStream so that the Runtime can build a
        /// template with it.
        /// 
        /// </summary>
        /// <param name="name">name of template to fetch bytestream of
        /// </param>
        /// <returns> InputStream containing the template
        /// </returns>
        /// <throws>  ResourceNotFoundException if template not found </throws>
        /// <summary>         in the file template path.
        /// </summary>
        public override System.IO.Stream GetResourceStream(string name)
        {
            lock (this)
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new ResourceNotFoundException("URLResourceLoader : No template name provided");
                }

                System.IO.Stream inputStream = null;
                System.Exception exception = null;
                for (int i = 0; i < this.roots.Length; i++)
                {
                    try
                    {
                        System.Uri u = new System.Uri(this.roots[i] + name);
                        System.Net.HttpWebRequest conn = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(u);
                        this.tryToSetTimeout(conn);
                        inputStream = conn.GetResponse().GetResponseStream();

                        if (inputStream != null)
                        {
                            if (this.log.DebugEnabled)
                                this.log.Debug("URLResourceLoader: Found '" + name + "' at '" + this.roots[i] + "'");

                            // save this root for later re-use
                            this.templateRoots[name] = this.roots[i];
                            break;
                        }
                    }
                    catch (System.IO.IOException ioe)
                    {
                        if (this.log.DebugEnabled)
                            this.log.Debug("URLResourceLoader: Exception when looking for '" + name + "' at '" + this.roots[i] + "'", ioe);

                        // only save the first one for later throwing
                        if (exception == null)
                        {
                            exception = ioe;
                        }
                    }
                }

                // if we never found the template
                if (inputStream == null)
                {
                    string msg;
                    if (exception == null)
                    {
                        msg = "URLResourceLoader : Resource '" + name + "' not found.";
                    }
                    else
                    {
                        msg = exception.Message;
                    }
                    // convert to a general Velocity ResourceNotFoundException
                    throw new ResourceNotFoundException(msg);
                }

                return inputStream;
            }
        }

        /// <summary> Checks to see if a resource has been deleted, moved or modified.
        /// 
        /// </summary>
        /// <param name="resource">Resource  The resource to check for modification
        /// </param>
        /// <returns> boolean  True if the resource has been modified, moved, or unreachable
        /// </returns>
        public override bool IsSourceModified(Resource resource)
        {
            long fileLastModified = this.GetLastModified(resource);
            // if the file is unreachable or otherwise changed
            if (fileLastModified == 0 || fileLastModified != resource.LastModified)
            {
                return true;
            }
            return false;
        }

        /// <summary> Checks to see when a resource was last modified
        /// 
        /// </summary>
        /// <param name="resource">Resource the resource to check
        /// </param>
        /// <returns> long The time when the resource was last modified or 0 if the file can't be reached
        /// </returns>
        public override long GetLastModified(Resource resource)
        {
            // Get the previously used root
            string name = resource.Name;
            string root = (string)this.templateRoots[name];

            try
            {
                // Get a connection to the URL
                System.Uri u = new System.Uri(root + name);
                System.Net.HttpWebRequest conn = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(u);
                this.tryToSetTimeout(conn);

                return SupportClass.URLConnectionSupport.GetLastModifiedHeaderField(conn);
            }
            catch (System.IO.IOException ioe)
            {
                // the file is not reachable at its previous address
                string msg = "URLResourceLoader: '" + name + "' is no longer reachable at '" + root + "'";
                this.log.Error(msg, ioe);
                throw new ResourceNotFoundException(msg, ioe);
            }
        }

        private void tryToSetTimeout(System.Net.HttpWebRequest conn)
        {
            if (this.timeout > 0)
            {
                object[] arg = new object[] { (System.Int32)this.timeout };
                try
                {
                    this.timeoutMethods[0].Invoke(conn, (object[])arg);
                    this.timeoutMethods[1].Invoke(conn, (object[])arg);
                }
                catch (System.Exception e)
                {
                    string msg = "Unexpected exception while setting connection timeout for " + conn;
                    this.log.Error(msg, e);
                    throw new VelocityException(msg, e);
                }
            }
        }
    }
}

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
using System.Collections;
using System.Collections.Generic;
using System.IO;

using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Exception;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Directive;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Log;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Parser;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Parser.Node;

namespace WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime
{
	/// <summary>  VelocimacroFactory.java
    /// 
    /// manages the set of VMs in a running Velocity engine.
    /// 
    /// </summary>
    /// <author>  <a href="mailto:geirm@optonline.net">Geir Magnusson Jr.</a>
    /// </author>
    /// <version>  $Id: VelocimacroFactory.java 718442 2008-11-18 00:01:17Z nbubna $
    /// </version>
    public class VelocimacroFactory
    {
        /// <summary> sets permission to have VMs local in scope to their declaring template note that this is
        /// really taken care of in the VMManager class, but we need it here for gating purposes in addVM
        /// eventually, I will slide this all into the manager, maybe.
        /// </summary>
        private bool TemplateLocalInline
        {
            get
            {
                return this.templateLocal;
            }

            set
            {
                this.templateLocal = value;
            }

        }

        /// <summary>  Get the switch for automatic reloading of
        /// global library-based VMs
        /// </summary>
        /// <summary>  set the switch for automatic reloading of
        /// global library-based VMs
        /// </summary>
        private bool Autoload
        {
            get
            {
                return this.autoReloadLibrary;
            }

            set
            {
                this.autoReloadLibrary = value;
            }

        }
        /// <summary>  runtime services for this instance</summary>
        private IRuntimeServices rsvc;

        /// <summary>  the Log for this instance</summary>
        private LogDisplayWrapper log;

        /// <summary>  VMManager : deal with namespace management
        /// and actually keeps all the VM definitions
        /// </summary>
        private VelocimacroManager vmManager = null;

        /// <summary>  determines if replacement of global VMs are allowed
        /// controlled by  VM_PERM_ALLOW_INLINE_REPLACE_GLOBAL
        /// </summary>
        private bool replaceAllowed = false;

        /// <summary>  controls if new VMs can be added.  Set by
        /// VM_PERM_ALLOW_INLINE  Note the assumption that only
        /// through inline defs can this happen.
        /// additions through autoloaded VMs is allowed
        /// </summary>
        private bool addNewAllowed = true;

        /// <summary>  sets if template-local namespace in used</summary>
        private bool templateLocal = false;

        /// <summary>  determines if the libraries are auto-loaded
        /// when they change
        /// </summary>
        private bool autoReloadLibrary = false;

        /// <summary>  vector of the library names</summary>
        private System.Collections.IList macroLibVec = null;

        /// <summary>  map of the library Template objects
        /// used for reload determination
        /// </summary>
        private IDictionary<string, Twonk> libModMap;

        /// <summary>  C'tor for the VelociMacro factory.
        /// 
        /// </summary>
        /// <param name="rsvc">Reference to a runtime services object.
        /// </param>
        public VelocimacroFactory(IRuntimeServices rsvc)
        {
            this.rsvc = rsvc;
            this.log = new LogDisplayWrapper(rsvc.Log, "Velocimacro : ", rsvc.GetBoolean(RuntimeConstants.VM_MESSAGES_ON, true));

            /*
            *  we always access in a synchronized(), so we
            *  can use an unsynchronized hashmap
            */
            this.libModMap = new Dictionary<string, Twonk>();
            this.vmManager = new VelocimacroManager(rsvc);
        }

        /// <summary>  Initialize the factory - setup all permissions
        /// load all global libraries.
        /// </summary>
        public void InitVelocimacro()
        {
            /*
            *  maybe I'm just paranoid...
            */
            lock (this)
            {
                this.log.Trace("initialization starting.");

                /*
                *   allow replacements while we Add the libraries, if exist
                */
                this.SetReplacementPermission(true);

                /*
                *  Add all library macros to the global namespace
                */

                this.vmManager.NamespaceUsage = false;

                /*
                *  now, if there is a global or local libraries specified, use them.
                *  All we have to do is Get the template. The template will be parsed;
                *  VM's  are added during the parse phase
                */

                object libfiles = this.rsvc.GetProperty(RuntimeConstants.VM_LIBRARY);

                if (libfiles == null)
                {
                    this.log.Debug("\"" + RuntimeConstants.VM_LIBRARY + "\" is not set.  Trying default library: " + NVelocity.Runtime.RuntimeConstants.VM_LIBRARY_DEFAULT);

                    // try the default library.
                    if (this.rsvc.GetLoaderNameForResource(RuntimeConstants.VM_LIBRARY_DEFAULT) != null)
                    {
                        libfiles = RuntimeConstants.VM_LIBRARY_DEFAULT;
                    }
                    else
                    {
                        this.log.Debug("Default library not found.");
                    }
                }

                if (libfiles != null)
                {
                    this.macroLibVec = new ArrayList();
                    if (libfiles is ArrayList)
                    {
                        var lbs = (ArrayList)libfiles;

                        foreach (var lb in lbs)
                        {
                            this.macroLibVec.Add(lb);
                        }
                    }
                    else if (libfiles is string)
                    {
                        this.macroLibVec.Add(libfiles);
                    }

                    for (int i = 0, is_Renamed = this.macroLibVec.Count; i < is_Renamed; i++)
                    {
                        string lib = (string)this.macroLibVec[i];

                        /*
                        * only if it's a non-empty string do we bother
                        */

                        if (!String.IsNullOrEmpty(lib))
                        {
                            /*
                            *  let the VMManager know that the following is coming
                            *  from libraries - need to know for auto-load
                            */

                            this.vmManager.RegisterFromLib = true;

                            this.log.Debug("adding VMs from VM library : " + lib);

                            try
                            {
                                Template template = this.rsvc.GetTemplate(lib);

                                /*
                                *  save the template.  This depends on the assumption
                                *  that the Template object won't change - currently
                                *  this is how the Resource manager works
                                */

                                Twonk twonk = new Twonk();
                                twonk.template = template;
                                twonk.modificationTime = template.LastModified;
                                this.libModMap[lib] = twonk;
                            }
                            catch (System.Exception e)
                            {
                                string msg = "Velocimacro : Error using VM library : " + lib;
                                this.log.Error(true, msg, e);
                            }

                            this.log.Trace("VM library registration complete.");

                            this.vmManager.RegisterFromLib = false;
                        }
                    }
                }

                /*
                *   now, the permissions
                */


                /*
                *  allowinline : anything after this will be an inline macro, I think
                *  there is the question if a #include is an inline, and I think so
                *
                *  default = true
                */
                this.SetAddMacroPermission(true);

                if (!this.rsvc.GetBoolean(RuntimeConstants.VM_PERM_ALLOW_INLINE, true))
                {
                    this.SetAddMacroPermission(false);

                    this.log.Debug("allowInline = false : VMs can NOT be defined inline in templates");
                }
                else
                {
                    this.log.Debug("allowInline = true : VMs can be defined inline in templates");
                }

                /*
                *  allowInlineToReplaceGlobal : allows an inline VM , if allowed at all,
                *  to replace an existing global VM
                *
                *  default = false
                */
                this.SetReplacementPermission(false);

                if (this.rsvc.GetBoolean(RuntimeConstants.VM_PERM_ALLOW_INLINE_REPLACE_GLOBAL, false))
                {
                    this.SetReplacementPermission(true);

                    this.log.Debug("allowInlineToOverride = true : VMs " + "defined inline may replace previous VM definitions");
                }
                else
                {
                    this.log.Debug("allowInlineToOverride = false : VMs " + "defined inline may NOT replace previous VM definitions");
                }

                /*
                * now turn on namespace handling as far as permissions allow in the
                * manager, and also set it here for gating purposes
                */
                this.vmManager.NamespaceUsage = true;

                /*
                *  template-local inline VM mode : default is off
                */
                this.TemplateLocalInline = this.rsvc.GetBoolean(Runtime.RuntimeConstants.VM_PERM_INLINE_LOCAL, false);

                if (this.TemplateLocalInline)
                {
                    this.log.Debug("allowInlineLocal = true : VMs " + "defined inline will be local to their defining template only.");
                }
                else
                {
                    this.log.Debug("allowInlineLocal = false : VMs " + "defined inline will be global in scope if allowed.");
                }

                this.vmManager.TemplateLocalInlineVM = this.TemplateLocalInline;

                /*
                *  autoload VM libraries
                */
                this.Autoload = this.rsvc.GetBoolean(RuntimeConstants.VM_LIBRARY_AUTORELOAD, false);

                if (this.Autoload)
                {
                    this.log.Debug("autoload on : VM system " + "will automatically reload global library macros");
                }
                else
                {
                    this.log.Debug("autoload off : VM system " + "will not automatically reload global library macros");
                }

                this.log.Trace("Velocimacro : initialization complete.");
            }
        }

        /// <summary> Adds a macro to the factory.
        /// 
        /// addVelocimacro(String, Node, String[] argArray, String) should be used internally
        /// instead of this.
        /// 
        /// </summary>
        /// <param name="name">Name of the Macro to Add.
        /// </param>
        /// <param name="macroBody">String representation of the macro.
        /// </param>
        /// <param name="argArray">Macro arguments. First element is the macro name.
        /// </param>
        /// <param name="sourceTemplate">Source template from which the macro gets registered.
        /// 
        /// </param>
        /// <returns> true if Macro was registered successfully.
        /// </returns>
        public bool AddVelocimacro(string name, string macroBody, string[] argArray, string sourceTemplate)
        {
            /*
            * maybe we should throw an exception, maybe just tell
            * the caller like this...
            *
            * I hate this : maybe exceptions are in order here...
            * They definitely would be if this was only called by directly
            * by users, but Velocity calls this internally.
            */
            if (name == null || macroBody == null || argArray == null || sourceTemplate == null)
            {
                string msg = "VM '" + name + "' addition rejected : ";
                if (name == null)
                {
                    msg += "name";
                }
                else if (macroBody == null)
                {
                    msg += "macroBody";
                }
                else if (argArray == null)
                {
                    msg += "argArray";
                }
                else
                {
                    msg += "sourceTemplate";
                }
                msg += " argument was null";
                this.log.Error(msg);
                throw new System.NullReferenceException(msg);
            }

            /*
            *  see if the current ruleset allows this addition
            */

            if (!this.CanAddVelocimacro(name, sourceTemplate))
            {
                return false;
            }

            lock (this)
            {
                try
                {
                    INode macroRootNode = this.rsvc.Parse(new StringReader(macroBody), sourceTemplate);

                    this.vmManager.AddVM(name, macroRootNode, argArray, sourceTemplate, this.replaceAllowed);
                }
                catch (ParseException ex)
                {
                    // to keep things 1.3 compatible call toString() here
                    throw new System.SystemException(ex.ToString());
                }
            }

            if (this.log.DebugEnabled)
            {
                System.Text.StringBuilder msg = new System.Text.StringBuilder("added ");
                Macro.MacroToString(msg, argArray);
                msg.Append(" : source = ").Append(sourceTemplate);
                this.log.Debug(msg.ToString());
            }

            return true;
        }

        /// <summary> Adds a macro to the factory.
        /// 
        /// </summary>
        /// <param name="name">Name of the Macro to Add.
        /// </param>
        /// <param name="macroBody">root node of the parsed macro AST
        /// </param>
        /// <param name="argArray">Name of the macro arguments. First element is the macro name.
        /// </param>
        /// <param name="sourceTemplate">Source template from which the macro gets registered.
        /// </param>
        /// <returns> true if Macro was registered successfully.
        /// </returns>
        /// <since> 1.6
        /// </since>
        public bool AddVelocimacro(string name, INode macroBody, string[] argArray, string sourceTemplate)
        {
            // Called by RuntimeInstance.addVelocimacro

            /*
            * maybe we should throw an exception, maybe just tell
            * the caller like this...
            *
            * I hate this : maybe exceptions are in order here...
            * They definitely would be if this was only called by directly
            * by users, but Velocity calls this internally.
            */
            if (name == null || macroBody == null || argArray == null || sourceTemplate == null)
            {
                string msg = "VM '" + name + "' addition rejected : ";
                if (name == null)
                {
                    msg += "name";
                }
                else if (macroBody == null)
                {
                    msg += "macroBody";
                }
                else if (argArray == null)
                {
                    msg += "argArray";
                }
                else
                {
                    msg += "sourceTemplate";
                }
                msg += " argument was null";
                this.log.Error(msg);
                throw new System.NullReferenceException(msg);
            }

            /*
            *  see if the current ruleset allows this addition
            */

            if (!this.CanAddVelocimacro(name, sourceTemplate))
            {
                return false;
            }

            lock (this)
            {
                this.vmManager.AddVM(name, macroBody, argArray, sourceTemplate, this.replaceAllowed);
            }
            return (true);
        }


        /// <summary>  determines if a given macro/namespace (name, source) combo is allowed
        /// to be added
        /// 
        /// </summary>
        /// <param name="name">Name of VM to Add
        /// </param>
        /// <param name="sourceTemplate">Source template that contains the defintion of the VM
        /// </param>
        /// <returns> true if it is allowed to be added, false otherwise
        /// </returns>
        private bool CanAddVelocimacro(string name, string sourceTemplate)
        {
            lock (this)
            {
                /*
                *  short circuit and do it if autoloader is on, and the
                *  template is one of the library templates
                */

                if (this.autoReloadLibrary && (this.macroLibVec != null))
                {
                    if (this.macroLibVec.Contains(sourceTemplate))
                        return true;
                }


                /*
                * maybe the rules should be in manager?  I dunno. It's to manage
                * the namespace issues first, are we allowed to Add VMs at all?
                * This trumps all.
                */
                if (!this.addNewAllowed)
                {
                    this.log.Warn("VM addition rejected : " + name + " : inline VMs not allowed.");
                    return false;
                }

                /*
                *  are they local in scope?  Then it is ok to Add.
                */
                if (!this.templateLocal)
                {
                    /*
                    * otherwise, if we have it already in global namespace, and they can't replace
                    * since local templates are not allowed, the global namespace is implied.
                    *  remember, we don't know anything about namespace managment here, so lets
                    *  note do anything fancy like trying to give it the global namespace here
                    *
                    *  so if we have it, and we aren't allowed to replace, bail
                    */
                    if (!this.replaceAllowed && this.IsVelocimacro(name, sourceTemplate))
                    {
                        /*
                        * Concurrency fix: the Log entry was changed to Debug scope because it
                        * causes false alarms when several concurrent threads simultaneously (re)parse
                        * some macro
                        */
                        if (this.log.DebugEnabled)
                            this.log.Debug("VM addition rejected : " + name + " : inline not allowed to replace existing VM");
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary> Tells the world if a given directive string is a Velocimacro</summary>
        /// <param name="vm">Name of the Macro.
        /// </param>
        /// <param name="sourceTemplate">Source template from which the macro should be loaded.
        /// </param>
        /// <returns> True if the given name is a macro.
        /// </returns>
        public virtual bool IsVelocimacro(string vm, string sourceTemplate)
        {
            // synchronization removed
            return (this.vmManager.Get(vm, sourceTemplate) != null);
        }

        /// <summary>  parameters factory : creates a Directive that will
        /// behave correctly wrt getting the framework to
        /// dig out the correct # of args
        /// </summary>
        /// <param name="vmName">Name of the Macro.
        /// </param>
        /// <param name="sourceTemplate">Source template from which the macro should be loaded.
        /// </param>
        /// <returns> A directive representing the Macro.
        /// </returns>
        public virtual Directive.Directive GetVelocimacro(string vmName, string sourceTemplate)
        {
            return (this.GetVelocimacro(vmName, sourceTemplate, null));
        }

        /// <since> 1.6
        /// </since>
        public virtual Directive.Directive GetVelocimacro(string vmName, string sourceTemplate, string renderingTemplate)
        {
            VelocimacroProxy vp = null;

            vp = this.vmManager.Get(vmName, sourceTemplate, renderingTemplate);

            /*
            * if this exists, and autoload is on, we need to check where this VM came from
            */

            if (vp != null && this.autoReloadLibrary)
            {
                lock (this)
                {
                    /*
                    * see if this VM came from a library. Need to pass sourceTemplate in the event
                    * namespaces are set, as it could be masked by local
                    */

                    string lib = this.vmManager.GetLibraryName(vmName, sourceTemplate);

                    if (lib != null)
                    {
                        try
                        {
                            /*
                            * Get the template from our map
                            */

                          
                            if (this.libModMap.ContainsKey(lib))
                            {
                                Twonk tw = this.libModMap[lib];

                                Template template = tw.template;

                                /*
                                * now, Compare the last modified time of the resource with the last
                                * modified time of the template if the file has changed, then reload.
                                * Otherwise, we should be ok.
                                */

                                long tt = tw.modificationTime;
                                long ft = template.ResourceLoader.GetLastModified(template);

                                if (ft > tt)
                                {
                                    this.log.Debug("auto-reloading VMs from VM library : " + lib);

                                    /*
                                    * when there are VMs in a library that invoke each other, there are
                                    * calls into getVelocimacro() from the Init() process of the VM
                                    * directive. To stop the infinite loop we save the current time
                                    * reported by the resource loader and then be honest when the
                                    * reload is complete
                                    */

                                    tw.modificationTime = ft;

                                    template = this.rsvc.GetTemplate(lib);

                                    /*
                                    * and now we be honest
                                    */

                                    tw.template = template;
                                    tw.modificationTime = template.LastModified;

                                    /*
                                    * note that we don't need to Put this twonk
                                    * back into the map, as we can just use the
                                    * same reference and this block is synchronized
                                    */
                                }
                            }
                        }
                        catch (System.Exception e)
                        {
                            string msg = "Velocimacro : Error using VM library : " + lib;
                            //Log.Error(true, msg, e);
                            throw new VelocityException(msg, e);
                        }

                        vp = this.vmManager.Get(vmName, sourceTemplate, renderingTemplate);
                    }
                }
            }

            return vp;
        }

        /// <summary> tells the vmManager to dump the specified namespace
        /// 
        /// </summary>
        /// <param name="namespace">Namespace to dump.
        /// </param>
        /// <returns> True if namespace has been dumped successfully.
        /// </returns>
        public virtual bool DumpVMNamespace(string namespace_Renamed)
        {
            return this.vmManager.DumpNamespace(namespace_Renamed);
        }

        /// <summary> sets the permission to Add new macros</summary>
        private bool SetAddMacroPermission(bool addNewAllowed)
        {
            bool b = this.addNewAllowed;
            this.addNewAllowed = addNewAllowed;
            return b;
        }

        /// <summary> sets the permission for allowing addMacro() calls to replace existing VM's</summary>
        private bool SetReplacementPermission(bool arg)
        {
            bool b = this.replaceAllowed;
            this.replaceAllowed = arg;
            this.vmManager.InlineReplacesGlobal = arg;
            return b;
        }

        /// <summary> small container class to hold the tuple
        /// of a template and modification time.
        /// We keep the modification time so we can
        /// 'override' it on a reload to prevent
        /// recursive reload due to inter-calling
        /// VMs in a library
        /// </summary>
        private class Twonk
        {
            /// <summary>Template kept in this container. </summary>
            public Template template;

            /// <summary>modification time of the template. </summary>
            public long modificationTime;
        }
    }
}
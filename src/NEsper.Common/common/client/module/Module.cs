///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.client.configuration.common;

namespace com.espertech.esper.common.client.module
{
    /// <summary>
    /// Represent a deployment unit consisting of deployment declarative information
    /// (module name, uses and imports) as well as EPL statements represented by
    /// <see cref="ModuleItem" />. May have an additional user object and archive name 
    /// and uri pointing to the module source attached.
    /// <para/>
    /// The module URI gets initialized with the filename, resource or URL being read,
    /// however may be overridden and has not further meaning to the deployment.
    /// <para/>
    /// The archive name and user object are opportunities to attach additional deployment
    /// information.
    /// </summary>
    public class Module
    {
        /// <summary>Ctor. </summary>
        /// <param name="name">module name</param>
        /// <param name="uri">module uri</param>
        /// <param name="uses">names of modules that this module depends on</param>
        /// <param name="imports">the type imports</param>
        /// <param name="items">EPL statements</param>
        /// <param name="moduleText">text of module</param>
        public Module(
            string name,
            string uri,
            ICollection<string> uses,
            ICollection<Import> imports,
            IList<ModuleItem> items,
            string moduleText)
        {
            Name = name;
            Uri = uri;
            Uses = uses;
            Imports = new HashSet<Import>(imports);
            Items = new List<ModuleItem>(items);
            ModuleText = moduleText;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Module"/> class.
        /// </summary>
        public Module()
        {
            Items = new List<ModuleItem>();
        }

        /// <summary>Returns the name of the archive this module originated from, or null if not applicable. </summary>
        /// <value>archive name</value>
        public string ArchiveName { get; set; }

        /// <summary>Returns the optional user object that may be attached to the module. </summary>
        /// <value>user object</value>
        public object UserObjectCompileTime { get; set; }

        /// <summary>Returns the module name, if provided. </summary>
        /// <value>module name</value>
        public string Name { get; set; }

        /// <summary>Returns the module URI if provided. </summary>
        /// <value>module URI</value>
        public string Uri { get; set; }

        /// <summary>Returns the dependencies the module may have on other modules. </summary>
        /// <value>module dependencies</value>
        public ICollection<string> Uses { get; set; }

        /// <summary>Returns a list of statements (some may be comments only) that make up the module. </summary>
        /// <value>statements</value>
        public IList<ModuleItem> Items { get; set; }

        /// <summary>Returns the imports defined by the module. </summary>
        /// <value>module imports</value>
        public ICollection<Import> Imports { get; set; }

        /// <summary>Returns module text. </summary>
        /// <value>text</value>
        public string ModuleText { get; set; }

        public override string ToString()
        {
            var buf = new StringBuilder();
            if (Name == null) {
                buf.Append("(unnamed)");
            }
            else {
                buf.Append("'" + Name + "'");
            }

            if (Uri != null) {
                buf.Append(" uri '" + Uri + "'");
            }

            return buf.ToString();
        }
    }
}
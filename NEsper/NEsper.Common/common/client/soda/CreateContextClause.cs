///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>Create a context. </summary>
    [Serializable]
    public class CreateContextClause
    {
        /// <summary>Ctor. </summary>
        public CreateContextClause()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="contextName">context name</param>
        /// <param name="descriptor">context dimension descriptor</param>
        public CreateContextClause(
            string contextName,
            ContextDescriptor descriptor)
        {
            ContextName = contextName;
            Descriptor = descriptor;
        }

        /// <summary>Returns the context name </summary>
        /// <value>context name</value>
        public string ContextName { get; set; }

        /// <summary>Returns the context dimension informatin </summary>
        /// <value>context descriptor</value>
        public ContextDescriptor Descriptor { get; set; }

        /// <summary>RenderAny as EPL. </summary>
        /// <param name="writer">to output to</param>
        /// <param name="formatter">formatter</param>
        public void ToEPL(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            writer.Write("create context ");
            writer.Write(ContextName);
            writer.Write(" as ");
            Descriptor.ToEPL(writer, formatter);
        }
    }
}
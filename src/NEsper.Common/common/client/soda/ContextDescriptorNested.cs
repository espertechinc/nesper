///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>Nested context. </summary>
    [Serializable]
    public class ContextDescriptorNested : ContextDescriptor
    {
        /// <summary>Ctor. </summary>
        public ContextDescriptorNested()
        {
            Contexts = new List<CreateContextClause>();
        }

        /// <summary>Ctor. </summary>
        /// <param name="contexts">the nested contexts</param>
        public ContextDescriptorNested(IList<CreateContextClause> contexts)
        {
            Contexts = contexts;
        }

        /// <summary>Returns the list of nested contexts </summary>
        /// <value>contexts</value>
        public IList<CreateContextClause> Contexts { get; set; }

        public void ToEPL(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            string delimiter = "";
            foreach (CreateContextClause context in Contexts)
            {
                writer.Write(delimiter);
                writer.Write("context ");
                writer.Write(context.ContextName);
                writer.Write(" as ");
                context.Descriptor.ToEPL(writer, formatter);
                delimiter = ", ";
            }
        }
    }
}
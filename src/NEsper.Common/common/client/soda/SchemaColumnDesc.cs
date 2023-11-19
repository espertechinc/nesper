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
    /// <summary>
    ///     Descriptor for use in create-schema syntax to define property name and type of an event property.
    /// </summary>
    public class SchemaColumnDesc
    {
        private string name;
        private string type;

        /// <summary>
        ///     Ctor.
        /// </summary>
        public SchemaColumnDesc()
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="name">column name</param>
        /// <param name="type">type name</param>
        public SchemaColumnDesc(
            string name,
            string type)
        {
            this.name = name;
            this.type = type;
        }

        /// <summary>
        ///     Returns property name.
        /// </summary>
        /// <returns>name</returns>
        public string Name {
            get => name;
            set => name = value;
        }

        /// <summary>
        ///     Returns property type.
        /// </summary>
        /// <returns>type</returns>
        public string Type {
            get => type;
            set => type = value;
        }

        /// <summary>
        ///     Render to EPL.
        /// </summary>
        /// <param name="writer">to render to</param>
        public void ToEPL(TextWriter writer)
        {
            writer.Write(name);
            writer.Write(' ');
            writer.Write(type);
        }
    }
} // end of namespace
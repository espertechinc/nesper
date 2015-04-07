///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Descriptor for use in create-schema syntax to define property name and type of an event property.
    /// </summary>
    [Serializable]
    public class SchemaColumnDesc
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public SchemaColumnDesc()
        {
        }
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="name">column name</param>
        /// <param name="type">type name</param>
        /// <param name="array">array flag</param>
        public SchemaColumnDesc(string name, string type, bool array)
        {
            Name = name;
            Type = type;
            IsArray = array;
        }
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="name">property name</param>
        /// <param name="type">property type, can be any simple class name or fully-qualified class name or existing event type name</param>
        /// <param name="array">true for array property</param>
        /// <param name="primitiveArray">true for array of primitive (requires array property to be set and a primitive type)</param>
        public SchemaColumnDesc(string name, string type, bool array, bool primitiveArray)
        {
            Name = name;
            Type = type;
            IsArray = array;
            IsPrimitiveArray = primitiveArray;
        }

        /// <summary>
        /// Returns property name.
        /// </summary>
        /// <value>name</value>
        public string Name { get; set; }

        /// <summary>
        /// Returns property type.
        /// </summary>
        /// <value>type</value>
        public string Type { get; set; }

        /// <summary>
        /// Returns true for array properties.
        /// </summary>
        /// <value>indicator</value>
        public bool IsArray { get; set; }

        /// <summary>
        /// Returns indicator whether array of primitives (requires array and a primitive type)
        /// </summary>
        /// <value>indicator</value>
        public bool IsPrimitiveArray { get; set; }

        /// <summary>
        /// Render to EPL.
        /// </summary>
        /// <param name="writer">to render to</param>
        public void ToEPL(TextWriter writer)
        {
            writer.Write(Name);
            writer.Write(' ');
            writer.Write(Type);
            if (IsArray) {
                if (IsPrimitiveArray) {
                    writer.Write("[primitive]");
                }
                else {
                    writer.Write("[]");
                }
            }
        }
    
    }
}

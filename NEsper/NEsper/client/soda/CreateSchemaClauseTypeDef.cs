///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.client.soda
{
    /// <summary>Represents a type definition for use with the create-schema syntax for creating a new event type. </summary>
    [Serializable]
    public enum CreateSchemaClauseTypeDef
    {
        /// <summary>Variant type. </summary>
        VARIANT,

        /// <summary>Map underlying type. </summary>
        MAP,

        /// <summary>Object-array underlying type. </summary>
        OBJECTARRAY,

        /// <summary>Undefined (system default) underlying type. </summary>
        NONE
    }

    public static class CreateSchemaClauseTypeDefExtensions
    {
        /// <summary>
        /// Write keyword according to type def.
        /// </summary>
        /// <param name="typeDef">The type def.</param>
        /// <param name="writer">to write to</param>
        public static void Write(this CreateSchemaClauseTypeDef typeDef, TextWriter writer)
        {
            switch (typeDef)
            {
                case CreateSchemaClauseTypeDef.VARIANT:
                    writer.Write(" variant");
                    break;
                case CreateSchemaClauseTypeDef.MAP:
                    writer.Write(" map");
                    break;
                case CreateSchemaClauseTypeDef.OBJECTARRAY:
                    writer.Write(" objectarray");
                    break;
            }
        }
    }
}

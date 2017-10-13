///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.client.hook
{
    /// <summary>
    /// For Avro schemas for mapping a given type to a given Avro schema.
    /// </summary>
    public interface TypeRepresentationMapper {
        /// <summary>
        /// Return Avro schema for type information provided.
        /// </summary>
        /// <param name="context">type and contextual information</param>
        /// <returns>schema</returns>
        Object Map(TypeRepresentationMapperContext context);
    }
} // end of namespace

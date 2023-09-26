///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.annotation
{
    /// <summary>
    ///     Annotation for use with JSON schemas.
    /// </summary>
    public class JsonSchemaAttribute : Attribute
    {
        /// <summary>
        ///     Flag indicating whether to discard unrecognized property names (the default, false, i.e. non-dynamic)
        ///     or whether to retain all JSON object properties (true, dynamic)
        /// </summary>
        /// <returns>dynamic flag</returns>
        public virtual bool Dynamic { get; set; } = false;

        public virtual string ClassName { get; set; } = "";
    }
} // end of namespace
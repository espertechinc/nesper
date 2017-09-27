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

namespace com.espertech.esper.client.annotation
{
    /// <summary>
    /// Annotation for providing a statement execution hint.
    /// <para>
    /// Hints are providing instructions that can change latency, throughput or memory requirements of a statement.
    /// </para>
    /// </summary>
    public @interface Hint {
    
        /// <summary>
        /// Hint Keyword(s), comma-separated.
        /// </summary>
        /// <returns>keywords</returns>
        string Value() default "";
    
        /// <summary>
        /// Optional information to what the hint applies to
        /// </summary>
        /// <returns>applies</returns>
        AppliesTo Applies() default AppliesTo.UNDEFINED;
    
        /// <summary>
        /// Optional model name.
        /// </summary>
        /// <returns>model name</returns>
        string Model() default "";
    }
} // end of namespace

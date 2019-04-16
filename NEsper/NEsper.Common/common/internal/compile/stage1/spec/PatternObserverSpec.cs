///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    /// Specification for a pattern observer object consists of a namespace, name and object parameters.
    /// </summary>
    [Serializable]
    public class PatternObserverSpec : ObjectSpec
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="namespace">if the namespace the object is in</param>
        /// <param name="objectName">is the name of the object</param>
        /// <param name="objectParameters">is a list of values representing the object parameters</param>
        public PatternObserverSpec(
            string @namespace,
            string objectName,
            IList<ExprNode> objectParameters)
            : base(@namespace, objectName, objectParameters)
        {
        }
    }
} // end of namespace
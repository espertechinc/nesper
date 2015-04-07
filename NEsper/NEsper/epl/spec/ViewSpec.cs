///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.spec
{
    /// <summary>
    /// Specification for a view object consists of a namespace, name and view object parameters.
    /// </summary>
    [Serializable]
    public sealed class ViewSpec : ObjectSpec
    {
        public static readonly ViewSpec[] EMPTY_VIEWSPEC_ARRAY = new ViewSpec[0];

        /// <summary>Constructor. </summary>
        /// <param name="namespace">if the namespace the object is in</param>
        /// <param name="objectName">is the name of the object</param>
        /// <param name="viewParameters">is a list of expressions representing the view parameters</param>
        public ViewSpec(String @namespace, String objectName, IList<ExprNode> viewParameters)
            : base(@namespace, objectName, viewParameters)
        {
        }

        public static ViewSpec[] ToArray(IList<ViewSpec> viewSpecs)
        {
            if (viewSpecs.IsEmpty())
            {
                return EMPTY_VIEWSPEC_ARRAY;
            }
            return viewSpecs.ToArray();
        }
    }
}
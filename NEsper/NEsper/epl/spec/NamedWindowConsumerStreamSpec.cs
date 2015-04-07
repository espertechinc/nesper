///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.property;

namespace com.espertech.esper.epl.spec
{
    /// <summary>
    /// Specification for use of an existing named window.
    /// </summary>
    [Serializable]
    public class NamedWindowConsumerStreamSpec : StreamSpecBase, StreamSpecCompiled
    {
        [NonSerialized] private readonly PropertyEvaluator _optPropertyEvaluator;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="windowName">specifies the name of the named window</param>
        /// <param name="optionalAsName">a name or null if none defined</param>
        /// <param name="viewSpecs">is the view specifications</param>
        /// <param name="filterExpressions">the named window filters</param>
        /// <param name="streamSpecOptions">additional options such as unidirectional stream in a join</param>
        /// <param name="optPropertyEvaluator">The opt property evaluator.</param>
        public NamedWindowConsumerStreamSpec(String windowName, String optionalAsName, ViewSpec[] viewSpecs, IList<ExprNode> filterExpressions, StreamSpecOptions streamSpecOptions, PropertyEvaluator optPropertyEvaluator)
            : base(optionalAsName, viewSpecs, streamSpecOptions)
        {
            WindowName = windowName;
            FilterExpressions = filterExpressions;
            _optPropertyEvaluator = optPropertyEvaluator;
        }

        /// <summary>Returns the window name. </summary>
        /// <value>window name</value>
        public string WindowName { get; private set; }

        /// <summary>Returns list of filter expressions onto the named window, or no filter expressions if none defined. </summary>
        /// <value>list of filter expressions</value>
        public IList<ExprNode> FilterExpressions { get; private set; }

        public PropertyEvaluator OptPropertyEvaluator
        {
            get { return _optPropertyEvaluator; }
        }
    }
}

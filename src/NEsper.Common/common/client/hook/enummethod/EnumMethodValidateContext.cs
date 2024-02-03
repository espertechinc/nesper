///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.methodbase;
using com.espertech.esper.common.@internal.epl.streamtype;

namespace com.espertech.esper.common.client.hook.enummethod
{
    /// <summary>
    ///     Context for use with the enumeration method extension API
    /// </summary>
    public class EnumMethodValidateContext
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="footprintFound">actual footprint chosen</param>
        /// <param name="inputEventType">input event type or null if the input is not a collection of events</param>
        /// <param name="inputCollectionComponentType">type of scalar or object (non-event) input values, or null if the input is a collection of events</param>
        /// <param name="streamTypeService">event type information</param>
        /// <param name="currentMethod">information on the current method</param>
        /// <param name="currentParameters">parameters</param>
        /// <param name="statementRawInfo">EPL statement information</param>
        public EnumMethodValidateContext(
            DotMethodFP footprintFound,
            EventType inputEventType,
            Type inputCollectionComponentType,
            StreamTypeService streamTypeService,
            EnumMethodEnum currentMethod,
            IList<ExprNode> currentParameters,
            StatementRawInfo statementRawInfo)
        {
            FootprintFound = footprintFound;
            InputEventType = inputEventType;
            InputCollectionComponentType = inputCollectionComponentType;
            StreamTypeService = streamTypeService;
            CurrentMethod = currentMethod;
            CurrentParameters = currentParameters;
            StatementRawInfo = statementRawInfo;
        }

        /// <summary>
        ///     Returns the actual footprint chosen.
        /// </summary>
        /// <value>footprint</value>
        public DotMethodFP FootprintFound { get; }

        /// <summary>
        ///     Returns event type information.
        /// </summary>
        /// <value>type info</value>
        public StreamTypeService StreamTypeService { get; }

        /// <summary>
        ///     Returns the enumeration method information
        /// </summary>
        /// <value>current method</value>
        public EnumMethodEnum CurrentMethod { get; }

        /// <summary>
        ///     Returns the parameters to the enumeration method.
        /// </summary>
        /// <value>parameter expressions</value>
        public IList<ExprNode> CurrentParameters { get; }

        /// <summary>
        ///     Returns EPL statement information.
        /// </summary>
        /// <value>statement info</value>
        public StatementRawInfo StatementRawInfo { get; }

        /// <summary>
        ///     Returns the event type of the events that are the input of the enumeration method,
        ///     or null if the input to the enumeration method are scalar value input and not events
        /// </summary>
        /// <value>input event type or null for scalar input</value>
        public EventType InputEventType { get; }

        /// <summary>
        ///     Returns the component type of the values that are the input of the enumeration method,
        ///     or null if the input to the enumeration method are events and not scalar value input
        /// </summary>
        /// <value>scalar value input type or null when the input is events</value>
        public Type InputCollectionComponentType { get; }
    }
} // end of namespace
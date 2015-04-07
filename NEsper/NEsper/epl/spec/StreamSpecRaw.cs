///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.core.service;

namespace com.espertech.esper.epl.spec
{
    /// <summary>
    /// An uncompiled, unoptimize for of stream specification created by a parser.
    /// </summary>
    public interface StreamSpecRaw : StreamSpec
    {
        /// <summary>
        /// Compiles a raw stream specification consisting event type information and filter 
        /// expressions to an validated, optimized form for use with filter service
        /// </summary>
        /// <param name="statementContext">statement-level services</param>
        /// <param name="eventTypeReferences">event type names used by the statement</param>
        /// <param name="isInsertInto">true for insert-into</param>
        /// <param name="assignedTypeNumberStack">The assigned type number stack.</param>
        /// <param name="isJoin">indicates whether a join or not a join</param>
        /// <param name="isContextDeclaration">indicates whether declared as part of the context declarations, if any</param>
        /// <param name="isOnTrigger">if set to <c>true</c> [is on trigger].</param>
        /// <returns>compiled stream</returns>
        /// <throws>ExprValidationException to indicate validation errors</throws>
        StreamSpecCompiled Compile(
            StatementContext statementContext,
            ICollection<String> eventTypeReferences,
            bool isInsertInto,
            ICollection<int> assignedTypeNumberStack,
            bool isJoin,
            bool isContextDeclaration,
            bool isOnTrigger);

    }
}

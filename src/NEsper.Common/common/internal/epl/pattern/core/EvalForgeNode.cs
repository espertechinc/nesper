///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.util;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.schedule;

namespace com.espertech.esper.common.@internal.epl.pattern.core
{
    /// <summary>
    ///     Superclass of all nodes in an evaluation tree representing an event pattern expression.
    ///     Follows the Composite pattern. Child nodes do not carry references to parent nodes, the tree
    ///     is unidirectional.
    /// </summary>
    public interface EvalForgeNode
    {
        /// <summary>
        ///     Returns list of child nodes
        /// </summary>
        /// <returns>list of child nodes</returns>
        IList<EvalForgeNode> ChildNodes { get; }

        short FactoryNodeId { get; set; }

        /// <summary>
        ///     Returns precendence.
        /// </summary>
        /// <returns>precendence</returns>
        PatternExpressionPrecedenceEnum Precedence { get; }

        /// <summary>
        ///     Adds a child node.
        /// </summary>
        /// <param name="childNode">is the child evaluation tree node to add</param>
        void AddChildNode(EvalForgeNode childNode);

        void AddChildNodes(ICollection<EvalForgeNode> childNodes);

        bool IsAudit { get; set; }

        /// <summary>
        ///     Write expression considering precendence.
        /// </summary>
        /// <param name="writer">to use</param>
        /// <param name="parentPrecedence">precendence</param>
        void ToEPL(
            TextWriter writer,
            PatternExpressionPrecedenceEnum parentPrecedence);

        CodegenMethod MakeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope);

        void CollectSelfFilterAndSchedule(
            Func<short, CallbackAttribution> callbackAttribution,
            IList<FilterSpecTracked> filters,
            IList<ScheduleHandleTracked> schedules);
    }
} // end of namespace
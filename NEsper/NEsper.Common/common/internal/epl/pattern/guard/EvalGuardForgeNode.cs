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
using System.Reflection;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat.logging;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.pattern.guard
{
    /// <summary>
    ///     This class represents a guard in the evaluation tree representing an event expressions.
    /// </summary>
    public class EvalGuardForgeNode : EvalForgeNodeBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="patternGuardSpec">factory for guard construction</param>
        public EvalGuardForgeNode(PatternGuardSpec patternGuardSpec)
        {
            PatternGuardSpec = patternGuardSpec;
        }

        /// <summary>
        ///     Returns the guard object specification to use for instantiating the guard factory and guard.
        /// </summary>
        /// <returns>guard specification</returns>
        public PatternGuardSpec PatternGuardSpec { get; }

        /// <summary>
        ///     Returns the guard factory.
        /// </summary>
        /// <returns>guard factory</returns>
        public GuardForge GuardForge { get; set; }

        public bool IsFilterChildNonQuitting => false;

        public bool IsStateful => true;

        public override PatternExpressionPrecedenceEnum Precedence => PatternExpressionPrecedenceEnum.GUARD_POSTFIX;

        public override string ToString()
        {
            return "EvalGuardNode guardForge=" + GuardForge +
                   "  children=" + ChildNodes.Count;
        }

        protected override Type TypeOfFactory()
        {
            return typeof(EvalGuardFactoryNode);
        }

        protected override string NameOfFactory()
        {
            return "guard";
        }

        protected override void InlineCodegen(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block
                .SetProperty(Ref("node"), "ChildNode",
                    LocalMethod(ChildNodes[0].MakeCodegen(method, symbols, classScope)))
                .SetProperty(Ref("node"), "GuardFactory", GuardForge.MakeCodegen(method, symbols, classScope));
        }

        public override void CollectSelfFilterAndSchedule(
            IList<FilterSpecCompiled> filters,
            IList<ScheduleHandleCallbackProvider> schedules)
        {
            GuardForge.CollectSchedule(schedules);
        }

        public string ToPrecedenceFreeEPL()
        {
            var writer = new StringWriter();
            ToPrecedenceFreeEPL(writer);
            return writer.ToString();
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ChildNodes[0].ToEPL(writer, Precedence);
            if (PatternGuardSpec.ObjectNamespace.Equals(GuardEnum.WHILE_GUARD.GetNamespace()) &&
                PatternGuardSpec.ObjectName.Equals(GuardEnum.WHILE_GUARD.GetName())) {
                writer.Write(" while ");
            }
            else {
                writer.Write(" where ");
                writer.Write(PatternGuardSpec.ObjectNamespace);
                writer.Write(":");
                writer.Write(PatternGuardSpec.ObjectName);
            }

            writer.Write("(");
            ExprNodeUtilityPrint.ToExpressionStringParameterList(PatternGuardSpec.ObjectParameters, writer);
            writer.Write(")");
        }
    }
} // end of namespace
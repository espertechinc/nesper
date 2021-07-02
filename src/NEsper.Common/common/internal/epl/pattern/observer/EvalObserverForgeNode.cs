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

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.pattern.observer
{
    /// <summary>
    ///     This class represents an observer expression in the evaluation tree representing an pattern expression.
    /// </summary>
    public class EvalObserverForgeNode : EvalForgeNodeBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ObserverForge observerForge;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="patternObserverSpec">is the factory to use to get an observer instance</param>
        public EvalObserverForgeNode(bool attachPatternText, PatternObserverSpec patternObserverSpec) : base(attachPatternText)
        {
            PatternObserverSpec = patternObserverSpec;
        }

        /// <summary>
        ///     Returns the observer object specification to use for instantiating the observer factory and observer.
        /// </summary>
        /// <returns>observer specification</returns>
        public PatternObserverSpec PatternObserverSpec { get; }

        public override PatternExpressionPrecedenceEnum Precedence => PatternExpressionPrecedenceEnum.ATOM;

        /// <summary>
        ///     Supplies the observer factory to the node.
        /// </summary>
        /// <value>is the observer forge</value>
        public ObserverForge ObserverFactory {
            set => observerForge = value;
        }

        protected override Type TypeOfFactory()
        {
            return typeof(EvalObserverFactoryNode);
        }

        protected override string NameOfFactory()
        {
            return "Observer";
        }

        protected override void InlineCodegen(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block
                .SetProperty(Ref("node"), "ObserverFactory", observerForge.MakeCodegen(method, symbols, classScope));
        }

        public override void CollectSelfFilterAndSchedule(
            IList<FilterSpecCompiled> filters,
            IList<ScheduleHandleCallbackProvider> schedules)
        {
            observerForge.CollectSchedule(schedules);
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(PatternObserverSpec.ObjectNamespace);
            writer.Write(":");
            writer.Write(PatternObserverSpec.ObjectName);
            writer.Write("(");
            ExprNodeUtilityPrint.ToExpressionStringParameterList(PatternObserverSpec.ObjectParameters, writer);
            writer.Write(")");
        }

        public string ToPrecedenceFreeEPL()
        {
            var writer = new StringWriter();
            ToPrecedenceFreeEPL(writer);
            return writer.ToString();
        }

        protected override AppliesTo AppliesTo()
        {
            return client.annotation.AppliesTo.PATTERN_OBSERVER;
        }
    }
} // end of namespace
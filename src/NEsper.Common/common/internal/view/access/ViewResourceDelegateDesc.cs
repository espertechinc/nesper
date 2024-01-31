///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.view.access
{
    /// <summary>
    ///     Coordinates between view factories and requested resource (by expressions) the
    ///     availability of view resources to expressions.
    /// </summary>
    public class ViewResourceDelegateDesc
    {
        public static readonly ViewResourceDelegateDesc[] SINGLE_ELEMENT_ARRAY = new[] {
            new ViewResourceDelegateDesc(false, null, EmptySortedSet<int>.Instance)
        };

        public ViewResourceDelegateDesc(
            bool hasPrevious,
            StateMgmtSetting previousStateSettingsOpt,
            SortedSet<int> priorRequests)
        {
            HasPrevious = hasPrevious;
            PreviousStateSettingsOpt = previousStateSettingsOpt;
            PriorRequests = priorRequests;
        }

        public bool HasPrevious { get; }

        public SortedSet<int> PriorRequests { get; }

        public StateMgmtSetting PreviousStateSettingsOpt { get; }

        public static CodegenExpression ToExpression(ViewResourceDelegateDesc[] descs)
        {
            if (descs.Length == 1 && !descs[0].HasPrevious && descs[0].PriorRequests.IsEmpty()) {
                return PublicConstValue(typeof(ViewResourceDelegateDesc), "SINGLE_ELEMENT_ARRAY");
            }

            var values = new CodegenExpression[descs.Length];
            for (var ii = 0; ii < descs.Length; ii++) {
                values[ii] = descs[ii].ToExpression();
            }

            return NewArrayWithInit(typeof(ViewResourceDelegateDesc), values);
        }

        public CodegenExpression ToExpression()
        {
            return NewInstance(
                GetType(),
                Constant(HasPrevious),
                PreviousStateSettingsOpt == null ? ConstantNull() : PreviousStateSettingsOpt.ToExpression(),
                CodegenLegoRichConstant.ToExpression(PriorRequests));
        }

        public static bool HasPrior(ViewResourceDelegateDesc[] delegates)
        {
            foreach (var @delegate in delegates) {
                if (!@delegate.PriorRequests.IsEmpty()) {
                    return true;
                }
            }

            return false;
        }
    }
} // end of namespace
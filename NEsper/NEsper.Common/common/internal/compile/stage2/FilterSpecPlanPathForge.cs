///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.compile.stage2.FilterSpecPlanForge;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class FilterSpecPlanPathForge
    {
        public FilterSpecPlanPathForge(FilterSpecPlanPathTripletForge[] triplets,
            ExprNode pathNegate)
        {
            Triplets = triplets;
            PathNegate = pathNegate;
        }

        public ExprNode PathNegate { get; set; }

        public FilterSpecPlanPathTripletForge[] Triplets { get; }

        public bool EqualsFilter(FilterSpecPlanPathForge other)
        {
            if (Triplets.Length != other.Triplets.Length) {
                return false;
            }

            for (var i = 0; i < Triplets.Length; i++) {
                var mytriplet = Triplets[i];
                var othertriplet = other.Triplets[i];
                if (!mytriplet.EqualsFilter(othertriplet)) {
                    return false;
                }
            }

            if (!ExprNodeUtilityCompare.DeepEqualsNullChecked(PathNegate, other.PathNegate, true)) {
                return false;
            }

            return true;
        }

        public CodegenMethod Codegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbolWEventType symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(FilterSpecPlanPath), typeof(FilterSpecParamForge), classScope);
            method.Block.DeclareVar(
                typeof(FilterSpecPlanPathTriplet[]),
                "triplets",
                NewArrayByLength(typeof(FilterSpecPlanPathTriplet), Constant(Triplets.Length)));
            for (var i = 0; i < Triplets.Length; i++) {
                var triplet = Triplets[i].Codegen(method, symbols, classScope);
                method.Block.AssignArrayElement("triplets", Constant(i), LocalMethod(triplet));
            }

            method.Block
                .DeclareVar(typeof(FilterSpecPlanPath), "path", NewInstance(typeof(FilterSpecPlanPath)))
                .SetProperty(Ref("path"), "Triplets", Ref("triplets"))
                .SetProperty(Ref("path"), "PathNegate", OptionalEvaluator(PathNegate, method, classScope))
                .MethodReturn(Ref("path"));
            return method;
        }

        public void AppendFilterPlanPath(
            int indexPath,
            StringBuilder stringBuilder)
        {
            stringBuilder
                .Append("  -path #")
                .Append(indexPath)
                .Append(" there are ")
                .Append(Triplets.Length)
                .Append(" triplets")
                .Append(FilterSpecCompiler.NEWLINE);
            
            if (PathNegate != null) {
                stringBuilder
                    .Append("    -path-negate-expression: ")
                    .Append(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(PathNegate))
                    .Append(FilterSpecCompiler.NEWLINE);
            }

            var indextriplet = 0;
            foreach (var forge in Triplets) {
                forge.AppendFilterPlanTriplet(indextriplet, stringBuilder);
                indextriplet++;
            }
        }
    }
} // end of namespace
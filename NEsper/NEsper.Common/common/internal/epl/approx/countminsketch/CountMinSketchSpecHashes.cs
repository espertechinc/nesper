///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.approx.countminsketch
{
    public class CountMinSketchSpecHashes
    {
        public CountMinSketchSpecHashes(
            double epsOfTotalCount,
            double confidence,
            int seed)
        {
            EpsOfTotalCount = epsOfTotalCount;
            Confidence = confidence;
            Seed = seed;
        }

        public double EpsOfTotalCount { get; set; }

        public double Confidence { get; set; }

        public int Seed { get; set; }

        public CodegenExpression CodegenMake(CodegenMethod method, CodegenClassScope classScope)
        {
            return NewInstance<CountMinSketchSpecHashes>(
                Constant(EpsOfTotalCount),
                Constant(Confidence),
                Constant(Seed));
        }

}
}
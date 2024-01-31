///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprNodePropOrStreamExprDesc : ExprNodePropOrStreamDesc
    {
        public ExprNodePropOrStreamExprDesc(
            int streamNum,
            ExprNode originator)
        {
            StreamNum = streamNum;
            Originator = originator;
        }

        public ExprNode Originator { get; }

        public int StreamNum { get; }

        public string Textual => "expression '" +
                                 ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(Originator) +
                                 "' against stream " +
                                 StreamNum;
    }
} // end of namespace
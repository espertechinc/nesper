///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using com.espertech.esper.client.annotation;
using com.espertech.esper.compat.collections;
using com.espertech.esper.util;

namespace com.espertech.esper.pattern
{
    public class EvalAuditInstanceCount
    {

        private readonly IDictionary<EvalFactoryNode, int?> _counts;

        public EvalAuditInstanceCount()
        {
            _counts = new Dictionary<EvalFactoryNode, int?>();
        }

        public void DecreaseRefCount(EvalFactoryNode evalNode, EvalAuditStateNode current, String patternExpr, String statementName, String engineURI)
        {
            int? count = _counts.Get(evalNode);
            if (count == null)
            {
                return;
            }
            count--;
            if (count <= 0)
            {
                _counts.Remove(evalNode);
                Print(current, patternExpr, engineURI, statementName, false, 0);
                return;
            }
            _counts.Put(evalNode, count);
            Print(current, patternExpr, engineURI, statementName, false, count);


        }

        public void IncreaseRefCount(EvalFactoryNode evalNode, EvalAuditStateNode current, String patternExpr, String statementName, String engineURI)
        {
            int? count = _counts.Get(evalNode);
            if (count == null)
            {
                count = 1;
            }
            else
            {
                count++;
            }
            _counts.Put(evalNode, count);
            Print(current, patternExpr, engineURI, statementName, true, count);
        }

        private static void Print(EvalAuditStateNode current, String patternExpression, String engineURI, String statementName, bool added, int? count)
        {
            if (!AuditPath.IsAuditEnabled)
            {
                return;
            }

            var writer = new StringWriter();

            EvalAuditStateNode.WritePatternExpr(current, patternExpression, writer);

            if (added)
            {
                writer.Write(" increased to " + count);
            }
            else
            {
                writer.Write(" decreased to " + count);
            }

            AuditPath.AuditLog(engineURI, statementName, AuditEnum.PATTERNINSTANCES, writer.ToString());
        }
    }
}

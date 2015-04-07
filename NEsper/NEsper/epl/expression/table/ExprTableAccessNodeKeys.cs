///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.epl.expression.table
{
    [Serializable]
    public class ExprTableAccessNodeKeys 
        : ExprTableAccessNode 
        , ExprEvaluator
    {
        public ExprTableAccessNodeKeys(string tableName)
            : base(tableName)
        {
        }
    
        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ToPrecedenceFreeEPLInternal(writer);
            writer.Write(".keys()");
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        protected override void ValidateBindingInternal(ExprValidationContext validationContext, TableMetadata tableMetadata)
        {
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            return Strategy.Evaluate(
                evaluateParams.EventsPerStream, 
                evaluateParams.IsNewData, 
                evaluateParams.ExprEvaluatorContext);
        }

        public virtual Type ReturnType
        {
            get { return typeof (object[]); }
        }

        protected override bool EqualsNodeInternal(ExprTableAccessNode other)
        {
            return true;
        }
    }
}

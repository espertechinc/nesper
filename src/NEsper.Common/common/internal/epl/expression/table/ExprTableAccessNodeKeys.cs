///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.strategy;

namespace com.espertech.esper.common.@internal.epl.expression.table
{
    public class ExprTableAccessNodeKeys : ExprTableAccessNode
    {
        public ExprTableAccessNodeKeys(string tableName)
            : base(tableName)
        {
        }

        public override Type EvaluationType => typeof(object[]);

        public override ExprForge Forge => this;

        protected override string InstrumentationQName => "ExprTableTop";

        protected override CodegenExpression[] InstrumentationQParams => new CodegenExpression[0];

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            ToPrecedenceFreeEPLInternal(writer, flags);
            writer.Write(".Keys()");
        }

        protected override void ValidateBindingInternal(ExprValidationContext validationContext)
        {
        }

        public override ExprTableEvalStrategyFactoryForge TableAccessFactoryForge {
            get {
                var forge = new ExprTableEvalStrategyFactoryForge(TableMeta, null);
                forge.StrategyEnum = ExprTableEvalStrategyEnum.KEYS;
                return forge;
            }
        }

        protected override bool EqualsNodeInternal(ExprTableAccessNode other)
        {
            return true;
        }
    }
} // end of namespace
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.epl.expression.codegen
{
    public class StaticMethodCodegenArgDesc
    {
        private readonly string _blockRefName;
        private readonly Type _declareType;
        private readonly CodegenExpression _argExpression;

        public StaticMethodCodegenArgDesc(
            string blockRefName,
            Type declareType,
            CodegenExpression argExpression)
        {
            this._blockRefName = blockRefName;
            this._declareType = declareType;
            this._argExpression = argExpression;
        }

        public string BlockRefName {
            get => _blockRefName;
        }

        public Type DeclareType {
            get => _declareType;
        }

        public CodegenExpression ArgExpression {
            get => _argExpression;
        }
    }
} // end of namespace
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionUtil;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionConstant : CodegenExpression
    {
        private readonly object _constant;

        public CodegenExpressionConstant(object constant)
        {
            _constant = constant;
        }

        public object Constant => _constant;

        public bool IsNull => _constant == null;

        public void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            RenderConstant(builder, _constant);
        }

        public void MergeClasses(ISet<Type> classes)
        {
            if (_constant == null) {
                return;
            }
            else if (_constant is Array constantArray) {
                classes.AddToSet(_constant.GetType().GetElementType());
                // Add elements from type arrays to the set
                if (constantArray is Type[] typeArray) {
                    typeArray.ForEach(t => classes.AddToSet(t));
                }
                else if (constantArray is object[] objectArray) {
                    objectArray.OfType<Type>().For(t => classes.AddToSet(t));
                }
            }
            else if (_constant.GetType().IsEnum) {
#if DIAGNOSTICS
                Console.WriteLine("MERGE: {0}", _constant.GetType().FullName);
#endif
                classes.AddToSet(_constant.GetType());
            }
            else if (_constant is Type constantType) {
                classes.AddToSet(constantType);
            }
        }

        public void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
        }

        public static void MergeClassConstant(
            Object entryValue,
            Object value)
        {
        }
    }
} // end of namespace
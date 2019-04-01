///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.events;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using static NEsper.Avro.Getter.AvroEventBeanGetterIndexed;

namespace NEsper.Avro.Getter
{
    using Collection = ICollection<object>;

    public class AvroEventBeanGetterNestedIndexRooted : EventPropertyGetterSPI
    {
        private readonly Field _posTop;
        private readonly int _index;
        private readonly AvroEventPropertyGetter _nested;

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="record">record</param>
        /// <param name="posTop">postop</param>
        /// <param name="index">index</param>
        /// <exception cref="PropertyAccessException">ex</exception>
        /// <returns>value</returns>
        public static GenericRecord GetAtIndex(GenericRecord record, Field posTop, int index)
        {
            var values = (Collection)record.Get(posTop);
            var value = GetAvroIndexedValue(values, index);
            if (value == null || !(value is GenericRecord))
            {
                return null;
            }
            return (GenericRecord)value;
        }

        public AvroEventBeanGetterNestedIndexRooted(Field posTop, int index, AvroEventPropertyGetter nested)
        {
            this._posTop = posTop;
            this._index = index;
            this._nested = nested;
        }

        public object Get(EventBean eventBean)
        {
            var record = (GenericRecord)eventBean.Underlying;
            var inner = GetAtIndex(record, _posTop, _index);
            return inner == null ? null : _nested.GetAvroFieldValue(inner);
        }

        private string GetCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(object), typeof(GenericRecord), "record", this.GetType())
                .DeclareVar(
                    typeof(GenericRecord), "inner",
                    StaticMethodTakingExprAndConst(this.GetType(), "GetAtIndex", Ref("record"), _posTop, _index))
                .IfRefNullReturnNull("inner")
                .MethodReturn(_nested.CodegenUnderlyingGet(Ref("inner"), context));
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public object GetFragment(EventBean eventBean)
        {
            var record = (GenericRecord)eventBean.Underlying;
            var values = (Collection)record.Get(_posTop);
            var value = GetAvroIndexedValue(values, _index);
            if (value == null || !(value is GenericRecord))
            {
                return null;
            }
            return _nested.GetAvroFragment((GenericRecord)value);
        }

        private string GetFragmentCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(object), typeof(GenericRecord), "record", this.GetType())
                .DeclareVar(
                    typeof(Collection), "values",
                    Cast(typeof(Collection), ExprDotMethod(Ref("record"), "Get", Constant(_posTop))))
                .DeclareVar(
                    typeof(object), "value",
                    StaticMethod(
                        typeof(AvroEventBeanGetterIndexed), "GetAvroIndexedValue", Ref("values"), Constant(_index)))
                .IfRefNullReturnNull("value")
                .IfRefNotTypeReturnConst("value", typeof(GenericRecord), null)
                .MethodReturn(_nested.CodegenUnderlyingFragment(Cast(typeof(GenericRecord), Ref("value")), context));
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingGet(CastUnderlying(typeof(GenericRecord), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingFragment(CastUnderlying(typeof(GenericRecord), beanExpression), context);
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return LocalMethod(GetCodegen(context), underlyingExpression);
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return LocalMethod(GetFragmentCodegen(context), underlyingExpression);
        }
    }
} // end of namespace

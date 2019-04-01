///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.events;

using NEsper.Avro.Core;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterNestedIndexRootedMultilevel : EventPropertyGetterSPI
    {
        private readonly Field _posTop;
        private readonly int _index;
        private readonly AvroEventPropertyGetter[] _nested;

        public AvroEventBeanGetterNestedIndexRootedMultilevel(Field posTop, int index, AvroEventPropertyGetter[] nested)
        {
            _posTop = posTop;
            _index = index;
            _nested = nested;
        }

        public Object Get(EventBean eventBean)
        {
            var value = Navigate((GenericRecord)eventBean.Underlying);
            if (value == null)
            {
                return null;
            }
            return _nested[_nested.Length - 1].GetAvroFieldValue(value);
        }

        private string GetCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(Object), typeof(GenericRecord), "record", GetType())
                .DeclareVar(typeof(GenericRecord), "value", LocalMethod(NavigateMethodCodegen(context), Ref("record")))
                .IfRefNullReturnNull("value")
                .MethodReturn(_nested[_nested.Length - 1].CodegenUnderlyingGet(Ref("value"), context));
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public Object GetFragment(EventBean eventBean)
        {
            var value = Navigate((GenericRecord)eventBean.Underlying);
            if (value == null)
            {
                return null;
            }
            return _nested[_nested.Length - 1].GetAvroFragment(value);
        }

        private string GetFragmentCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(Object), typeof(GenericRecord), "record", GetType())
                .DeclareVar(typeof(GenericRecord), "value", LocalMethod(NavigateMethodCodegen(context), Ref("record")))
                .IfRefNullReturnNull("value")
                .MethodReturn(_nested[_nested.Length - 1].CodegenUnderlyingFragment(Ref("value"), context));
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

        private GenericRecord Navigate(GenericRecord record)
        {
            Object value = AvroEventBeanGetterNestedIndexRooted.GetAtIndex(record, _posTop, _index);
            if (value == null)
            {
                return null;
            }
            return NavigateRecord((GenericRecord)value);
        }

        private string NavigateMethodCodegen(ICodegenContext context)
        {
            var navigateRecordMethod = NavigateRecordMethodCodegen(context);
            return context.AddMethod(typeof(GenericRecord), typeof(GenericRecord), "record", GetType())
                .DeclareVar(typeof(Object), "value", StaticMethodTakingExprAndConst(typeof(AvroEventBeanGetterNestedIndexRooted), "GetAtIndex", Ref("record"), _posTop, _index))
                .IfRefNullReturnNull("value")
                .MethodReturn(LocalMethod(navigateRecordMethod, CastRef(typeof(GenericRecord), "value")));
        }

        private GenericRecord NavigateRecord(GenericRecord record)
        {
            var current = record;
            for (var i = 0; i < _nested.Length - 1; i++)
            {
                var value = _nested[i].GetAvroFieldValue(current);
                if (!(value is GenericRecord))
                {
                    return null;
                }
                current = (GenericRecord)value;
            }
            return current;
        }

        private string NavigateRecordMethodCodegen(ICodegenContext context)
        {
            var block = context.AddMethod(typeof(GenericRecord), typeof(GenericRecord), "record", GetType())
                .DeclareVar(typeof(GenericRecord), "current", Ref("record"))
                .DeclareVarNull(typeof(Object), "value");
            for (var i = 0; i < _nested.Length - 1; i++)
            {
                block.AssignRef("value", _nested[i].CodegenUnderlyingGet(Ref("current"), context))
                    .IfRefNotTypeReturnConst("value", typeof(GenericRecord), null)
                    .AssignRef("current", CastRef(typeof(GenericRecord), "value"));
            }

            return block.MethodReturn(Ref("current"));
        }
    }
} // end of namespace

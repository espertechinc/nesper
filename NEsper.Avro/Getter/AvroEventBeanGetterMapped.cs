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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.magic;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace NEsper.Avro.Getter
{
    using Map = IDictionary<string, object>;

    public class AvroEventBeanGetterMapped : AvroEventPropertyGetter
    {
        private readonly string _key;
        private readonly Field _pos;

        public AvroEventBeanGetterMapped(Field pos, string key)
        {
            _pos = pos;
            _key = key;
        }

        public Object Get(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            var values = (Map) record.Get(_pos);
            return GetAvroMappedValueWNullCheck(values, _key);
        }

        public Object GetAvroFieldValue(GenericRecord record)
        {
            var values = (Map) record.Get(_pos);
            return GetAvroMappedValueWNullCheck(values, _key);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public bool IsExistsPropertyAvro(GenericRecord record)
        {
            return true;
        }

        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }

        public Object GetAvroFragment(GenericRecord record)
        {
            return null;
        }

        public static Object GetMappedValue(object map, string key)
        {
            if (map == null)
            {
                return null;
            }

            var magicMap = MagicMarker.GetStringDictionary(map);
            return magicMap.Get(key);
        }

        private string GetAvroFieldValueCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(Object), typeof(GenericRecord), "record", GetType())
                .DeclareVar(typeof(Map), "values", Cast(typeof(Map), ExprDotMethod(Ref("record"), "get", Constant(_pos))))
                .IfRefNullReturnNull("values")
                .MethodReturn(ExprDotMethod(Ref("values"), "get", Constant(_key)));
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
            return ConstantNull();
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return LocalMethod(GetAvroFieldValueCodegen(context), underlyingExpression);
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantNull();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="map">map</param>
        /// <param name="key">key</param>
        /// <returns>value</returns>
        public static Object GetAvroMappedValueWNullCheck(Map map, string key)
        {
            return map?.Get(key);
        }
    }
} // end of namespace
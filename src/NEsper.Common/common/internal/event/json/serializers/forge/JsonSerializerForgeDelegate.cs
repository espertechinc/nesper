///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.serializers.forge
{
	public class JsonSerializerForgeDelegate : JsonSerializerForge {
	    private readonly string delegateFactoryClassName;

	    public JsonSerializerForgeDelegate(string delegateFactoryClassName) {
	        this.delegateFactoryClassName = delegateFactoryClassName;
	    }

	    public CodegenExpression CodegenSerialize(JsonSerializerForgeRefs refs, CodegenMethod method, CodegenClassScope classScope) {
	        return StaticMethod(delegateFactoryClassName, "WriteStatic", refs.Context, refs.Field);
	    }
	}
} // end of namespace

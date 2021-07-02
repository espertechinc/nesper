///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.method.core;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.agg.method;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;

namespace com.espertech.esper.common.@internal.epl.agg.method.firstlastever
{
	public class AggregationForgeFactoryFirstLastEver : AggregationForgeFactoryBase {
		private readonly ExprFirstLastEverNode _parent;
	    private readonly Type _childType;
	    private readonly DataInputOutputSerdeForge _serde;
	    private AggregatorMethod _aggregator;

	    public AggregationForgeFactoryFirstLastEver(ExprFirstLastEverNode parent, Type childType, DataInputOutputSerdeForge serde) {
	        _parent = parent;
	        _childType = childType;
	        _serde = serde;
	    }

	    public override Type ResultType => _childType;

	    public override void InitMethodForge(int col, CodegenCtor rowCtor, CodegenMemberCol membersColumnized, CodegenClassScope classScope) {
	        if (_parent.IsFirst) {
	            _aggregator = new AggregatorFirstEver(this, col, rowCtor, membersColumnized, classScope, null, null, _parent.HasFilter, _parent.OptionalFilter, _childType, _serde);
	        } else {
	            _aggregator = new AggregatorLastEver(this, col, rowCtor, membersColumnized, classScope, null, null, _parent.HasFilter, _parent.OptionalFilter, _childType, _serde);
	        }
	    }

	    public override AggregatorMethod Aggregator => _aggregator;

	    public override ExprAggregateNodeBase AggregationExpression => _parent;

	    public override AggregationPortableValidation AggregationPortableValidation => new AggregationPortableValidationFirstLastEver(_parent.IsDistinct, _parent.HasFilter, _childType, _parent.IsFirst);

	    public override ExprForge[] GetMethodAggregationForge(bool join, EventType[] typesPerStream) {
	        return ExprMethodAggUtil.GetDefaultForges(_parent.PositionalParams, join, typesPerStream);
	    }
	}

} // end of namespace

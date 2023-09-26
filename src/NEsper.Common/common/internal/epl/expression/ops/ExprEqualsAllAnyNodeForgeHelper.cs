///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat.collections;

public static class ExprEqualsAllAnyNodeForgeHelper
{
    public static CodegenExpression ItemToCollectionUnboxing(
        CodegenExpression itemExpression,
        Type itemType,
        Type collectionType)
    {
        // We need to determine if the item needs to be unboxed in order to be checked in a container.
        // We cannot call "Contains" on a collection of unboxed (value-types) with a boxed value.

        if (ReferenceEquals(collectionType, itemType)) {
            // collectionType is same as itemType
            // - nothing to do
            return itemExpression;
        }
        else if (collectionType.IsNullable()) {
            // Let's make some assumptions that we've done sufficient type checking and that
            // the underlying data type for item is the same as the collection, but is just
            // a boxed or unboxed form.  If this is a reasonable assumption, then the type
            // for item is the unboxed value and this will implicitly upcast.
            // - nothing to do
            return itemExpression;
        }
        else {
            // As with the previous block, this section means that the collection is a value
            // type and the item is a boxed type (which it should be).  In this case, we
            // need to unbox the item value prior to calling contains.
            return CodegenExpressionBuilder.Unbox(itemExpression, itemType);
        }
    }
}
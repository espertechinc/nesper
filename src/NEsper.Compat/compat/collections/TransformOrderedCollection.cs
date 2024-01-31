///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.compat.collections
{
    public class TransformOrderedCollection<TInt,TExt> : TransformCollection<TInt, TExt>
        , IOrderedCollection<TExt>
    {
        /// <summary>
        /// Underlying collection
        /// </summary>
        private readonly IOrderedCollection<TInt> _orderedCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformCollection&lt;TInt, TExt&gt;"/> class.
        /// </summary>
        /// <param name="orderedCollection">The ordered collection.</param>
        /// <param name="transformExtInt">The transform ext int.</param>
        /// <param name="transformIntExt">The transform int ext.</param>
        public TransformOrderedCollection(
            IOrderedCollection<TInt> orderedCollection,
            Func<TExt, TInt> transformExtInt,
            Func<TInt, TExt> transformIntExt) : base(
            orderedCollection,
            transformExtInt,
            transformIntExt)
        {
            _orderedCollection = orderedCollection;
        }

        public TExt FirstEntry => IntToExt(_orderedCollection.FirstEntry);
        public TExt LastEntry => IntToExt(_orderedCollection.LastEntry);

        public IOrderedCollection<TExt> Head(
            TExt value,
            bool isInclusive = false)
        {
            return new TransformOrderedCollection<TInt, TExt>(
                _orderedCollection.Head(ExtToInt(value), isInclusive),
                ExtToInt,
                IntToExt);
        }

        public IOrderedCollection<TExt> Tail(
            TExt value,
            bool isInclusive = true)
        {
            return new TransformOrderedCollection<TInt, TExt>(
                _orderedCollection.Tail(ExtToInt(value), isInclusive),
                ExtToInt,
                IntToExt);
        }

        public IOrderedCollection<TExt> Between(
            TExt startValue,
            bool isStartInclusive,
            TExt endValue,
            bool isEndInclusive)
        {
            return new TransformOrderedCollection<TInt, TExt>(
                _orderedCollection.Between(
                    ExtToInt(startValue), isStartInclusive,
                    ExtToInt(endValue), isEndInclusive),
                ExtToInt,
                IntToExt);
        }

        public TExt GreaterThanOrEqualTo(TExt value)
        {
            return IntToExt(_orderedCollection.GreaterThanOrEqualTo(ExtToInt(value)));
        }

        public bool TryGreaterThanOrEqualTo(
            TExt value,
            out TExt result)
        {
            if (_orderedCollection.TryGreaterThanOrEqualTo(ExtToInt(value), out var intResult)) {
                result = IntToExt(intResult);
                return true;
            }

            result = default;
            return false;
        }

        public TExt LessThanOrEqualTo(TExt value)
        {
            return IntToExt(_orderedCollection.LessThanOrEqualTo(ExtToInt(value)));
        }

        public bool TryLessThanOrEqualTo(
            TExt value,
            out TExt result)
        {
            if (_orderedCollection.TryLessThanOrEqualTo(ExtToInt(value), out var intResult)) {
                result = IntToExt(intResult);
                return true;
            }

            result = default;
            return false;
        }

        public TExt GreaterThan(TExt value)
        {
            return IntToExt(_orderedCollection.GreaterThan(ExtToInt(value)));
        }

        public bool TryGreaterThan(
            TExt value,
            out TExt result)
        {
            if (_orderedCollection.TryGreaterThan(ExtToInt(value), out var intResult)) {
                result = IntToExt(intResult);
                return true;
            }

            result = default;
            return false;
        }

        public TExt LessThan(TExt value)
        {
            return IntToExt(_orderedCollection.LessThan(ExtToInt(value)));
        }

        public bool TryLessThan(
            TExt value,
            out TExt result)
        {
            if (_orderedCollection.TryLessThan(ExtToInt(value), out var intResult)) {
                result = IntToExt(intResult);
                return true;
            }

            result = default;
            return false;
        }
    }
}

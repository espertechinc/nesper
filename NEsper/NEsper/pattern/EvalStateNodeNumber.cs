///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.pattern
{
    [Serializable]
    public class EvalStateNodeNumber 
    {
        private readonly int[] _stateNumber;
        private int _hashCode;
    
        /// <summary>Ctor - constructs a top-level node number. </summary>
        public EvalStateNodeNumber()
        {
            _stateNumber = new int[0];
            ComputeHashCode();
        }
    
        /// <summary>Contructs a given node number. </summary>
        /// <param name="number">to contruct</param>
        public EvalStateNodeNumber(int[] number)
        {
            _stateNumber = number;
            ComputeHashCode();
        }

        /// <summary>Get the depth of the node number. </summary>
        /// <value>ordinal</value>
        public int OrdinalNumber
        {
            get { return _stateNumber[_stateNumber.Length - 1]; }
        }

        /// <summary>Generates a new child node number to the current node number with the given child id. </summary>
        /// <param name="newStateNumber">child's node num</param>
        /// <returns>child node num</returns>
        public EvalStateNodeNumber NewChildNum(int newStateNumber)
        {
            int[] num = new int[_stateNumber.Length + 1];
            Array.Copy(_stateNumber, 0, num, 0, _stateNumber.Length);
            num[_stateNumber.Length] = newStateNumber;
            return new EvalStateNodeNumber(num);
        }
    
        /// <summary>Generates a new sibling node number to the current node. </summary>
        /// <returns>sibling node</returns>
        public EvalStateNodeNumber NewSiblingState()
        {
            int size = _stateNumber.Length;
            int[] num = new int[size];
            Array.Copy(_stateNumber, 0, num, 0, size);
            num[size - 1] = _stateNumber[size - 1] + 1;
            return new EvalStateNodeNumber(num);
        }
    
        public override String ToString()
        {
            return _stateNumber.Render();
        }

        /// <summary>Returns the internal number representation. </summary>
        /// <value>state number</value>
        public int[] StateNumber
        {
            get { return _stateNumber; }
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }
    
        public override bool Equals(Object otherObj)
        {
            if (!(otherObj is EvalStateNodeNumber))
            {
                return false;
            }

            EvalStateNodeNumber other = (EvalStateNodeNumber) otherObj;
            int[] otherNum = other.StateNumber;
            if (otherNum.Length != _stateNumber.Length)
            {
                return false;
            }
            for (int i = 0; i < _stateNumber.Length; i++)
            {
                if (otherNum[i] != _stateNumber[i])
                {
                    return false;
                }
            }
            return true;
        }
    
        private void ComputeHashCode()
        {
            _hashCode = 7;
            for (int i = 0; i < _stateNumber.Length; i++)
            {
                _hashCode ^= _stateNumber[i];
            }
        }
    }
}

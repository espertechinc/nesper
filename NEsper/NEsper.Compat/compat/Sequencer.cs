///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

namespace com.espertech.esper.compat
{
    public class Sequencer
    {
        private int _currentPos;
        private int _currentGaps;
        private int _startPos;
        private readonly List<ISequence> _seqList;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sequencer"/> class.
        /// </summary>
        public Sequencer()
        {
            _seqList = new List<ISequence>();
            _currentPos = 0;
            _currentGaps = 0;
            _startPos = 0;
        }

        /// <summary>
        /// Allocates this instance.
        /// </summary>
        /// <returns></returns>
        internal ISequence Allocate()
        {
            _PrivateSequence _seq;

            if (_currentGaps != 0)
            {
                int _endPos = _seqList.Count;
                for (int ii = _startPos; ii < _endPos; ii++)
                {
                    if (_seqList[ii] == null)
                    {
                        _seq = new _PrivateSequence(this, ii);
                        if (--_currentGaps == 0)
                        {
                            _startPos = 0;
                        }
                        else
                        {
                            _startPos = ii + 1;
                        }

                        return _seq;
                    }
                }

                throw new ArgumentException("Sequencer failed to render unique value, but reported gaps");
            }

            _seq = new _PrivateSequence(this, ++_currentPos);
            _seqList.Add(_seq);

            return _seq;
        }

        /// <summary>
        /// Releases the specified sequence.
        /// </summary>
        /// <param name="sequence">The sequence.</param>
        internal void Release(int sequence)
        {
            _seqList[sequence] = null;
            ++_currentGaps;
            if (sequence < _startPos)
            {
                _startPos = sequence;
            }
        }

        internal class _PrivateSequence : ISequence
        {
            private readonly Sequencer _sequencer;
            private readonly int _sequence;

            /// <summary>
            /// Initializes a new instance of the <see cref="_PrivateSequence"/> class.
            /// </summary>
            /// <param name="sequencer">The sequencer.</param>
            /// <param name="sequence">The sequence.</param>
            public _PrivateSequence(Sequencer sequencer, int sequence)
            {
                this._sequencer = sequencer;
                this._sequence = sequence;
            }

            /// <summary>
            /// Gets the sequence.
            /// </summary>
            /// <value>The sequence.</value>
            public int Sequence
            {
                get { return _sequence; }
            }

            #region IDisposable Members

            public void Dispose()
            {
                _sequencer.Release(_sequence);
            }

            #endregion
        }
    }

    public interface ISequence : IDisposable
    {
        int Sequence { get; }
    }
}

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.virtualdw;
using com.espertech.esper.view;

namespace com.espertech.esper.core.service
{
    /// <summary>Analysis result for joins.</summary>
    public class StreamJoinAnalysisResult
    {
        private readonly int _numStreams;
        private readonly bool[] _isUnidirectionalInd;
        private readonly bool[] _isUnidirectionalNonDriving;
        private bool _isPureSelfJoin;
        private readonly bool[] _hasChildViews;
        private readonly bool[] _isNamedWindow;
        private readonly VirtualDWViewProviderForAgentInstance[] _viewExternal;
        private readonly string[][][] _uniqueKeys;
        private readonly TableMetadata[] _tablesPerStream;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="numStreams">number of streams</param>
        public StreamJoinAnalysisResult(int numStreams)
        {
            _numStreams = numStreams;
            _isPureSelfJoin = false;
            _isUnidirectionalInd = new bool[numStreams];
            _isUnidirectionalNonDriving = new bool[numStreams];
            _hasChildViews = new bool[numStreams];
            _isNamedWindow = new bool[numStreams];
            _viewExternal = new VirtualDWViewProviderForAgentInstance[numStreams];
            _uniqueKeys = new string[numStreams][][];
            _tablesPerStream = new TableMetadata[numStreams];
        }

        /// <summary>
        /// Returns unidirection ind.
        /// </summary>
        /// <value>unidirectional flags</value>
        public bool[] UnidirectionalInd
        {
            get { return _isUnidirectionalInd; }
        }

        /// <summary>
        /// Sets flag.
        /// </summary>
        /// <param name="index">index</param>
        public void SetUnidirectionalInd(int index) {
            _isUnidirectionalInd[index] = true;
        }

        /// <summary>
        /// Returns non-driving unidirectional streams when partial self-joins.
        /// </summary>
        /// <value>indicators</value>
        public bool[] UnidirectionalNonDriving
        {
            get { return _isUnidirectionalNonDriving; }
        }

        /// <summary>
        /// Sets flag.
        /// </summary>
        /// <param name="index">index</param>
        public void SetUnidirectionalNonDriving(int index) {
            _isUnidirectionalNonDriving[index] = true;
        }

        /// <summary>
        /// True for self-join.
        /// </summary>
        /// <value>self-join</value>
        public bool IsPureSelfJoin
        {
            get { return _isPureSelfJoin; }
            set { _isPureSelfJoin = value; }
        }

        /// <summary>
        /// Returns child view flags.
        /// </summary>
        /// <value>flags</value>
        public bool[] HasChildViews
        {
            get { return _hasChildViews; }
        }

        /// <summary>
        /// Sets child view flags.
        /// </summary>
        /// <param name="index">to set</param>
        public void SetHasChildViews(int index) {
            this._hasChildViews[index] = true;
        }

        /// <summary>
        /// Return named window flags.
        /// </summary>
        /// <value>flags</value>
        public bool[] NamedWindow
        {
            get { return _isNamedWindow; }
        }

        /// <summary>
        /// Sets named window flag
        /// </summary>
        /// <param name="index">to set</param>
        public void SetNamedWindow(int index) {
            _isNamedWindow[index] = true;
        }

        /// <summary>
        /// Returns streams num.
        /// </summary>
        /// <value>num</value>
        public int NumStreams
        {
            get { return _numStreams; }
        }

        public VirtualDWViewProviderForAgentInstance[] ViewExternal
        {
            get { return _viewExternal; }
        }

        public string[][][] UniqueKeys
        {
            get { return _uniqueKeys; }
        }

        public void SetTablesForStream(int streamNum, TableMetadata metadata) {
            this._tablesPerStream[streamNum] = metadata;
        }

        public TableMetadata[] TablesPerStream
        {
            get { return _tablesPerStream; }
        }

        public void AddUniquenessInfo(ViewFactoryChain[] unmaterializedViewChain, Attribute[] annotations)
        {
            for (int i = 0; i < unmaterializedViewChain.Length; i++)
            {
                if (unmaterializedViewChain[i].DataWindowViewFactoryCount > 0)
                {
                    var uniquenessProps =
                        ViewServiceHelper.GetUniqueCandidateProperties(
                            unmaterializedViewChain[i].FactoryChain, annotations);
                    if (uniquenessProps != null)
                    {
                        _uniqueKeys[i] = new string[1][];
                        _uniqueKeys[i][0] = uniquenessProps.ToArray();
                    }
                }
            }
        }

        public bool IsUnidirectional
        {
            get
            {
                foreach (bool ind in _isUnidirectionalInd)
                {
                    if (ind)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public int UnidirectionalStreamNumberFirst
        {
            get
            {
                for (int i = 0; i < _isUnidirectionalInd.Length; i++)
                {
                    if (_isUnidirectionalInd[i])
                    {
                        return i;
                    }
                }
                throw new IllegalStateException();
            }
        }

        public bool IsUnidirectionalAll
        {
            get
            {
                foreach (bool ind in _isUnidirectionalInd)
                {
                    if (!ind)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public int UnidirectionalCount
        {
            get
            {
                int count = 0;
                foreach (bool ind in _isUnidirectionalInd)
                {
                    count += ind ? 1 : 0;
                }
                return count;
            }
        }
    }
} // end of namespace

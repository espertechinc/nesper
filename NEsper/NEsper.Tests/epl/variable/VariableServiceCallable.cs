///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.epl.variable
{
    public class VariableServiceCallable : ICallable<bool>
    {
        private readonly Random _random;
        private readonly String[] _variables;
        private readonly VariableReader[] _readers;
        private readonly VariableService _variableService;
        private readonly VariableVersionCoord _variableVersionCoord;
        private readonly int _numLoops;
        private readonly int[][] _results;
        private readonly int[] _marks;
    
        public VariableServiceCallable(String[] variables, VariableService variableService, VariableVersionCoord variableVersionCoord, int numLoops)
        {
            _random = new Random();
            _variables = variables;
            _variableService = variableService;
            _variableVersionCoord = variableVersionCoord;
            _numLoops = numLoops;
            
            _results = new int[numLoops][];
            for( int ii = 0 ; ii < numLoops ; ii++ ) {
                _results[ii] = new int[variables.Length];
            }

            _marks = new int[numLoops];
    
            _readers = new VariableReader[variables.Length];
            for (int i = 0; i < variables.Length; i++)
            {
                _readers[i] = variableService.GetReader(variables[i], 0);
            }
        }

        public bool Call()
        {
            // For each loop
            for (int i = 0; i < _numLoops; i++)
            {
                DoLoop(i);
            }
    
            return true; // assertions therefore return a result that fails the test
        }
    
        private void DoLoop(int loopNumber)
        {
            // Set a mark, there should be no number above that number
            int mark = _variableVersionCoord.SetVersionGetMark();
            int[] indexes = GetIndexesShuffled(_variables.Length, _random, loopNumber);
            _marks[loopNumber] = mark;
    
            // Perform first read of all variables
            int[] readResults = new int[_variables.Length];
            ReadAll(indexes, readResults, mark);
    
            // Start a write cycle for the write we are getting an exclusive write lock
            using (_variableService.ReadWriteLock.AcquireWriteLock()) {
                // Write every second of the variables
                for (int i = 0; i < indexes.Length; i++) {
                    int variableNum = indexes[i];
                    String variableName = _variables[variableNum];

                    if (i%2 == 0) {
                        int newMark = _variableVersionCoord.IncMark();
                        if (Log.IsDebugEnabled) {
                            Log.Debug(".run Thread {0} at mark {1} write variable '{2}' new value {3}",
                                            Thread.CurrentThread.ManagedThreadId,
                                            mark,
                                            variableName,
                                            newMark);
                        }
                        _variableService.Write(_readers[variableNum].VariableMetaData.VariableNumber, 0, newMark);
                    }
                }

                // Commit (apply) the changes and unlock
                _variableService.Commit();
            }
    
            // Read again and compare to first result
            _results[loopNumber] = new int[_variables.Length];
            ReadAll(indexes, _results[loopNumber], mark);
    
            // compare first read with second read, written values are NOT visible
            for (int i = 0; i < _variables.Length; i++)
            {
                if (_results[loopNumber][i] != readResults[i])
                {
                    String text = "Error in loop#" + loopNumber +
                            " comparing a re-read result for variable " + _variables[i] +
                            " expected " + readResults[i] +
                            " but was " + _results[loopNumber][i];
                    Assert.Fail(text);
                }
            }
        }
    
        private void ReadAll(int[] indexes, int[] results, int mark)
        {
            for (int j = 0; j < indexes.Length; j++)
            {
                int index = indexes[j];
                String variableName = _variables[index];
                int value = (int) _readers[index].Value;
                results[index] = value;
    
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(".run Thread " + Thread.CurrentThread.ManagedThreadId + " at mark " + mark + " read variable '" + variableName + " value " + value);
                }            
            }
        }

        public int[][] Results
        {
            get { return _results; }
        }

        public int[] Marks
        {
            get { return _marks; }
        }

        // Make a random list between 0 and Count for each variable
        private static int[] GetIndexesShuffled(int length, Random random, int loopNum)
        {
            int[] indexRandomized = new int[length];
    
            for (int i = 0; i < indexRandomized.Length; i++)
            {
                indexRandomized[i] = i;
            }
    
            for (int i = 0; i < length; i++)
            {
                int indexOne = random.Next(length);
                int indexTwo = random.Next(length);
                int temp = indexRandomized[indexOne];
                indexRandomized[indexOne] = indexRandomized[indexTwo];
                indexRandomized[indexTwo] = temp;
            }
    
            return indexRandomized;
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.util;


namespace com.espertech.esperio.csv
{
	/// <summary>
	/// A source that processes a CSV file and returns CSV recordsfrom that file.
	/// </summary>
	public class CSVReader
	{
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

	    private readonly CSVSource _source;
		
		private readonly List<String> _values = new List<String>();
		private bool _isClosed = false;
		private bool _atEof = false;
		private bool _isReset = true;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="adapterInputSource">the source of the CSV file</param>
        /// <exception cref="ArgumentException">AdapterInputSource cannot be null</exception>
        /// <throws>EPException in case of errors in reading the CSV file</throws>

        public CSVReader(IContainer container, AdapterInputSource adapterInputSource)
		{
			if(adapterInputSource == null)
			{
				throw new ArgumentException("AdapterInputSource cannot be null");
			}
			_source = new CSVSource(container, adapterInputSource);
		}

		/// <summary>Close the source and release the input source.</summary>
		/// <throws>EPException in case of error in closing resources</throws>

		public void Close()
		{
			if(_isClosed)
			{
				throw new EPException("Calling Close() on an already closed CSVReader");
			}
			try
			{
				_isClosed = true;
				_source.Close();
			} 
			catch (IOException e)
			{
				throw new EPException(e);
			}
		}

		/// <summary>Get the next record from the CSV file.</summary>
		/// <returns>a string array containing the values of the record</returns>
		/// <throws>EOFException in case no more records can be read (end-of-file has been reached and isLooping is false)</throws>
		/// <throws>EPException in case of error in reading the CSV file</throws>

		public String[] GetNextRecord()
		{
            try
            {
                String[] result = GetNextValidRecord();

                if (_atEof && result == null)
                {
                    throw new EndOfStreamException("In reading CSV file, reached end-of-file and not looping to the beginning");
                }

                if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
                {
                    Log.Debug(".GetNextRecord record==" + result.Render());
                }

                return result;
            }
            catch (ObjectDisposedException e)
            {
                throw new EPException(e);
            }
            catch (EndOfStreamException)
            {
                throw;
            }
            catch (IOException e)
            {
                throw new EPException(e);
            }
		}

	    /// <summary>Gets or sets the isUsingTitleRow value.</summary>
	    public bool IsUsingTitleRow { get; set; }

	    /// <summary>Gets or sets the looping value.</summary>
	    public bool Looping { get; set; }

	    /// <summary>Reset the source to the beginning of the file.</summary>
		/// <throws>EPException in case of errors in resetting the source</throws>

		public void Reset() 
		{
			try
			{
                if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
                {
                    Log.Debug(".reset");
                }
			    _source.Reset();
				_atEof = false;
				if(IsUsingTitleRow)
				{
					// Ignore the title row
					GetNextRecord();
				}
				_isReset = true;
			} 
			catch (IOException e)
			{
				throw new EPException(e);
			}
		}
		
		/// <summary>Return and set to false the isReset value, which is set totrue whenever the CSVReader is reset.</summary>
		/// <returns>isReset</returns>

		public bool GetAndClearIsReset()
		{
			bool result = _isReset;
			_isReset = false;
			return result;
		}
		
		/// <summary>Return true if this CSVReader supports the reset() method.</summary>
		/// <returns>true if the underlying AdapterInputSource is resettable</returns>

		public bool IsResettable
		{
			get { return _source.IsResettable; }
		}

		private String[] GetNextValidRecord()
		{
			String[] result = null;
			
			// Search for a valid record to the end of the CSV file
			result = GetNoCommentNoWhitespace();
			
			// If haven't found a valid record and at the end of the
			// file and looping, search from the beginning of the file
			if(result == null && _atEof && Looping)
			{
				Reset();
				result = GetNoCommentNoWhitespace();
			}

			return result;
		}

		private String[] GetNoCommentNoWhitespace() 
		{
			String[] result = null;
			// This loop serves to filter out commented lines and
			//lines that contain only whitespace
			while(result == null && !_atEof)
			{
				SkipCommentedLines();
				result = GetNewValues();
			}
			return result;
		}
		
		private String[] GetNewValues()
		{
			_values.Clear();
			bool doConsume = true;
			
			while(true)
			{
				String value = MatchValue();

				if(AtComma(doConsume))
				{
					AddNonFinalValue(value);
					continue;
				}
				else if(AtNewline(doConsume) || AtEOF(doConsume))
				{
					AddFinalValue(value);
					break;
				}
				else
				{
					throw UnexpectedCharacterException((char)_source.Read());
				}
			}
			
			// All values empty means that this line was just whitespace
			return (_values.Count == 0) ? null : _values.ToArray() ;
		}
		
		private void AddNonFinalValue(String value)
		{
			// Represent empty values as empty strings
			value = (value == null) ? "" : value;
			_values.Add(value);
		}
		
		private void AddFinalValue(String value)
		{
			// Add this value only if it is nonempty or if it is the
			// last value of a nonempty record.
			if(value != null)
			{
				_values.Add(value);
			}
			else
			{
				if(_values.Count != 0)
				{
					_values.Add("");
				}
			}
		}
		
		private bool AtNewline(bool doConsume) 
		{
			return AtWinNewline(doConsume) || AtChar('\n', doConsume) || AtChar('\r', doConsume);
		}
		
		private bool AtWinNewline(bool doConsume)
		{
			MarkReader(2, doConsume);
			
			char firstChar = (char)_source.Read();
			char secondChar = (char)_source.Read();
			bool result = (firstChar == '\r' && secondChar == '\n');
			
			ResetReader(doConsume, result);
			return result;
		}
		
		private bool AtChar(char character, bool doConsume) 
		{
			MarkReader(1, doConsume);
			
			char firstChar = (char)_source.Read();
			bool result = (firstChar == character);
			
			ResetReader(doConsume, result);
			return result;
		}

		private void ResetReader(bool doConsume, bool result) 
		{
			// Reset the source unless in consuming mode and the 
			// matched character was what was expected
			if(!(doConsume && result))
			{
				_source.ResetToMark();
			}
		}

		private void MarkReader(int markLimit, bool doConsume) 
		{
			_source.Mark(markLimit);
		}
		
		private bool AtEOF(bool doConsume) 
		{
			MarkReader(1, doConsume);
			
			int value = _source.Read();
			_atEof = (value == -1);
			
			ResetReader(doConsume, _atEof);
			return _atEof;
		}
		
		private bool AtComma(bool doConsume)
		{
			return AtChar(',', doConsume);
		}
		
		private String MatchValue()
		{
			ConsumeWhiteSpace();
			
			String value = MatchQuotedValue();
			if(value == null)
			{
				value = MatchUnquotedValue();
			}
			
			ConsumeWhiteSpace();
			return value;
		}
		
		private String MatchQuotedValue()
		{
			// Enclosing quotes and quotes used to escape other quotes
			// are discarded
			
			bool doConsume = true;
			if(!AtChar('"', doConsume))
			{
				// This isn't a quoted value
				return null;
			}

			StringBuilder value = new StringBuilder();
			while(true)
			{
				char currentChar = (char)_source.Read();

				if(currentChar == '"' && !AtChar('"', doConsume))
				{
					// Single quote ends the value
					break;
				}

				value.Append(currentChar);
			}
			
			return value.ToString();
		}
		
		private String MatchUnquotedValue()
		{
			bool doConsume = false;
			StringBuilder value = new StringBuilder();
			int trailingSpaces = 0;
			
			while(true)
			{
				// Break on NewLine or comma without consuming
				if(AtNewline(doConsume) || AtEOF(doConsume) || AtComma(doConsume))
				{
					break;
				}
				
				// Unquoted values cannot contain quotes
				if(AtChar('"', doConsume))
				{
                    if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
                    {
                        Log.Debug(".matchUnquotedValue matched unexpected double-quote while matching " + value);
                        Log.Debug(".matchUnquotedValue values==" + _values);
                    }
				    throw UnexpectedCharacterException('"');
				}
				
				char currentChar = (char)_source.Read();
				
				// Update the count of trailing spaces
				trailingSpaces = (IsWhiteSpace(currentChar)) ?
						trailingSpaces + 1 : 0;
				
				value.Append(currentChar);
			}
			
			// Remove the trailing spaces
			int end = value.Length;
			value.Remove(end - trailingSpaces, trailingSpaces);
			
			// An empty string means that this value was just whitespace, 
			// so nothing was matched
			return value.Length == 0 ? null : value.ToString();
		}
		
		private void ConsumeWhiteSpace()
		{
			while(true)
			{	
				_source.Mark(1);
				char currentChar = (char)_source.Read();

				if(!IsWhiteSpace(currentChar))
				{
					_source.ResetToMark();
					break;
				}
			}
		}
		
		private bool IsWhiteSpace(char currentChar)
		{
			return currentChar == ' ' || currentChar == '\t';
		}
		
		private EPException UnexpectedCharacterException(char unexpected)
		{
			return new EPException("Encountered unexpected character " + unexpected);
		}
		
		private void SkipCommentedLines()
		{
			bool doConsume = false;
			while(true)
			{
				if(_atEof && Looping)
				{
					Reset();
				}
				if(AtChar('#', doConsume))
				{
					ConsumeLine();
				}
				else
				{
					break;
				}
			}
		}
		
		private void ConsumeLine()
		{
			bool doConsume = true;
			while(!AtEOF(doConsume) && !AtNewline(doConsume))
			{
				// Discard input
				_source.Read();
			}
		}
	}
}

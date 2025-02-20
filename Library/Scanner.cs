using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRcsharp.Library
{
    public class Scanner
    {

        private string _word;
        private int _currentIndex;

        public Scanner(string word) 
        { 
            _word = word;
            _currentIndex = 0;
        }

        public bool IsAtEnd
        {
            get { return _currentIndex >= _word.Length; }
        }

        public string ScanCharacters(char[] characters)
        {
            var result = string.Empty;
            while(characters.Contains(_word[_currentIndex]))
            {
                result += _word[_currentIndex];
                _currentIndex++;
            }

            return result;
        }

        public void ScanUpToCharacters(char[] characters)
        {
            while (!characters.Contains(_word[_currentIndex]))
            {
                _currentIndex++;
            }
        }
    }
}

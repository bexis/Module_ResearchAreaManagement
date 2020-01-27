using System;
using System.Collections.Generic;


namespace BExIS.Modules.PMM.UI.Helper
{
  
      public class NaturalSorter : IComparer<string>
        {
            //use a buffer for performance since we expect
            //the Compare method to be called a lot
            private char[] _splitBuffer = new char[256];

            public int Compare(string x, string y)
            {
                //first split each string into segments
                //of non-numbers and numbers
                IList<string> a = SplitByNumbers(x);
                IList<string> b = SplitByNumbers(y);

                int aInt, bInt;
                int numToCompare = (a.Count < b.Count) ? a.Count : b.Count;
                for (int i = 0; i < numToCompare; i++)
                {
                    if (a[i].Equals(b[i]))
                        continue;

                    bool aIsNumber = Int32.TryParse(a[i], out aInt);
                    bool bIsNumber = Int32.TryParse(b[i], out bInt);
                    bool bothNumbers = aIsNumber && bIsNumber;
                    bool bothNotNumbers = !aIsNumber && !bIsNumber;
                    //do an integer compare
                    if (bothNumbers) return aInt.CompareTo(bInt);
                    //do a string compare
                    if (bothNotNumbers) return a[i].CompareTo(b[i]);
                    //only one is a number, which are
                    //by definition less than non-numbers
                    if (aIsNumber) return -1;
                    return 1;
                }
                //only get here if one string is empty
                return a.Count.CompareTo(b.Count);
            }

            private IList<string> SplitByNumbers(string val)
            {
                System.Diagnostics.Debug.Assert(val.Length <= 256);
                List<string> list = new List<string>();
                int current = 0;
                int dest = 0;
                while (current < val.Length)
                {
                    //accumulate non-numbers
                    while (current < val.Length &&
                           !char.IsDigit(val[current]))
                    {
                        _splitBuffer[dest++] = val[current++];
                    }
                    if (dest > 0)
                    {
                        list.Add(new string(_splitBuffer, 0, dest));
                        dest = 0;
                    }
                    //accumulate numbers
                    while (current < val.Length &&
                           char.IsDigit(val[current]))
                    {
                        _splitBuffer[dest++] = val[current++];
                    }
                    if (dest > 0)
                    {
                        list.Add(new string(_splitBuffer, 0, dest));
                        dest = 0;
                    }
                }
                return list;
            }
        }
  
}
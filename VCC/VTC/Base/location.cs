using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VTC
{


    /// <summary>
    ///   Keeps track of the location in the program
    /// </summary>
    ///
    /// <remarks>
    ///   This uses a compact representation and a couple of auxiliary
    ///   structures to keep track of tokens to (file,line and column) 
    ///   mappings. The usage of the bits is:
    ///   
    ///     - 16 bits for "checkpoint" which is a mixed concept of
    ///       file and "line segment"
    ///     - 8 bits for line delta (offset) from the line segment
    ///     - 8 bits for column number.
    ///
    ///   http://lists.ximian.com/pipermail/mono-devel-list/2004-December/009508.html
    /// </remarks>
    public class Location : IEquatable<Location>
    {
        public long Pos { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public string FileName { get { return Path.GetFileName(FullPath); } }
        public string FullPath { get; set; }
        public Location(string file, int row, int col, long pos)
        {
            FullPath = file;
            Row = row;
            Column = col;
            Pos = pos;
        }
        public Location(int row, int col, long pos)
        {
            FullPath = "default";
            Row = row;
            Column = col;
            Pos = pos;
        }
        public bool IsNull { get { return Column == -1 || Row == -1; } }

        public static Location Null = new Location(-1, -1,-1);
        #region IEquatable<Location> Members

        public bool Equals(Location other)
        {
            return this.Row == other.Row && this.Column == other.Column && FullPath == other.FullPath;
        }

        #endregion
        public override string ToString()
        {
            return string.Format("({0},{1},{2})", Row,Column,Pos);
        }
    }


}

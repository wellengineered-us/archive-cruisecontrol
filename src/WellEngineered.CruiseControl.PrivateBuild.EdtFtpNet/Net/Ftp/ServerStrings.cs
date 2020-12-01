// edtFTPnet
// 
// Copyright (C) 2007 Enterprise Distributed Technologies Ltd
// 
// www.enterprisedt.com
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
// 
// Bug fixes, suggestions and comments should posted on 
// http://www.enterprisedt.com/forums/index.php
// 
// Change Log:
// 
// $Log: ServerStrings.cs,v $
// Revision 1.2  2007-05-23 00:30:49  hans
// Changed so that ServerString inherits from CollectionBase instead of StringCollection in order to allow notification of changes to the collection.  This was done in order to support the PropertyChanged event in FTPConnection.
//
// Revision 1.1  2007-01-30 04:39:15  bruceb
// new files for parsing server replies
//
//

using System;
using System.Collections;
using System.ComponentModel;

namespace WellEngineered.CruiseControl.PrivateBuild.EdtFtpNet.Net.Ftp
{

    /// <summary>  
    /// Manages strings that match various FTP server replies for
    /// various situations. The strings are not exact copies of server
    /// replies, but rather fragments that match server replies (so that
    /// as many servers as possible can be supported). All fragments are
    /// managed internally in upper case to make matching faster.
    /// </summary>
    /// <author>Bruce Blackshaw</author>
    /// <version>$Revision: 1.2 $</version>
    public class ServerStrings : CollectionBase
    {
        private PropertyChangedEventHandler propertyChangeHandler;

        /// <summary>
        /// Add the string to the collection.
        /// </summary>
        /// <param name="str">String to add.</param>
        public void Add(string str)
        {
            this.List.Add(str);
            this.OnMemberChanged();
        }

        /// <summary>
        /// Add all the strings to the collection.
        /// </summary>
        /// <param name="strs">Strings to add.</param>
        public void AddRange(string[] strs)
        {
            foreach (string str in strs)
                this.List.Add(str);
            this.OnMemberChanged();
        }

        /// <summary>
        /// Returns <c>true</c> if the given string is already in the collection.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public bool Contains(string str)
        {
            return this.List.Contains(str);
        }

        /// <summary>
        /// Copy all the strings in the collection to the <c>array</c> starting at the given index.
        /// </summary>
        /// <param name="array">Array to which to add strings.</param>
        /// <param name="index">Index at which to start adding.</param>
        public void CopyTo(string[] array, int index)
        {
            this.List.CopyTo(array, index);
        }

        /// <summary>
        /// Returns the index of the given string or <c>-1</c> if it's not in the collection.
        /// </summary>
        /// <param name="str">String to look for.</param>
        /// <returns>Index of the given string or <c>-1</c> if it's not in the collection.</returns>
        public int IndexOf(string str)
        {
            return this.List.IndexOf(str);
        }

        /// <summary>
        /// Inserts the given string into the collection at the given index.
        /// </summary>
        /// <param name="index">Index at which to add the string.</param>
        /// <param name="str">String to insert.</param>
        public void Insert(int index, string str)
        {
            this.List.Insert(index, str);
            this.OnMemberChanged();
        }

        /// <summary>
        /// Gets a reference to a string at the given index.
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>String at the given index.</returns>
        public string this[int index]
        {
            get
            {
                return (string)this.List[index];
            }
            set
            {
                if ((string)this.List[index] != value)
                {
                    this.List[index] = value;
                    this.OnMemberChanged();
                }
            }
        }

        /// <summary>
        /// Remove the given string from the collection.
        /// </summary>
        /// <param name="str">String to remove.</param>
        public void Remove(string str)
        {
            this.List.Remove(str);
            this.OnMemberChanged();
        }

        /// <summary>
        /// Returns true if any fragment is found in the supplied string
        /// </summary>
        /// <param name="reply">server reply to test for matches</param>
        /// <returns>true for a match, false otherwise</returns>
        public bool Matches(string reply) 
        {
            string upper = reply.ToUpper();
            for (int i = 0; i < this.Count; i++) 
            {
                string msg = this[i];
                if (upper.IndexOf(msg.ToUpper()) >= 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Called when a property is changed.
        /// </summary>
        private void OnMemberChanged()
        {
            if (this.propertyChangeHandler != null)
                this.propertyChangeHandler(this, new PropertyChangedEventArgs(null));
        }

        /// <summary>
        /// Called when a property is changed.
        /// </summary>
        internal PropertyChangedEventHandler PropertyChangeHandler
        {
            get { return this.propertyChangeHandler; }
            set { this.propertyChangeHandler = value; }
        }
    }
}

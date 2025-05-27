using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySoft.model
{
    /// <summary>
    /// allow to know with resource is modify.
    /// </summary>
    class FileCompare : System.Collections.Generic.IEqualityComparer<System.IO.FileInfo>
    {
        public FileCompare() { }

        public bool Equals(System.IO.FileInfo file1, System.IO.FileInfo file2)
        {
            return (file1.Name == file2.Name &&
                    file1.Length == file2.Length);
        }

        /// <summary>
        /// retrieve the hashe of the resource
        /// </summary>
        /// <param name="fi"></param>
        /// <returns></returns>
        public int GetHashCode(System.IO.FileInfo file)
        {
            string Hash = $"{file.Name}{file.Length}";
            return Hash.GetHashCode();
        }
    }
}

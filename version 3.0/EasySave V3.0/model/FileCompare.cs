namespace EasySaveApp.model
{
    //Class used for differential backup
    //It allows the comparison of the hashes of the files to know which file has been modified.
    class FileCompare : System.Collections.Generic.IEqualityComparer<System.IO.FileInfo>
    {
        public FileCompare() { }

        public bool Equals(System.IO.FileInfo File1, System.IO.FileInfo File2)
        {
            return (File1.Name == File2.Name &&
                    File1.Length == File2.Length);
        }


        public int GetHashCode(System.IO.FileInfo File) // Function to retrieve the hash of files
        {
            string s = $"{File.Name}{File.Length}";
            return s.GetHashCode(); // Return a hash that reflects the comparison criteria.  
        }
    }
}
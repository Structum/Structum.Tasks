using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Structum.Tasks.MSBuild
{
    /// <summary>
    /// FileVersioner Class
    /// </summary>
    /// <remarks>
    /// Helps creating the new filename using
    /// the file's hash.
    /// </remarks>
    internal class FileVersioner
    {
        #region Members
        /// <summary>
        /// A Stream to the file to versionify.
        /// </summary>
        private FileStream _fs;

        /// <summary>
        /// Hash algorith to use.
        /// </summary>
        private HashAlgorithm _alg;

        /// <summary>
        /// File to versionify.
        /// </summary>
        private string _file;
        #endregion

        #region Ctor
        /// <summary>
        /// Default constructor, restricted.
        /// </summary>
        private FileVersioner()
        {
        }

        /// <summary>
        /// Constructor which accepts a filename
        /// </summary>
        /// <param name="filename">Filename</param>
        public FileVersioner(string filename) : this()
        {
            this._file = filename;
            this._fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
        }

        /// <summary>
        /// Creates a new FileVersioner
        /// </summary>
        /// <param name="filename">File to versionify</param>
        /// <returns><c>FileVersioner</c></returns>
        public static FileVersioner For(string filename)
        {
            return new FileVersioner(filename);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Initializes a new object of the provided
        /// HashAlgorith subclass to use in the hasher method.
        /// </summary>
        /// <param name="alg">Algorithm Type</param>
        /// <returns><c>FileVersioner</c></returns>
        public FileVersioner WithAlgorithm(Type algType)
        {
            this._alg = algType.GetConstructor(Type.EmptyTypes).Invoke(null) as HashAlgorithm;
            return this;
        }

        /// <summary>
        /// Generates the new tagged filename and file.
        /// </summary>
        public string GenerateTaggedFile(string output)
        {
            string hash = this.ComputeHash();
            this._fs.Close();

            string newFilename = output + @"\" + this.GetTaggedFilename(hash);

            // Let's write the file
            File.WriteAllBytes(newFilename, File.ReadAllBytes(this._file));

            return newFilename;
        }
        #endregion

        #region Support methods
        /// <summary>
        /// Computes the file hash.
        /// </summary>
        /// <returns>Hash string</returns>
        private string ComputeHash()
        {
            StringBuilder hash = new StringBuilder();
            byte[] hashData = this._alg.ComputeHash(this._fs);
            
            foreach (byte b in hashData) {
                hash.Append(b.ToString("x2"));
            }

            return hash.ToString();
        }

        /// <summary>
        /// Gets the filename with the hash tag attached.
        /// </summary>
        /// <param name="hash">hash to use</param>
        /// <returns><c>string</c></returns>
        private string GetTaggedFilename(string hash)
        {
            FileInfo fi = new FileInfo(this._file);
            string taggedFileNameTemplate = "{0}.{1}.min.js";

            return String.Format(taggedFileNameTemplate, Path.GetFileNameWithoutExtension(this._file).Replace(".min", String.Empty), hash);
        }
        #endregion
    }
}

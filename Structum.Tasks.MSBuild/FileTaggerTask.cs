using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace Structum.Tasks.MSBuild
{
    /// <summary>
    /// FileTaggerTask
    /// </summary>
    /// <remarks>
    /// Represents a MSBuild task to process css and
    /// javascript files in order generate a proper etag-like
    /// thing, in order to versionify the files.
    /// </remarks>
    public class FileTaggerTask : Task
    {
        #region Members
        private ITaskItem[] _cssItems;

        /// <summary>
        /// Css objects to process.
        /// </summary>
        /// <value>Gets/Sets the _cssItems member</value>
        public ITaskItem[] CssItems
        {
            get
            {
                return this._cssItems;
            }
            set
            {
                this._cssItems = value;
            }
        }

        private ITaskItem[] _jsItems;

        /// <summary>
        /// Javascript objects to process.
        /// </summary>
        /// <value>Gets/Sets the _jsItems member</value>
        public ITaskItem[] JsItems
        {
            get
            {
                return this._jsItems;
            }
            set
            {
                this._jsItems = value;
            }
        }

        private string _outputDir;

        /// <summary>
        /// Output directory for the files.
        /// </summary>
        /// <value>Gets/Sets the _outputDir member</value>
        public string OutputDirectory
        {
            get
            {
                return this._outputDir;
            }
            set
            {
                this._outputDir = value;
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Task entry point
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            bool taskSuccess;

            IList<string> cssFiles = this._cssItems.Select(f => f.ItemSpec).ToList();
            IList<string> jsFiles = this._jsItems.Select(f => f.ItemSpec).ToList();

            try {
                Log.LogMessage("*** Begining File versioning task ***");
                
                // Clean the output directory
                this.CleanupOutputDirectory();

                // Process files.
                this.ProcessCssFiles(cssFiles);
                this.ProcessJsFiles(jsFiles);
                
                // Delete all js and css content from Resource/
                this.RemoveSourceFiles();

                taskSuccess = true;
                Log.LogMessage("*** File versioning task ended nicely :) ***");
            } catch (Exception ex) {
                Log.LogError("Error ocurred when processing the input files, ex: " + ex.Message);
                taskSuccess = false;
                Log.LogMessage("*** File versioning task wen't just bad :( ***");
            }

            return taskSuccess;
        }
        #endregion

        #region Support methods
        /// <summary>
        /// Processes the provided css files.
        /// </summary>
        /// <param name="files">Collection of css files</param>
        private void ProcessCssFiles(IList<string> files)
        {
            foreach (string file in files) {
                string newFilename = FileVersioner
                                        .For(file)
                                        .WithAlgorithm(typeof(System.Security.Cryptography.MD5CryptoServiceProvider))
                                        .GenerateTaggedFile(output: this._outputDir);
                Log.LogMessage(newFilename);
            }
        }

        /// <summary>
        /// Processes the provided js files.
        /// </summary>
        /// <param name="files">Collectiin of javascript files</param>
        private void ProcessJsFiles(IList<string> files)
        {
            foreach (string file in files) {
                string newFilename = FileVersioner
                                        .For(file)
                                        .WithAlgorithm(typeof(System.Security.Cryptography.MD5CryptoServiceProvider))
                                        .GenerateTaggedFile(output: this._outputDir);
                Log.LogMessage(newFilename);
            }
        }

        /// <summary>
        /// Cleans up the output directory.
        /// </summary>
        private void CleanupOutputDirectory()
        {
            var sourceFiles = new System.IO.DirectoryInfo(this._outputDir).GetFiles();

            if (sourceFiles.Length > 0) {
                foreach (var file in sourceFiles) {
                    System.IO.File.Delete(file.FullName);
                }
            }
        }

        /// <summary>
        /// Cleans up the source files.
        /// </summary>
        private void RemoveSourceFiles()
        {
            // Delete source css files.
            foreach (var file in this._cssItems.Select(css => css.ItemSpec)) {
                if (System.IO.File.Exists(file)) {
                    System.IO.File.Delete(file);
                }
            }

            // Delete source js files.
            foreach (var file in this._jsItems.Select(js => js.ItemSpec)) {
                if (System.IO.File.Exists(file)) {
                    System.IO.File.Delete(file);
                }
            }
        }
        #endregion
    }
}

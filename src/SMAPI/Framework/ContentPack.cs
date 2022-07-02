using System;
using System.IO;
using StardewModdingAPI.Toolkit.Serialization;
using StardewModdingAPI.Toolkit.Utilities;
using StardewModdingAPI.Toolkit.Utilities.PathLookups;

namespace StardewModdingAPI.Framework
{
    /// <summary>Manages access to a content pack's metadata and files.</summary>
    internal class ContentPack : IContentPack
    {
        /*********
        ** Fields
        *********/
        /// <summary>Encapsulates SMAPI's JSON file parsing.</summary>
        private readonly JsonHelper JsonHelper;

        /// <summary>A lookup for files within the <see cref="DirectoryPath"/>.</summary>
        private readonly IFileLookup FileLookup;


        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public string DirectoryPath { get; }

        /// <inheritdoc />
        public IManifest Manifest { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="directoryPath">The full path to the content pack's folder.</param>
        /// <param name="manifest">The content pack's manifest.</param>
        /// <param name="jsonHelper">Encapsulates SMAPI's JSON file parsing.</param>
        /// <param name="fileLookup">A lookup for files within the <paramref name="directoryPath"/>.</param>
        public ContentPack(string directoryPath, IManifest manifest, JsonHelper jsonHelper, IFileLookup fileLookup)
        {
            this.DirectoryPath = directoryPath;
            this.Manifest = manifest;
            this.JsonHelper = jsonHelper;
            this.FileLookup = fileLookup;
        }

        /// <inheritdoc />
        public bool HasFile(string path)
        {
            path = PathUtilities.NormalizePath(path);

            return this.GetFile(path).Exists;
        }

        /// <inheritdoc />
        public TModel? ReadJsonFile<TModel>(string path) where TModel : class
        {
            path = PathUtilities.NormalizePath(path);

            FileInfo file = this.GetFile(path);
            return file.Exists && this.JsonHelper.ReadJsonFileIfExists(file.FullName, out TModel? model)
                ? model
                : null;
        }

        /// <inheritdoc />
        public void WriteJsonFile<TModel>(string path, TModel data) where TModel : class
        {
            path = PathUtilities.NormalizePath(path);

            FileInfo file = this.GetFile(path);
            bool didExist = file.Exists;

            this.JsonHelper.WriteJsonFile(file.FullName, data);

            if (!didExist)
            {
                this.FileLookup.Add(
                    Path.GetRelativePath(this.DirectoryPath, file.FullName)
                );
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the underlying file info.</summary>
        /// <param name="relativePath">The normalized file path relative to the content pack directory.</param>
        private FileInfo GetFile(string relativePath)
        {
            if (!PathUtilities.IsSafeRelativePath(relativePath))
                throw new InvalidOperationException($"You must call {nameof(IContentPack)} methods with a relative path.");

            return this.FileLookup.GetFile(relativePath);
        }
    }
}

using UnixStyleContainerFileFramework;
using VirtualFileSystem.Exceptions;

namespace VirtualFileSystem
{
        /// <summary>
        ///         represents a virtual disk in the virtual file system.
        /// </summary>
        public class VirtualDisk
        {
                /// <summary>
                ///         the first block index of the root directory.
                /// </summary>
                private const uint ROOT_DIRECTORY_FILE_NUMBER = 0;

                /// <summary>
                ///         the root directory's name.
                /// </summary>
                private const string ROOT_DIRECTORY_NAME = "root";

                /// <summary>
                ///         the underlying storage implementation.
                /// </summary>
                private readonly ContainerFile _containerFile;


                /// <summary>
                ///         constructor.
                /// </summary>
                /// <param name="containerFile">the container file instance the virtual disk is operating on.</param>
                private VirtualDisk(ContainerFile containerFile)
                {
                        _containerFile = containerFile;
                }

                /// <summary>
                ///         returns the root directory of the virtual disk.
                /// </summary>
                public VirtualDirectory RootDirectory
                {
                        get { return new VirtualDirectory(ROOT_DIRECTORY_FILE_NUMBER, ROOT_DIRECTORY_NAME, null, _containerFile); }
                }

                /// <summary>
                ///         returns how many more files the virtual disk could hold.
                /// </summary>
                public uint FreeFiles
                {
                        get { return _containerFile.FreeFiles; }
                }

                /// <summary>
                ///         returns the amount of free space of the virtual disk in bytes.
                /// </summary>
                public long FreeSpace
                {
                        get { return _containerFile.FreeSpace; }
                }

                /// <summary>
                ///         returns the total amount of space of the virtual disk in bytes.
                /// </summary>
                public long TotalSpace
                {
                        get { return _containerFile.TotalSpace; }
                }

                /// <summary>
                ///         creates a new virtual disk.
                /// </summary>
                /// <param name="fileName">the file name of the new virtual disk (including file path).</param>
                /// <param name="maximumSizeInBytes">the maximum size of the virtual disk in bytes.</param>
                /// <param name="maximumFiles">the maximum amount of file the virtual disk should be able to store.</param>
                /// <returns>the created virtual disk instance.</returns>
                public static VirtualDisk Create(string fileName, long maximumSizeInBytes, uint maximumFiles)
                {
                        ContainerFile basicVirtualDisk = ContainerFile.Create(fileName, maximumSizeInBytes, maximumFiles);

                        if (basicVirtualDisk.CreateFile(1) != ROOT_DIRECTORY_FILE_NUMBER)
                        {
                                throw new RootLocationException(string.Format("the root directory must have fileNumber = {0}", ROOT_DIRECTORY_FILE_NUMBER));
                        }

                        return new VirtualDisk(basicVirtualDisk);
                }

                /// <summary>
                ///         opens an existing virtual disk.
                /// </summary>
                /// <param name="fileName">the file name of the new virtual disk (including file path).</param>
                /// <returns>the open virtual disk instance.</returns>
                public static VirtualDisk Open(string fileName)
                {
                        return new VirtualDisk(ContainerFile.Open(fileName));
                }

                /// <summary>
                ///         closes the virtual disk and writes all changes back to the host file system.
                /// </summary>
                public void Close()
                {
                        _containerFile.Close();
                }

                /// <summary>
                ///         deletes the virtual disk.
                /// </summary>
                public void Delete()
                {
                        _containerFile.Delete();
                }
        }
}
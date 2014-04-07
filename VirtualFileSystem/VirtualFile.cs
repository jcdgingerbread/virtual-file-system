using System;
using System.IO;
using System.Linq;
using UnixStyleContainerFileFramework;
using VirtualFileSystem.Exceptions;
using FileStream = UnixStyleContainerFileFramework.FileStream;

namespace VirtualFileSystem
{
        /// <summary>
        ///         represents a file in the virtual file system.
        /// </summary>
        public class VirtualFile : VirtualItem
        {
                /// <summary>
                ///         constructor.
                /// </summary>
                /// <param name="firstBlockIndex">the block index the file begins at in the parent container file.</param>
                /// <param name="name">the name of this virtual item (file name or directory name).</param>
                /// <param name="parentDirectory">an instance of the parent directory.</param>
                /// <param name="containerFile">an instance of the parent container file.</param>
                internal VirtualFile(uint firstBlockIndex, string name, VirtualDirectory parentDirectory, ContainerFile containerFile)
                        : base(firstBlockIndex, name, parentDirectory, containerFile)
                {
                }

                /// <summary>
                ///         imports a file from the host file system as content into this file.
                /// </summary>
                /// <param name="fileToImport">the file name (including file path) of the file to import from the host file system</param>
                internal void ImportContent(string fileToImport)
                {
                        const long IMPORT_CHUNK_SIZE = 1024 * 1024;

                        using (var reader = new BinaryReader(File.Open(fileToImport, FileMode.Open)))
                        {
                                long newFileSize = reader.BaseStream.Length;

                                ContainerFile.ChangeFileSize(FileNumber, (uint) (newFileSize + 8));

                                FileStream stream = GetFileStream();

                                stream.Write(BitConverter.GetBytes(newFileSize));

                                while (reader.BaseStream.Position < reader.BaseStream.Length)
                                {
                                        var dataLeft = (ulong) (reader.BaseStream.Length - reader.BaseStream.Position);
                                        ulong dataToImport = dataLeft > IMPORT_CHUNK_SIZE ? IMPORT_CHUNK_SIZE : dataLeft;

                                        var buffer = new byte[dataToImport];
                                        reader.BaseStream.Read(buffer, 0, buffer.Length);
                                        stream.Write(buffer);
                                }
                        }
                }

                /// <summary>
                ///         exports the contents of this file to the host file system.
                /// </summary>
                /// <param name="pathName">the name of the exported file (including file path).</param>
                public override void Export(string pathName)
                {
                        const long EXPORT_CHUNK_SIZE = 1024 * 1024;

                        using (var writer = new BinaryWriter(File.Open(pathName, FileMode.CreateNew)))
                        {
                                FileStream stream = GetFileStream();

                                long fileSize = BitConverter.ToInt64(stream.Read(8), 0);

                                long dataLeftToExport = fileSize;

                                while (dataLeftToExport > 0)
                                {
                                        long amountOfDataToExport = dataLeftToExport > EXPORT_CHUNK_SIZE ? EXPORT_CHUNK_SIZE : dataLeftToExport;

                                        writer.Write(stream.Read(amountOfDataToExport));

                                        dataLeftToExport -= amountOfDataToExport;
                                }
                        }
                }

                /// <summary>
                ///         copies this file into the target directory with a given name.
                /// </summary>
                /// <param name="targetDirectory">the target directory this file should be copied to</param>
                /// <param name="newName">the new name the copied file should take after copying.</param>
                /// <returns>the created copy.</returns>
                public override VirtualItem Copy(VirtualDirectory targetDirectory, string newName)
                {
                        const int COPY_CHUNK_SIZE = 2048;

                        if (targetDirectory == null)
                        {
                                throw new ArgumentException("targetDirectory must not be null");
                        }

                        if (targetDirectory.ReadAllItems().Any(item => item.Name.Equals(newName)))
                        {
                                throw new ItemNameInUseException(newName);
                        }

                        VirtualFile fileCopy = targetDirectory.AddFile(newName);

                        FileStream fromStream = GetFileStream();
                        FileStream toStream = fileCopy.GetFileStream();

                        ContainerFile.ChangeFileSize(fileCopy.FileNumber, fromStream.Length);

                        long fromStreamLength = fromStream.Length;

                        while (fromStream.Position < fromStreamLength)
                        {
                                long remainingData = fromStreamLength - fromStream.Position;

                                long amountToCopy = COPY_CHUNK_SIZE < remainingData ? COPY_CHUNK_SIZE : remainingData;

                                toStream.Write(fromStream.Read(amountToCopy));
                        }

                        return fileCopy;
                }
        }
}
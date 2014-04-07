using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnixStyleContainerFileFramework;
using VirtualFileSystem.Exceptions;
using VirtualFileSystem.Utils;
using FileStream = UnixStyleContainerFileFramework.FileStream;

namespace VirtualFileSystem
{
        /// <summary>
        ///         represents a directory in the virtual file system.
        /// </summary>
        public class VirtualDirectory : VirtualItem
        {
                /// <summary>
                ///         constructor.
                /// </summary>
                /// <param name="fileNumber">the block index the file begins at in the parent container file.</param>
                /// <param name="name">the name of this virtual item (file name or directory name).</param>
                /// <param name="parentDirectory">an instance of the parent directory.</param>
                /// <param name="containerFile">an instance of the parent container file.</param>
                internal VirtualDirectory(uint fileNumber, string name, VirtualDirectory parentDirectory,
                                          ContainerFile containerFile)
                        : base(fileNumber, name, parentDirectory, containerFile)
                {
                }

                /// <summary>
                ///         returns the directory separator character used in the GetRelativeItem function.
                /// </summary>
                public static char DirectorySeparator
                {
                        get { return '/'; }
                }

                /// <summary>
                ///         adds a new directory to this directory.
                /// </summary>
                /// <param name="directoryName">the name of the new directory to be created.</param>
                /// <returns>the created directory.</returns>
                public VirtualDirectory AddDirectory(string directoryName)
                {
                        if (!ValidName(directoryName))
                        {
                                throw new ArgumentException("directoryName invalid");
                        }

                        if (ReadAllItems().Any(item => item.Name.Equals(directoryName)))
                        {
                                throw new ItemNameInUseException(directoryName);
                        }

                        var newDirectory = new VirtualDirectory(CreateNewFile(), directoryName, this, ContainerFile);
                        AddItem(newDirectory);

                        return newDirectory;
                }

                /// <summary>
                ///         adds a new file to this directory.
                /// </summary>
                /// <param name="fileName">the name of the file to be created.</param>
                /// <returns>the created file.</returns>
                public VirtualFile AddFile(string fileName)
                {
                        if (!ValidName(fileName))
                        {
                                throw new ArgumentException("fileName invalid");
                        }

                        if (ReadAllItems().Any(item => item.Name.Equals(fileName)))
                        {
                                throw new ItemNameInUseException(fileName);
                        }

                        var newFile = new VirtualFile(CreateNewFile(), fileName, this, ContainerFile);
                        AddItem(newFile);

                        return newFile;
                }

                /// <summary>
                ///         creates a new file.
                /// </summary>
                /// <returns>the file number of the newly created file.</returns>
                private uint CreateNewFile()
                {
                        uint fileNumber = ContainerFile.CreateFile(8);

                        FileStream fileStream = ContainerFile.OpenFile(fileNumber);

                        fileStream.Write(BitConverter.GetBytes((long) 0));

                        return fileNumber;
                }

                /// <summary>
                ///         adds an item to this directory.
                /// </summary>
                /// <param name="itemToAdd">the item to add.</param>
                internal void AddItem(VirtualItem itemToAdd)
                {
                        FileStream fileStream = GetFileStream();

                        long currentSize = BitConverter.ToInt64(fileStream.Read(8), 0);

                        var dataWriter = new DataWriter();
                        SerializeItem(itemToAdd, dataWriter);
                        byte[] data = dataWriter.GetData();

                        long newSize = currentSize + data.Length;

                        if (fileStream.Length < newSize + 8)
                        {
                                ContainerFile.ChangeFileSize(FileNumber, (uint) (newSize + 8));
                        }

                        fileStream = GetFileStream();
                        fileStream.Write(BitConverter.GetBytes(newSize));
                        fileStream.Seek(currentSize + 8, SeekOrigin.Begin);
                        fileStream.Write(data);
                }

                /// <summary>
                ///         reads all items stored in this directory.
                /// </summary>
                /// <returns>an enumeration of all items in this directory.</returns>
                public IEnumerable<VirtualItem> ReadAllItems()
                {
                        FileStream fileStream = GetFileStream();

                        long currentSize = BitConverter.ToInt64(fileStream.Read(8), 0);

                        byte[] allData = fileStream.Read(currentSize);
                        var dataReader = new DataReader(allData);

                        var allItems = new List<VirtualItem>();

                        while (dataReader.Position < dataReader.Length)
                        {
                                allItems.Add(DeserializeDirectoryItem(dataReader));
                        }

                        return allItems;
                }

                /// <summary>
                ///         deserializes one directory item entry from the stream.
                /// </summary>
                /// <param name="dataReader">the stream to deserialize from.</param>
                /// <returns>the deserialized item.</returns>
                private VirtualItem DeserializeDirectoryItem(DataReader dataReader)
                {
                        uint itemFirstBlockIndex = dataReader.ReadUInt32();
                        var itemType = (ItemType) dataReader.ReadByte();
                        string itemName = dataReader.ReadString();

                        if (itemType == ItemType.Directory)
                        {
                                return new VirtualDirectory(itemFirstBlockIndex, itemName, this, ContainerFile);
                        }

                        return new VirtualFile(itemFirstBlockIndex, itemName, this, ContainerFile);
                }

                /// <summary>
                ///         serializes a directory item entry.
                /// </summary>
                /// <param name="item">the item to be serialized.</param>
                /// <param name="dataWriter">the stream the item should be serialized to.</param>
                private static void SerializeItem(VirtualItem item, DataWriter dataWriter)
                {
                        dataWriter.WriteUInt32(item.FileNumber);
                        dataWriter.WriteByte((byte) (item is VirtualDirectory ? ItemType.Directory : ItemType.File));
                        dataWriter.WriteString(item.Name);
                }

                /// <summary>
                ///         deletes this directory.
                /// </summary>
                public override void Delete()
                {
                        if (ParentDirectory == null)
                        {
                                throw new RootDeletionException("deleting root might be problematic");
                        }

                        IEnumerable<VirtualItem> items = ReadAllItems();

                        foreach (VirtualItem item in items)
                        {
                                item.Delete();
                        }

                        base.Delete();
                }

                /// <summary>
                ///         exports this directory to the host file system at the given path.
                /// </summary>
                /// <param name="pathName">the name of the exported directory (including path).</param>
                public override void Export(string pathName)
                {
                        Directory.CreateDirectory(pathName);

                        IEnumerable<VirtualItem> items = ReadAllItems().ToArray();

                        IEnumerable<VirtualDirectory> directories = items.OfType<VirtualDirectory>();
                        IEnumerable<VirtualFile> files = items.OfType<VirtualFile>();

                        foreach (VirtualDirectory directory in directories)
                        {
                                directory.Export(pathName + Path.DirectorySeparatorChar + directory.Name);
                        }

                        foreach (VirtualFile file in files)
                        {
                                file.Export(pathName + Path.DirectorySeparatorChar + file.Name);
                        }
                }

                /// <summary>
                ///         /// copies the directory into the target directory with a given name.
                /// </summary>
                /// <param name="targetDirectory">the target directory this directory should be copied to</param>
                /// <param name="newName">the new name the copied directory should take after copying.</param>
                /// <returns>the created copy.</returns>
                public override VirtualItem Copy(VirtualDirectory targetDirectory, string newName)
                {
                        if (!ValidName(newName))
                        {
                                throw new ArgumentException("newName invalid");
                        }

                        return CopyInternal(targetDirectory, newName, new HashSet<uint>());
                }

                private VirtualDirectory CopyInternal(VirtualDirectory targetDirectory, string newName, HashSet<uint> copies)
                {
                        if (copies.Contains(FileNumber)) return null;

                        VirtualItem[] items = ReadAllItems().ToArray();

                        int copyNumber = 0;
                        string baseName = newName;
                        VirtualItem[] targetItems = targetDirectory.ReadAllItems().ToArray();
                        while (targetItems.Any(item => item.Name.Equals(baseName)))
                        {
                                baseName = newName + ++copyNumber;
                        }

                        VirtualDirectory directoryCopy = targetDirectory.AddDirectory(baseName);

                        copies.Add(directoryCopy.FileNumber);

                        foreach (VirtualDirectory directory in items.OfType<VirtualDirectory>())
                        {
                                directory.CopyInternal(directoryCopy, directory.Name, copies);
                        }

                        foreach (VirtualFile file in items.OfType<VirtualFile>())
                        {
                                file.Copy(directoryCopy, file.Name);
                        }

                        return directoryCopy;
                }


                /// <summary>
                ///         deletes an item entry from this directory.
                /// </summary>
                /// <param name="itemToDelete">the item to delete.</param>
                internal void DeleteItem(VirtualItem itemToDelete)
                {
                        RemoveItem(itemToDelete);

                        ContainerFile.DeleteFile(itemToDelete.FileNumber);
                }

                /// <summary>
                ///         removes an item entry from this directory.
                /// </summary>
                /// <param name="itemToRemove">the item to remove.</param>
                internal void RemoveItem(VirtualItem itemToRemove)
                {
                        IEnumerable<VirtualItem> items = ReadAllItems();

                        var dataWriter = new DataWriter();

                        foreach (VirtualItem remainingItem in items.Where(item => item.FileNumber != itemToRemove.FileNumber))
                        {
                                SerializeItem(remainingItem, dataWriter);
                        }

                        FileStream stream = GetFileStream();
                        byte[] data = dataWriter.GetData();
                        stream.Write(BitConverter.GetBytes((ulong) data.Length));
                        stream.Write(data);
                }

                /// <summary>
                ///         imports a directory from the host file system into this virtual directory.
                /// </summary>
                /// <param name="directoryToImport">the location of the directory to import in the host file system.</param>
                /// <returns>the imported virtual directory.</returns>
                public VirtualDirectory ImportDirectory(string directoryToImport)
                {
                        if (!Directory.Exists(directoryToImport))
                        {
                                throw new DirectoryNotFoundException();
                        }

                        string importedDirectoryName = Path.GetFileName(directoryToImport);

                        VirtualDirectory importedDirectory = AddDirectory(importedDirectoryName);

                        foreach (string directory in Directory.GetDirectories(directoryToImport))
                        {
                                importedDirectory.ImportDirectory(directory);
                        }

                        foreach (string file in Directory.GetFiles(directoryToImport))
                        {
                                importedDirectory.ImportFile(file);
                        }

                        return importedDirectory;
                }

                /// <summary>
                ///         imports a file from the host file system into the virtual disk.
                /// </summary>
                /// <param name="fileToImport">the location of the fil to import in the host file system.</param>
                /// <returns>the imported virtual file.</returns>
                public VirtualFile ImportFile(string fileToImport)
                {
                        if (!File.Exists(fileToImport))
                        {
                                throw new FileNotFoundException();
                        }

                        string importedFileName = Path.GetFileName(fileToImport);
                        VirtualFile importedFile = AddFile(importedFileName);

                        importedFile.ImportContent(fileToImport);

                        return importedFile;
                }

                internal void RenameItem(VirtualItem virtualItem, string newName)
                {
                        if (!ValidName(newName))
                        {
                                throw new ArgumentException("newName invalid");
                        }

                        var dataWriter = new DataWriter();

                        foreach (VirtualItem item in ReadAllItems())
                        {
                                if (item.FileNumber == virtualItem.FileNumber)
                                {
                                        item.Name = newName;
                                }

                                SerializeItem(item, dataWriter);
                        }

                        FileStream stream = GetFileStream();
                        byte[] data = dataWriter.GetData();

                        if (stream.Length < data.Length + 8)
                        {
                                ContainerFile.ChangeFileSize(FileNumber, (uint) (data.Length + 8));
                        }

                        stream = GetFileStream();
                        stream.Write(BitConverter.GetBytes((ulong) data.Length));
                        stream.Write(data);
                }

                /// <summary>
                ///         returns a virtual item given a relative path in the virtual disks directory hierarchy starting from this directory.
                /// </summary>
                /// <param name="relativePath">the relative path.</param>
                /// <returns>the last item in the relative path or null if the path does not refer to an item.</returns>
                public VirtualItem GetRelativeItem(string relativePath)
                {
                        if (relativePath == null)
                        {
                                throw new ArgumentException("relativePath must not be null");
                        }

                        string[] splittedPath = relativePath.Split(DirectorySeparator);

                        if (splittedPath.Length < 0)
                        {
                                throw new ArgumentException("relativePath invalid");
                        }

                        if (!splittedPath.All(ValidName))
                        {
                                throw new ArgumentException("some part of relativePath is not well formed");
                        }

                        VirtualDirectory current = this;

                        for (int i = 0; i < splittedPath.Length - 1; i++)
                        {
                                current = (VirtualDirectory) current.ReadAllItems().FirstOrDefault(item => item is VirtualDirectory && item.Name.Equals(splittedPath[i]));

                                if (current == null)
                                {
                                        return null;
                                }
                        }

                        return current.ReadAllItems().FirstOrDefault(item => item.Name.Equals(splittedPath.Last()));
                }

                /// <summary>
                ///         used to distinguish between directory- and file entries.
                /// </summary>
                private enum ItemType : byte
                {
                        Directory = 0,
                        File = 1
                }
        }
}
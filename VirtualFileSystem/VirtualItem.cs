using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using UnixStyleContainerFileFramework;
using FileStream = UnixStyleContainerFileFramework.FileStream;

namespace VirtualFileSystem
{
        /// <summary>
        ///         represents a directory or file in the virtual file system.
        /// </summary>
        public abstract class VirtualItem
        {
                /// <summary>
                ///         constructor.
                /// </summary>
                /// <param name="fileNumber">the block index the file begins at in the parent container file.</param>
                /// <param name="name">the name of this virtual item (file name or directory name).</param>
                /// <param name="parentDirectory">an instance of the parent directory.</param>
                /// <param name="containerFile">an instance of the parent container file.</param>
                internal VirtualItem(uint fileNumber, string name, VirtualDirectory parentDirectory,
                                     ContainerFile containerFile)
                {
                        FileNumber = fileNumber;
                        Name = name;
                        ParentDirectory = parentDirectory;
                        ContainerFile = containerFile;
                }

                /// <summary>
                ///         the parent container file this virtual item is existing in.
                /// </summary>
                /// this field is used to enable items to operate without going over the parent virtual
                /// disk object.
                protected ContainerFile ContainerFile { get; private set; }

                /// <summary>
                ///         the name of this virtual item (file name or directory name).
                /// </summary>
                public string Name { get; internal set; }

                /// <summary>
                ///         the block index the file begins at in the parent container file.
                /// </summary>
                internal uint FileNumber { get; private set; }

                /// <summary>
                ///         the parent directory of this item.
                /// </summary>
                public VirtualDirectory ParentDirectory { get; private set; }


                /// <summary>
                ///         checks whether a given string is valid to be used as a file name.
                /// </summary>
                /// <param name="name">the name to check.</param>
                /// <returns>whether the name can be used as a file/directory name.</returns>
                internal static bool ValidName(string name)
                {
                        if (string.IsNullOrEmpty(name)) return false;

                        return Path.GetInvalidFileNameChars().All(character => !name.ToCharArray().Any(c => c == character));
                }

                /// <summary>
                ///         returns the file stream to access the contents of this item in the container file.
                /// </summary>
                /// <returns>the file stream to access the contents of this item in the container file.</returns>
                /// SuppressMessage justification: operation can be very expensive therefore it should indicate it computes something
                [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
                protected FileStream GetFileStream()
                {
                        return ContainerFile.OpenFile(FileNumber);
                }

                /// <summary>
                ///         deletes the item.
                /// </summary>
                /// derived classes may override this method to dispose themselves apropriately.
                public virtual void Delete()
                {
                        ParentDirectory.DeleteItem(this);
                }

                /// <summary>
                ///         renames the item.
                /// </summary>
                /// <param name="newName">the new name the item should use.</param>
                public void Rename(string newName)
                {
                        if (!ValidName(newName))
                        {
                                throw new ArgumentException("newName is not valid");
                        }

                        if (ParentDirectory != null)
                        {
                                ParentDirectory.RenameItem(this, newName);

                                Name = newName;
                        }
                }

                /// <summary>
                ///         moves the item into the target directory.
                /// </summary>
                /// <param name="targetDirectory">the directory this item should be moved to.</param>
                public void Move(VirtualDirectory targetDirectory)
                {
                        if (targetDirectory == null)
                        {
                                throw new ArgumentException("targetDirectory must not be null");
                        }

                        if (ParentDirectory == null)
                        {
                                throw new ArgumentException("cannot move root");
                        }

                        ParentDirectory.RemoveItem(this);

                        ParentDirectory = targetDirectory;

                        targetDirectory.AddItem(this);
                }

                /// <summary>
                ///         exports the item to the host file system.
                /// </summary>
                /// <param name="pathName">the name of the exported item (including path).</param>
                public abstract void Export(string pathName);

                /// <summary>
                ///         copies this item into the target directory with a given name.
                /// </summary>
                /// <param name="targetDirectory">the target directory this item should be copied to</param>
                /// <param name="newName">the new name the copied item should take after copying.</param>
                /// <returns>the created copy.</returns>
                public abstract VirtualItem Copy(VirtualDirectory targetDirectory, string newName);
        }
}
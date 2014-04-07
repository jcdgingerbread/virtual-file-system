using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NUnit.Framework;
using VirtualFileSystem.Exceptions;

namespace VirtualFileSystem.Tests
{
        [TestFixture]
        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        internal class DeleteTest
        {
                [Test]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void DeleteOthers()
                {
                        const string FILE_NAME = "test.jcd";

                        VirtualDisk vd = VirtualDisk.Create(FILE_NAME, 1024 * 1024, 10);

                        VirtualDirectory root = vd.RootDirectory;

                        root.AddDirectory("dir1");
                        VirtualDirectory dir2 = root.AddDirectory("dir2");
                        root.AddDirectory("dir3");
                        VirtualFile file1 = root.AddFile("file1");
                        VirtualDirectory dir4 = root.AddDirectory("dir4");
                        dir4.AddFile("file2");
                        root.AddDirectory("dir5");

                        dir2.Delete();

                        VirtualItem[] itemsAfterDir2Delete = root.ReadAllItems().ToArray();
                        Assert.True(itemsAfterDir2Delete.Length == 5);
                        Assert.True(itemsAfterDir2Delete.All(entry => !entry.Name.Equals(dir2.Name) && entry.FileNumber != dir2.FileNumber));


                        dir4.Delete();

                        VirtualItem[] itemsAfterDir4Delete = root.ReadAllItems().ToArray();
                        Assert.True(itemsAfterDir4Delete.Length == 4);
                        Assert.True(itemsAfterDir4Delete.All(entry => !entry.Name.Equals(dir4.Name) && entry.FileNumber != dir4.FileNumber));


                        file1.Delete();

                        VirtualItem[] itemsAfterFile1Delete = root.ReadAllItems().ToArray();
                        Assert.True(itemsAfterFile1Delete.Length == 3);
                        Assert.True(itemsAfterFile1Delete.All(entry => !entry.Name.Equals(file1.Name) && entry.FileNumber != file1.FileNumber));
                }

                [Test]
                [ExpectedException(typeof (RootDeletionException))]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void DeleteRoot()
                {
                        const string FILE_NAME = "test.jcd";

                        VirtualDisk vd = VirtualDisk.Create(FILE_NAME, 1024 * 1024, 1);

                        VirtualDirectory root = vd.RootDirectory;

                        root.Delete();

                        vd.Close();
                }
        }
}
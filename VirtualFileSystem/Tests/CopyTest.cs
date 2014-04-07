using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NUnit.Framework;
using VirtualFileSystem.Exceptions;

namespace VirtualFileSystem.Tests
{
        [TestFixture]
        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        internal class CopyTest
        {
                [Test]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void CopyDirectory()
                {
                        const string FILE_NAME = "test.jcd";

                        VirtualDisk vd = VirtualDisk.Create(FILE_NAME, 1024 * 1024, 100);

                        VirtualDirectory root = vd.RootDirectory;

                        VirtualDirectory dir1 = root.AddDirectory("dir1");
                        root.AddDirectory("dir2");
                        root.AddDirectory("dir3");
                        root.AddFile("file1");
                        VirtualDirectory dir4 = root.AddDirectory("dir4");
                        dir4.AddFile("file2");
                        dir4.AddDirectory("dir5");

                        dir4.Copy(dir1, dir4.Name);

                        Assert.True(dir1.ReadAllItems().Any(item => item.Name.Equals(dir4.Name)));
                }

                [Test]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void CopySameDirectoryTwice()
                {
                        const string FILE_NAME = "test.jcd";

                        VirtualDisk vd = VirtualDisk.Create(FILE_NAME, 1024 * 1024, 100);

                        VirtualDirectory root = vd.RootDirectory;

                        VirtualDirectory dir1 = root.AddDirectory("dir1");
                        root.AddDirectory("dir2");
                        root.AddDirectory("dir3");
                        root.AddFile("file1");
                        VirtualDirectory dir4 = root.AddDirectory("dir4");
                        dir4.AddFile("file2");
                        dir4.AddDirectory("dir5");

                        dir4.Copy(dir1, dir4.Name);
                        dir4.Copy(dir1, dir4.Name);
                }

                [Test]
                [ExpectedException(typeof (ArgumentException))]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void InvalidDirectoryCopyName()
                {
                        const string FILE_NAME = "test.jcd";

                        VirtualDisk vd = VirtualDisk.Create(FILE_NAME, 1024 * 1024, 100);

                        VirtualDirectory root = vd.RootDirectory;

                        VirtualDirectory dir1 = root.AddDirectory("dir1");
                        root.AddDirectory("dir2");
                        root.AddDirectory("dir3");
                        root.AddFile("file1");
                        VirtualDirectory dir4 = root.AddDirectory("dir4");
                        dir4.AddFile("file2");
                        dir4.AddDirectory("dir5");

                        dir4.Copy(dir1, "*/*ds..,^^");
                }

                [Test]
                [ExpectedException(typeof (ArgumentException))]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void InvalidFileCopyName()
                {
                        const string FILE_NAME = "test.jcd";

                        VirtualDisk vd = VirtualDisk.Create(FILE_NAME, 1024 * 1024, 100);

                        VirtualDirectory root = vd.RootDirectory;

                        VirtualDirectory dir1 = root.AddDirectory("dir1");
                        root.AddDirectory("dir2");
                        root.AddDirectory("dir3");
                        root.AddFile("file1");
                        VirtualDirectory dir4 = root.AddDirectory("dir4");
                        VirtualFile file2 = dir4.AddFile("file2");
                        dir4.AddDirectory("dir5");

                        file2.Copy(dir1, "*/*ds..,^^");
                }

                [Test]
                [ExpectedException(typeof (ArgumentException))]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void InvalidFileCopyTarget1()
                {
                        const string FILE_NAME = "test.jcd";

                        VirtualDisk vd = VirtualDisk.Create(FILE_NAME, 1024 * 1024, 100);

                        VirtualDirectory root = vd.RootDirectory;

                        root.AddDirectory("dir1");
                        root.AddDirectory("dir2");
                        root.AddDirectory("dir3");
                        root.AddFile("file1");
                        VirtualDirectory dir4 = root.AddDirectory("dir4");
                        VirtualFile file2 = dir4.AddFile("file2");
                        dir4.AddDirectory("dir5");

                        file2.Copy(null, file2.Name);
                }

                [Test]
                [ExpectedException(typeof (ItemNameInUseException))]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void InvalidFileCopyTargetAlreadyHasItemWithSameName()
                {
                        const string FILE_NAME = "test.jcd";
                        const string SAME_NAME = "file_same";

                        VirtualDisk vd = VirtualDisk.Create(FILE_NAME, 1024 * 1024, 100);

                        VirtualDirectory root = vd.RootDirectory;

                        VirtualDirectory dir1 = root.AddDirectory("dir1");
                        dir1.AddFile(SAME_NAME);
                        root.AddDirectory("dir2");
                        root.AddDirectory("dir3");
                        root.AddFile("file1");
                        VirtualDirectory dir4 = root.AddDirectory("dir4");
                        VirtualFile file2 = dir4.AddFile(SAME_NAME);
                        dir4.AddDirectory("dir5");

                        file2.Copy(dir1, file2.Name);
                }
        }
}
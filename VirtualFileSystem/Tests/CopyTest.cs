using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NUnit.Framework;

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
                public static void InvalidCopyName()
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
        }
}
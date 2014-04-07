using System;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace VirtualFileSystem.Tests
{
        [TestFixture]
        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        internal class InvalidItemAddTest
        {
                [Test]
                [ExpectedException(typeof (ArgumentException))]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void InvalidDirectoryAdd()
                {
                        const string FILE_NAME = "test.jcd";

                        VirtualDisk vd = VirtualDisk.Create(FILE_NAME, 1024 * 1024, 100);

                        VirtualDirectory root = vd.RootDirectory;

                        root.AddDirectory("dir1");
                        root.AddDirectory("dir2");
                        root.AddDirectory("dir3");
                        root.AddFile("file1");
                        VirtualDirectory dir4 = root.AddDirectory("dir4");
                        dir4.AddFile("file2");
                        dir4.AddDirectory("dir5");

                        dir4.AddDirectory("``^^?'//*");
                }

                [Test]
                [ExpectedException(typeof (ArgumentException))]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void InvalidFileAdd()
                {
                        const string FILE_NAME = "test.jcd";

                        VirtualDisk vd = VirtualDisk.Create(FILE_NAME, 1024 * 1024, 100);

                        VirtualDirectory root = vd.RootDirectory;

                        root.AddDirectory("dir1");
                        root.AddDirectory("dir2");
                        root.AddDirectory("dir3");
                        root.AddFile("file1");
                        VirtualDirectory dir4 = root.AddDirectory("dir4");
                        dir4.AddFile("file2");
                        dir4.AddDirectory("dir5");

                        dir4.AddFile("``^^?'//*");
                }

                [Test]
                [ExpectedException(typeof (ArgumentException))]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void InvalidFileAdd2()
                {
                        const string FILE_NAME = "test.jcd";

                        VirtualDisk vd = VirtualDisk.Create(FILE_NAME, 1024 * 1024, 100);

                        VirtualDirectory root = vd.RootDirectory;

                        root.AddDirectory("dir1");
                        root.AddDirectory("dir2");
                        root.AddDirectory("dir3");
                        root.AddFile("file1");
                        VirtualDirectory dir4 = root.AddDirectory("dir4");
                        dir4.AddFile("file2");
                        dir4.AddDirectory("dir5");

                        dir4.AddFile(null);
                }
        }
}
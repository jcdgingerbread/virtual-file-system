using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using VirtualFileSystem.Exceptions;

namespace VirtualFileSystem.Tests
{
        [TestFixture]
        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        internal class SameItemNameTest
        {
                [Test]
                [ExpectedException(typeof (ItemNameInUseException))]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void SameDirectoryName()
                {
                        const string FILE_NAME = "test.jcd";

                        VirtualDisk vd = VirtualDisk.Create(FILE_NAME, 1024 * 1024, 10);

                        VirtualDirectory root = vd.RootDirectory;

                        root.AddDirectory("dir1");
                        root.AddDirectory("dir2");
                        root.AddDirectory("dir1");
                }

                [Test]
                [ExpectedException(typeof (ItemNameInUseException))]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void SameItemName()
                {
                        const string FILE_NAME = "test.jcd";

                        VirtualDisk vd = VirtualDisk.Create(FILE_NAME, 1024 * 1024, 10);

                        VirtualDirectory root = vd.RootDirectory;

                        root.AddFile("file1");
                        root.AddFile("file2");
                        root.AddFile("file1");
                }
        }
}
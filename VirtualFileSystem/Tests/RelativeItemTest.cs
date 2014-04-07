using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace VirtualFileSystem.Tests
{
        [TestFixture]
        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        internal class RelativeItemTest
        {
                [Test]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void RelativeItem()
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

                        VirtualItem file2Again = root.GetRelativeItem(dir4.Name + VirtualDirectory.DirectorySeparator + file2.Name);

                        Assert.AreEqual(file2.FileNumber, file2Again.FileNumber);
                }
        }
}
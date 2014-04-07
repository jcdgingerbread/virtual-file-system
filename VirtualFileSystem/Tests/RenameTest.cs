using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NUnit.Framework;

namespace VirtualFileSystem.Tests
{
        [TestFixture]
        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        internal class RenameTest
        {
                [Test]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void Rename()
                {
                        const string FILE_NAME = "test.jcd";
                        const string RENAMED_NAME = "renamed_item";

                        VirtualDisk vd = VirtualDisk.Create(FILE_NAME, 1024 * 1024, 100);

                        VirtualDirectory root = vd.RootDirectory;

                        root.AddDirectory("dir1");
                        root.AddDirectory("dir2");
                        root.AddDirectory("dir3");
                        root.AddFile("file1");
                        VirtualDirectory dir4 = root.AddDirectory("dir4");
                        dir4.AddFile("file2");
                        dir4.AddDirectory("dir5");

                        dir4.Rename(RENAMED_NAME);

                        Assert.True(dir4.Name.Equals(RENAMED_NAME));
                        Assert.True(root.ReadAllItems().Any(item => item.Name.Equals(dir4.Name)));
                }
        }
}
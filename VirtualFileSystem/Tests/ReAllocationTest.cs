using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace VirtualFileSystem.Tests
{
        [TestFixture]
        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        internal class ReAllocationTest
        {
                [SetUp]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void Setup()
                {
                        const string FILE_TO_IMPORT = "SampleFile.txt";

                        VirtualDisk vd = VirtualDisk.Create(FILE_NAME, 1024 * 1024, 100);

                        VirtualDirectory root = vd.RootDirectory;

                        VirtualDirectory dir1 = root.AddDirectory("dir1");
                        dir1.AddDirectory("dir2");

                        VirtualFile importedFile = root.AddFile("test_imported_file");

                        importedFile.ImportContent(FILE_TO_IMPORT);

                        root.AddDirectory("dir3");

                        importedFile.Delete();

                        vd.Close();
                }

                private const string FILE_NAME = "test.jcd";

                [Test]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void ReAllocate()
                {
                        VirtualDisk vd = VirtualDisk.Open(FILE_NAME);

                        VirtualDirectory root = vd.RootDirectory;

                        VirtualDirectory dir4 = root.AddDirectory("dir4");

                        dir4.AddFile("file1");

                        vd.Close();
                }
        }
}
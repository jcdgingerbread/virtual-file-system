using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace VirtualFileSystem.Tests
{
        [TestFixture]
        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        internal class FreeLargeFileTest
        {
                [Test]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void FreeLargeFile()
                {
                        const string FILE_NAME = "test.jcd";
                        const string FILE_TO_IMPORT = "SampleFile.txt";

                        VirtualDisk vd = VirtualDisk.Create(FILE_NAME, 1024 * 1024, 100);

                        VirtualDirectory root = vd.RootDirectory;

                        VirtualFile importedFile = root.AddFile("test_imported_file");

                        importedFile.ImportContent(FILE_TO_IMPORT);

                        importedFile.Delete();

                        vd.Close();
                }
        }
}
using System.Diagnostics.CodeAnalysis;
using System.IO;
using NUnit.Framework;

namespace VirtualFileSystem.Tests
{
        [TestFixture]
        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        internal class ImportExportTest
        {
                [Test]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void ImportExport()
                {
                        const string FILE_NAME = "test.jcd";
                        const string FILE_TO_IMPORT = "SampleFile.txt";
                        const string EXPORTED_DIRECTORY = "exported\\";

                        if (!Directory.Exists(EXPORTED_DIRECTORY))
                        {
                                Directory.CreateDirectory(EXPORTED_DIRECTORY);
                        }

                        if (File.Exists(EXPORTED_DIRECTORY + FILE_TO_IMPORT))
                        {
                                File.Delete(EXPORTED_DIRECTORY + FILE_TO_IMPORT);
                        }

                        VirtualDisk vd = VirtualDisk.Create(FILE_NAME, 2024 * 1024 * 1024, 100);

                        VirtualDirectory root = vd.RootDirectory;

                        VirtualFile importedFile = root.ImportFile(FILE_TO_IMPORT);

                        importedFile.Export(EXPORTED_DIRECTORY);

                        vd.Close();
                }
        }
}
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
                public static void ImportExportDirectory()
                {
                        const string FILE_NAME = "test.jcd";
                        const string DIRECTORY_TO_IMPORT = "SampleDirectory";
                        const string EXPORTED_DIRECTORY = "exported";
                        string exportedDirectoryName = EXPORTED_DIRECTORY + Path.DirectorySeparatorChar + DIRECTORY_TO_IMPORT;

                        if (!Directory.Exists(EXPORTED_DIRECTORY))
                        {
                                Directory.CreateDirectory(EXPORTED_DIRECTORY);
                        }

                        if (Directory.Exists(exportedDirectoryName))
                        {
                                Directory.Delete(exportedDirectoryName, true);
                        }

                        VirtualDisk vd = VirtualDisk.Create(FILE_NAME, 2024 * 1024 * 1024, 100);

                        VirtualDirectory root = vd.RootDirectory;

                        VirtualDirectory importedDirectory = root.ImportDirectory(DIRECTORY_TO_IMPORT);

                        importedDirectory.Export(exportedDirectoryName);

                        vd.Close();
                }

                [Test]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void ImportExportFile()
                {
                        const string FILE_NAME = "test.jcd";
                        const string FILE_TO_IMPORT = "SampleFile.txt";
                        const string EXPORTED_DIRECTORY = "exported";
                        string exportedFileName = EXPORTED_DIRECTORY + Path.DirectorySeparatorChar + FILE_TO_IMPORT;

                        if (!Directory.Exists(EXPORTED_DIRECTORY))
                        {
                                Directory.CreateDirectory(EXPORTED_DIRECTORY);
                        }

                        if (File.Exists(exportedFileName))
                        {
                                File.Delete(exportedFileName);
                        }

                        VirtualDisk vd = VirtualDisk.Create(FILE_NAME, 2024 * 1024 * 1024, 100);

                        VirtualDirectory root = vd.RootDirectory;

                        VirtualFile importedFile = root.ImportFile(FILE_TO_IMPORT);

                        importedFile.Export(exportedFileName);

                        vd.Close();
                }
        }
}
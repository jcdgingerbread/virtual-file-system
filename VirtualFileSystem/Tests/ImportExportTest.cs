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
                [ExpectedException(typeof (DirectoryNotFoundException))]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void ImportDirectoryInvalid()
                {
                        const string FILE_NAME = "test.jcd";
                        const string DIRECTORY_TO_IMPORT = "SampleDirectory_ShouldNotExist";

                        if (Directory.Exists(DIRECTORY_TO_IMPORT))
                        {
                                Directory.Delete(DIRECTORY_TO_IMPORT, true);
                        }

                        VirtualDisk vd = VirtualDisk.Create(FILE_NAME, 2024 * 1024 * 1024, 100);

                        VirtualDirectory root = vd.RootDirectory;

                        root.ImportDirectory(DIRECTORY_TO_IMPORT);
                }

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

                [Test]
                [ExpectedException(typeof (FileNotFoundException))]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void ImportFileInvalid()
                {
                        const string FILE_NAME = "test.jcd";
                        const string FILE_TO_IMPORT = "SampleFile_ShouldNotExist";

                        if (File.Exists(FILE_TO_IMPORT))
                        {
                                File.Delete(FILE_TO_IMPORT);
                        }

                        VirtualDisk vd = VirtualDisk.Create(FILE_NAME, 2024 * 1024 * 1024, 100);

                        VirtualDirectory root = vd.RootDirectory;

                        root.ImportFile(FILE_TO_IMPORT);
                }
        }
}
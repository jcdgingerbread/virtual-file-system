using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using UnixStyleContainerFileFramework.Exceptions;

namespace VirtualFileSystem.Tests
{
        [TestFixture]
        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        internal class OutOfFreeFilesTest
        {
                [Test]
                [ExpectedException(typeof (OutOfFreeFilesException))]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void OutOfFreeFiles()
                {
                        const string FILE_NAME = "test.jcd";

                        VirtualDisk vd = VirtualDisk.Create(FILE_NAME, 1024 * 1024, 1);

                        VirtualDirectory root = vd.RootDirectory;

                        uint freeFiles = vd.FreeFiles;

                        for (int i = 0; i < freeFiles; i++)
                        {
                                root.AddFile("file" + i);
                        }

                        root.AddFile("over_the_top");

                        vd.Close();
                }

                [Test]
                [ExpectedException(typeof (OutOfFreeSpaceException))]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void OutOfFreeSpace()
                {
                        const string FILE_NAME = "test.jcd";
                        const string FILE_TO_IMPORT = "SampleFile.txt";

                        VirtualDisk vd = VirtualDisk.Create(FILE_NAME, 1024, 1);

                        VirtualDirectory root = vd.RootDirectory;

                        VirtualFile largeFile = root.AddFile("large_file_exceeding_free_space");

                        largeFile.ImportContent(FILE_TO_IMPORT);

                        vd.Close();
                }
        }
}
using System.Diagnostics.CodeAnalysis;
using System.IO;
using NUnit.Framework;
using UnixStyleContainerFileFramework.Exceptions;

namespace VirtualFileSystem.Tests
{
        [TestFixture]
        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        internal class InvalidInitializationTest
        {
                private const string FILE_NAME = "test.jcd";

                [Test]
                [ExpectedException(typeof (InvalidDiskSizeException))]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void EmptyDisk()
                {
                        VirtualDisk.Create(FILE_NAME, 0, 1);
                }

                [Test]
                [ExpectedException(typeof (InvalidFileCountException))]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void NoFiles()
                {
                        VirtualDisk.Create(FILE_NAME, 1, 0);
                }

                [Test]
                [ExpectedException(typeof (FileNotFoundException))]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void NonExistingDiskFile()
                {
                        if (File.Exists(FILE_NAME))
                        {
                                File.Delete(FILE_NAME);
                        }

                        VirtualDisk.Open(FILE_NAME);
                }

                [Test]
                [ExpectedException(typeof (InvalidFileCountException))]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void TooManyFilesPerDiskSize()
                {
                        VirtualDisk.Create(FILE_NAME, 1, 1337);
                }
        }
}
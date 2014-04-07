using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NUnit.Framework;

namespace VirtualFileSystem.Tests
{
        [TestFixture]
        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        internal class OpeningSavedDiskTest
        {
                [SetUp]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void Setup()
                {
                        VirtualDisk vd = VirtualDisk.Create(FILE_NAME, 1024 * 1024, 100);

                        VirtualDirectory root = vd.RootDirectory;

                        for (int i = 0; i < 100; i++)
                        {
                                root.AddDirectory("test_dir" + i);
                        }

                        VirtualItem[] items = vd.RootDirectory.ReadAllItems().ToArray();

                        for (int i = 0; i < 50; i++)
                        {
                                items[i].Delete();
                        }

                        vd.Close();
                }

                private const string FILE_NAME = "test.jcd";


                [Test]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void OpeningSavedDisk()
                {
                        VirtualDisk vd = VirtualDisk.Open(FILE_NAME);

                        VirtualItem[] items = vd.RootDirectory.ReadAllItems().ToArray();

                        Assert.True(items.Length == 50);

                        vd.Close();
                }
        }
}
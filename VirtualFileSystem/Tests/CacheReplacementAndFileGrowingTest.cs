using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NUnit.Framework;

namespace VirtualFileSystem.Tests
{
        [TestFixture]
        internal class CacheReplacementAndFileGrowingTest
        {
                [Test]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void CacheReplacementAndFileGrowing()
                {
                        const string FILE_NAME = "test.jcd";

                        VirtualDisk vd = VirtualDisk.Create(FILE_NAME, 1024 * 1024, 100);

                        VirtualDirectory root = vd.RootDirectory;

                        for (int i = 0; i < 100; i++)
                        {
                                root.AddDirectory("test_dir" + i);
                        }

                        VirtualItem[] items = root.ReadAllItems().ToArray();

                        for (int i = 0; i < 100; i++)
                        {
                                Assert.True(items.Any(item => item.Name.Equals("test_dir" + i)));
                        }

                        vd.Close();
                }
        }
}
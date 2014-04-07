using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace VirtualFileSystem.Tests
{
        [TestFixture]
        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        internal class DiskStatsTest
        {
                [Test]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                public static void DiskStats()
                {
                        const string FILE_NAME = "test.jcd";

                        const long MINIMUM_SPACE = 1024 * 1024;
                        const uint MINIMUM_FILES = 100;

                        VirtualDisk vd = VirtualDisk.Create(FILE_NAME, MINIMUM_SPACE, MINIMUM_FILES);

                        Assert.LessOrEqual(MINIMUM_SPACE, vd.TotalSpace);
                        Assert.LessOrEqual(MINIMUM_FILES, vd.FreeFiles + 1);
                        Assert.Less(vd.FreeSpace, vd.TotalSpace);
                }
        }
}
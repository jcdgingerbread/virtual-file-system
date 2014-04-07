using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using VirtualFileSystem;

namespace Test
{
        internal static class Program
        {
                [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "args")]
                private static void Main(string[] args)
                {
                        VirtualDisk virtualDisk = VirtualDisk.Create("test.jcd", 1024 * 1024, 100);
                        //VirtualDisk virtualDisk = VirtualDisk.Open("test.jcd");

                        VirtualDirectory root = virtualDisk.RootDirectory;

                        root.AddDirectory("test_dir1");
                        VirtualDirectory gingerbread = root.AddDirectory("gingerbread");
                        gingerbread.AddDirectory("blurdiblurb");

                        root.ImportDirectory("import_directory");


                        IEnumerable<VirtualDirectory> directories = root.ReadAllItems().OfType<VirtualDirectory>().ToArray();

                        VirtualDirectory first = directories.First();
                        VirtualDirectory last = directories.Last();


                        PrintFileSystem(root, "");

                        Console.WriteLine();

                        Console.WriteLine(first.Name);
                        Console.WriteLine(last.Name);

                        Console.WriteLine();

                        Console.WriteLine(virtualDisk.FreeFiles + " free files");
                        Console.WriteLine("{0} bytes free / {1} bytes used / {2} bytes total capacity", virtualDisk.FreeSpace, virtualDisk.TotalSpace - virtualDisk.FreeSpace, virtualDisk.TotalSpace);

                        Console.ReadKey();
                        Console.Clear();


                        //root.Copy(first, "root_copy");

                        first.Rename("simon");

                        PrintFileSystem(root, "");
                        /* const string FILE_NAME = "test.jcd";
                        const string FILE_TO_IMPORT = "SampleFile.txt";
                        const string EXPORTED_FILE_NAME = "exported.txt";

                        if (File.Exists(EXPORTED_FILE_NAME))
                        {
                                File.Delete(EXPORTED_FILE_NAME);
                        }

                        VirtualDisk vd = VirtualDisk.Create(FILE_NAME, 2048L * 1024 * 1024, 100);

                        VirtualDirectory root = vd.RootDirectory;

                        VirtualFile importedFile = root.AddFile("test_imported_file");

                        importedFile.ImportContent(FILE_TO_IMPORT);*/

                        Console.ReadKey();

                        virtualDisk.Close();
                }

                private static void PrintFileSystem(VirtualItem itemToPrint, string offset)
                {
                        var directory = itemToPrint as VirtualDirectory;

                        Console.Write(offset + itemToPrint.Name);

                        if (directory != null)
                        {
                                Console.WriteLine("/");

                                IEnumerable<VirtualItem> items = directory.ReadAllItems();
                                string newSpacing = " | " + offset;

                                foreach (VirtualItem item in items)
                                {
                                        PrintFileSystem(item, newSpacing);
                                }
                        }
                        else
                        {
                                Console.WriteLine();
                        }
                }
        }
}
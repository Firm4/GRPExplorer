using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using GRPExplorerLib;
using GRPExplorerLib.BigFile;
using GRPExplorerLib.Logging;
using GRPExplorerLib.BigFile.Files;
using GRPExplorerLib.BigFile.Files.Archetypes;
using GRPExplorerLib.Util;

namespace GRPExplorerTests
{
    class Program
    {
        static ILogProxy log = LogManager.GetLogProxy(">");

        static Action[] funcs =
        {
            TestTextures,
            ProcessBigmap,
            CompareFiles,
            CompareAllFiles,
            LogFiles,
            CheckFiles,
        };

        static void Main(string[] args)
        {
            for (int i = 0; i < funcs.Length; i++)
            {
                Console.WriteLine("{0}: {1}", i, funcs[i].Method.Name);
            }
            Console.Write("Choice: ");
            int num = int.Parse(Console.ReadLine());
            Console.Clear();
            funcs[num]();
        }

        static void CheckFiles()
        {
            Console.Write("File path: ");
            string path = Console.ReadLine();
            if (!File.Exists(path) && !Directory.Exists(path))
                Environment.Exit(69);

            LogManager.GlobalLogFlags = LogFlags.Error | LogFlags.Info;

            BigFile bigFile = BigFile.OpenBigFile(path);
            bigFile.LoadFromDisk();

            for (int i = 0; i < bigFile.FileMap.FilesList.Length; i++)
            {
                BigFileFile file = bigFile.FileMap.FilesList[i];

                if (file.FileInfo.Offset == -1)
                {
                    LogManager.GlobalLogFlags = LogFlags.All;
                    file.FileInfo.DebugLog(log);
                    LogManager.GlobalLogFlags = LogFlags.Error | LogFlags.Info;
                }
            }

            Console.ReadLine();
        }

        static void CompareAllFiles()
        {
            LogManager.GlobalLogFlags = LogFlags.Error | LogFlags.Info;

            Console.Write("Bigfile 1: ");
            string path1 = Console.ReadLine();
            Console.Write("Bigfile 2: ");
            string path2 = Console.ReadLine();

            IOBuffers buffer1 = new IOBuffers();
            IOBuffers buffer2 = new IOBuffers();

            BigFile file1 = BigFile.OpenBigFile(path1);
            BigFile file2 = BigFile.OpenBigFile(path2);

            if (file1 == null)
            {
                Console.WriteLine("file 1 null");
                Console.ReadLine();
                Environment.Exit(911);
            }
            if (file2 == null)
            {
                Console.WriteLine("file 2 null");
                Console.ReadLine();
                Environment.Exit(911);
            }

            file1.LoadFromDisk();
            Console.Write("Press enter...");
            Console.ReadLine();
            file2.LoadFromDisk();
            Console.Write("Press enter...");
            Console.ReadLine();
            Console.Clear();

            int missingFiles = 0;
            if (file1.FileMap.FilesList.Length != file2.FileMap.FilesList.Length)
            {
                Console.WriteLine("Files count don't match!");
                foreach (BigFileFile file1file in file1.FileMap.FilesList)
                {
                    if (file2.FileMap[file1file.FileInfo.Key] == null)
                    {
                        missingFiles++;
                        Console.WriteLine("Found file {0} (key: {1:X8}) that appears in the first bigfile but not the second!", file1file.Name, file1file.FileInfo.Key);
                    }
                }
                Console.ReadLine();
                foreach (BigFileFile file2file in file2.FileMap.FilesList)
                {
                    if (file1.FileMap[file2file.FileInfo.Key] == null)
                    {
                        missingFiles++;
                        Console.WriteLine("Found file {0} (key: {1:X8}) that appears in the second bigfile but not the first!", file2file.Name, file2file.FileInfo.Key);
                    }
                }
                Console.ReadLine();
            }

            Console.WriteLine("");
            Console.WriteLine("Found {0} file discrepancies between the two bigfiles", missingFiles);
            Console.Write("Would you like to continue? ");
            Console.ReadLine();

            Console.Clear();

            IEnumerator<int[]> headers1 = file1.FileReader.ReadAllHeaders(file1.FileMap.FilesList, buffer1, file1.FileReader.DefaultFlags).GetEnumerator();
            IEnumerator<int[]> headers2 = file2.FileReader.ReadAllHeaders(file2.FileMap.FilesList, buffer1, file2.FileReader.DefaultFlags).GetEnumerator();

            void CompareHeaders(BigFileFile file1file, BigFileFile file2file, int[] a, int[] b)
            {
                bool foundError = a.Length != b.Length;
                if (!foundError)
                {
                    for (int i = 0; i < a.Length; i++)
                    {
                        if (a[i] != b[i])
                        {
                            foundError = true;
                            break;
                        }
                    }
                }
                if (foundError)
                {
                    Console.WriteLine("File {0} has a header discrepancy!");
                    Console.WriteLine("File 1 header length: {0}", a.Length);
                    Console.WriteLine("File 2 header length: {1}", b.Length);
                    for (int i = 0; i < a.Length; i++)
                        Console.Write("{0:X8} ", a[i]);
                    for (int i = 0; i < b.Length; i++)
                        Console.Write("{0:X8} ", b[i]);
                }
            }


        }

        static void CompareFiles()
        {
            LogManager.GlobalLogFlags = LogFlags.Error | LogFlags.Info;

            Console.Write("Bigfile 1: ");
            string path1 = Console.ReadLine();
            Console.Write("Bigfile 2: ");
            string path2 = Console.ReadLine();

            IOBuffers buffer1 = new IOBuffers();
            IOBuffers buffer2 = new IOBuffers();

            Console.Write("File key: ");
            int key = 0;
            key = Convert.ToInt32(Console.ReadLine(), 16);
            if (key == 0)
                Environment.Exit(420);

            BigFile file1 = BigFile.OpenBigFile(path1);
            BigFile file2 = BigFile.OpenBigFile(path2);

            if (file1 == null)
            {
                Console.WriteLine("file 1 null");
                Console.ReadLine();
                Environment.Exit(911);
            }
            if (file2 == null)
            {
                Console.WriteLine("file 2 null");
                Console.ReadLine();
                Environment.Exit(911);
            }

            file1.LoadFromDisk();
            Console.Write("Press enter...");
            Console.ReadLine();
            file2.LoadFromDisk();
            Console.Write("Press enter...");
            Console.ReadLine();

            BigFileFile bigFileFile1 = file1.FileMap[key];
            BigFileFile bigFileFile2 = file2.FileMap[key];

            int[] header1 = file1.FileReader.ReadFileHeader(bigFileFile1, buffer1, file1.FileReader.DefaultFlags);
            int[] header2 = file2.FileReader.ReadFileHeader(bigFileFile2, buffer2, file2.FileReader.DefaultFlags);
            
            int size1 = file1.FileReader.ReadFileRaw(bigFileFile1, buffer1, file1.FileReader.DefaultFlags);
            int size2 = file2.FileReader.ReadFileRaw(bigFileFile2, buffer2, file2.FileReader.DefaultFlags);

            int chksum1 = 0;
            int chksum2 = 0;
            for (int i = 0; i < size1; i++)
                chksum1 += buffer1[size1][i];
            for (int i = 0; i < size2; i++)
                chksum2 += buffer2[size2][i];

            Console.Clear();

            Console.WriteLine("Size 1: " + size1);
            Console.WriteLine("Checksum 1: " + chksum1);
            Console.WriteLine("Size 2: " + size2);
            Console.WriteLine("Checksum 2: " + chksum2);

            Console.Write("Header 1, length: {0} : ", header1.Length);
            for (int i = 0; i < header1.Length; i++)
                Console.Write("{0:X8} ", header1[i]);
            Console.Write("\nHeader 2, length: {0} : ", header2.Length);
            for (int i = 0; i < header2.Length; i++)
                Console.Write("{0:X8} ", header2[i]);

            Console.WriteLine("");

            LogManager.GlobalLogFlags = LogFlags.All;

            bigFileFile1.FileInfo.DebugLog(log);
            bigFileFile2.FileInfo.DebugLog(log);


            Console.ReadLine();
        }

        static void ProcessBigmap()
        {
            Console.Write("File path: ");
            string path = Console.ReadLine();
            if (!File.Exists(path))
                Environment.Exit(69);

            byte[] tmpBytes = new byte[8];

            using (FileStream fs = File.OpenRead(path))
            {
                while (fs.Read(tmpBytes, 0, 8) != 0)
                {
                    Array.Reverse(tmpBytes, 0, 4);
                    Array.Reverse(tmpBytes, 4, 4);
                    int keyA = BitConverter.ToInt32(tmpBytes, 0);
                    int keyB = BitConverter.ToInt32(tmpBytes, 4);
                    Console.WriteLine("{0:X4} {1:X4}", keyA, keyB);

                    if (keyB != 0)
                    {
                        Console.Write("Press enter...");
                        Console.ReadLine();
                    }
                }
            }

            Console.Write("End!");
            Console.ReadLine();
        }

        static void TestTextures()
        {
            Console.Write("File path: ");
            string path = Console.ReadLine();
            if (!File.Exists(path))
                Environment.Exit(69);

            Console.Write("\nType: ");
            string type = Console.ReadLine();
            YetiTextureFormat format;
            if (!Enum.TryParse(type, out format))
                Environment.Exit(420);
            
            LogManager.GlobalLogFlags = LogFlags.Error | LogFlags.Info;

            PackedBigFile bigFile = new PackedBigFile(new FileInfo(path));
            bigFile.LoadFromDisk();

            List<BigFileFile> textureFiles = bigFile.RootFolder.GetAllFilesOfArchetype<TextureMetadataFileArchetype>();
            bigFile.FileLoader.LoadFiles(textureFiles);
            foreach (BigFileFile file in textureFiles)
            {
                TextureMetadataFileArchetype archetype = file.ArchetypeAs<TextureMetadataFileArchetype>();
                if (archetype.Format == format)
                    Console.WriteLine(file.FullFolderPath + file.Name + " " + string.Format("{0:X2} {1} {2}", archetype.Format, archetype.Width, archetype.Height));
            }

            Console.ReadLine();
        }

        static void LogFiles()
        {
            Console.Write("File path: ");
            string path = Console.ReadLine();
            if (!File.Exists(path))
                Environment.Exit(69);

            LogManager.GlobalLogFlags = LogFlags.Error | LogFlags.Info;

            PackedBigFile bigFile = new PackedBigFile(new FileInfo(path));
            bigFile.LoadFromDisk();

            List<BigFileFile> files = bigFile.RootFolder.GetAllFilesOfArchetype<CurveFileArchetype>();
            bigFile.FileLoader.LoadFiles(files);
            foreach (BigFileFile file in files)
            {
                CurveFileArchetype archetype = file.ArchetypeAs<CurveFileArchetype>();
                log.Info(file.Name);
                archetype.Log(log);
            }

            Console.ReadLine();
        }
    }
}

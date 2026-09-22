using System;
using System.IO;
using System.Linq;
using Concepto.Packages.KBDoctorCore.Sources;

namespace KBDoctorCore.Tests
{
    internal static class Program
    {
        private static int Main()
        {
            try
            {
                GetLastTwoComparerDirectoriesSelectsNewestSnapshotsByName();
                GetLastTwoComparerDirectoriesSupportsSecondsPrecision();
                Console.WriteLine("All KBDoctorCore tests passed.");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }
        }

        private static void GetLastTwoComparerDirectoriesSelectsNewestSnapshotsByName()
        {
            string root = CreateTempDirectory();
            try
            {
                string newest = Directory.CreateDirectory(Path.Combine(root, "NVG-2026-07-05-1530")).FullName;
                string previous = Directory.CreateDirectory(Path.Combine(root, "NVG-2026-07-05-1420")).FullName;
                string older = Directory.CreateDirectory(Path.Combine(root, "NVG-2026-07-04-2315")).FullName;
                Directory.CreateDirectory(Path.Combine(root, "NVG-invalid"));
                Directory.CreateDirectory(Path.Combine(root, "OBJ-2026-07-05-1700"));

                Directory.SetLastWriteTime(newest, new DateTime(2020, 1, 1));
                Directory.SetLastWriteTime(previous, new DateTime(2020, 1, 2));
                Directory.SetLastWriteTime(older, new DateTime(2099, 1, 1));

                string[] selected = Utility.GetLastTwoComparerDirectories(root, "NVG");

                AssertEqual(2, selected.Length, "Expected exactly two NVG snapshots.");
                AssertEqual(newest, selected[0], "Expected the newest snapshot by directory name first.");
                AssertEqual(previous, selected[1], "Expected the second newest snapshot by directory name second.");
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        private static void GetLastTwoComparerDirectoriesSupportsSecondsPrecision()
        {
            string root = CreateTempDirectory();
            try
            {
                string newest = Directory.CreateDirectory(Path.Combine(root, "NVG-2026-07-05-153045")).FullName;
                string previous = Directory.CreateDirectory(Path.Combine(root, "NVG-2026-07-05-153044")).FullName;
                Directory.CreateDirectory(Path.Combine(root, "NVG-2026-07-05-1530"));

                string[] selected = Utility.GetLastTwoComparerDirectories(root, "NVG");

                AssertEqual(newest, selected[0], "Expected seconds-precision snapshot to sort correctly.");
                AssertEqual(previous, selected[1], "Expected second newest seconds-precision snapshot.");
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        private static string CreateTempDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), "KBDoctorCore.Tests." + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static void AssertEqual<T>(T expected, T actual, string message)
        {
            if (!object.Equals(expected, actual))
            {
                throw new InvalidOperationException(message + " Expected: " + expected + ". Actual: " + actual + ".");
            }
        }
    }
}

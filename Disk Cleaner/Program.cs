// See https://aka.ms/new-console-template for more information
using System.Diagnostics;

Console.WriteLine("A DiskPart Tool - By Casper");

Process p = new Process();                                    // new instance of Process class
p.StartInfo.UseShellExecute = false;                          // do not start a new shell
p.StartInfo.RedirectStandardOutput = true;                    // Redirects the on screen results
p.StartInfo.FileName = @"C:\Windows\System32\diskpart.exe";   // executable to run
p.StartInfo.RedirectStandardInput = true;                     // Redirects the input commands
p.Start();                                                    // Starts the process
p.StandardInput.WriteLine("List Disk");                       // Issues commands to diskpart
p.StandardInput.WriteLine("exit");                            // _\|/_
string list_disk_output = p.StandardOutput.ReadToEnd();       // Places the output to a variable
p.WaitForExit();                                              // Waits for the exe to finish

int startindex = list_disk_output.IndexOf("DISKPART>");
int endindex = list_disk_output.LastIndexOf("DISKPART>");
string list_disk_output_trim = list_disk_output.Substring(startindex + 12, endindex - startindex - 14);

Console.WriteLine(list_disk_output_trim);
Console.Write("Select a disk (Number):");
string? select_disk = Console.ReadLine();

if (select_disk != null)
{
    Console.Write("Select a format (Fat32 for USB drive <32GB  |  exFAT for USB drive >32GB)  |  NTFS for windows drive):");
    string? select_format = Console.ReadLine();

    if (select_format != null)
    {
        p.Start();
        p.StandardInput.WriteLine($"select disk {select_disk}");
        p.StandardInput.WriteLine("clean");
        p.StandardInput.WriteLine("exit");
        string clean_output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();

        if (!clean_output.Contains("Access is denied"))
        {
            p.Start();
            p.StandardInput.WriteLine($"select disk {select_disk}");
            p.StandardInput.WriteLine("clean");
            p.StandardInput.WriteLine("create partition primary");
            p.StandardInput.WriteLine($"format fs={select_format} quick");
            p.StandardInput.WriteLine("active");
            p.StandardInput.WriteLine("assign");
            p.StandardInput.WriteLine("exit");
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            int output_startindex = output.IndexOf("DISKPART>");
            string output_trim = output.Substring(output_startindex);

            Console.WriteLine($"\r\nResult:\r\n{output_trim}");
        }
        else
        {
            Console.WriteLine("Cannot clean (Access is denied) - Trying dd...");

            Process dd = new Process();
            dd.StartInfo.UseShellExecute = false;
            dd.StartInfo.RedirectStandardOutput = true;
            dd.StartInfo.RedirectStandardInput = true;
            dd.StartInfo.FileName = "dd.exe";
            dd.StartInfo.Arguments = $"if=/dev/zero of=\\\\?\\Device\\Harddisk{select_disk}\\Partition0 count=1 bs=4096 --progress";
            dd.Start();
            dd.StandardInput.WriteLine("exit");
            string? dd_output = dd.StandardOutput.ReadToEnd();
            dd.WaitForExit();

            Console.WriteLine("Done\r\nTrying again with DiskPart...");

            p.Start();
            p.StandardInput.WriteLine($"select disk {select_disk}");
            p.StandardInput.WriteLine("clean");
            p.StandardInput.WriteLine("create partition primary");
            p.StandardInput.WriteLine($"format fs={select_format} quick");
            p.StandardInput.WriteLine("active");
            p.StandardInput.WriteLine("assign");
            p.StandardInput.WriteLine("exit");
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            int output_startindex = output.IndexOf("DISKPART>");
            string output_trim = output.Substring(output_startindex);

            Console.WriteLine($"\r\nResult:\r\n{output_trim}");
        }
    }
}
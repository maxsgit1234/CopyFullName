using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security;
using System.Reflection;

namespace CopyFullName
{
    class Program
    {
        // This attribute is REQUIRED for Clipboard access. Too bad Microsoft's  
        // highly-paid engineers couldn't "encapsulate" this implementation detail, so
        // now you and millions of others have to deal with it every.single.time.
        [STAThread] 
        static void Main(string[] args)
        {
            // If we run this with no arguments, that means we are trying to "install" it.
            // If we run it with 1 argument, we just copy that to the clipboard and exit.
            // If we run with 2 or more, something is wrong, so print help info.
            if (args.Length == 0)
                try
                {
                    DoInstall();
                }
                catch (SecurityException e)
                {
                    // 9 times out of 10...I swear to God.
                    Console.WriteLine("Seriously, what did I *just* tell you...");
                    Console.WriteLine("");
                    Console.WriteLine(e);
                    Console.ReadLine();
                }
            else if (args.Length == 1)
                // I like how this is literally the ONLY line in this program that 
                // directly serves the purpose for its existence. The rest is 
                // red-tape and overhead...thanks Obama.
                Clipboard.SetText(args[0]);
            else
                PrintUsage(args);
        }

        // Herein we are going to add some Registry keys that tell Windows Explorer
        // to add a context menu entry for our humble utility. It writes those entries
        // so that the menu will subsequently refer to this currently executing file 
        // as the program to invoke to copy text to the clipboard. Registry access is
        // the reason why we need Administrator privileges.
        private static void DoInstall()
        {
            // Shows the console. Note the present program is not a "Console Application"
            // See Properties -> Application -> Output type. 
            // It is a "Windows Application", whatever that is.
            GetConsole();

            // Fair warning in case someone got here by accident...
            Console.WriteLine("");
            Console.WriteLine("If you want to install this simple utility, press enter.");
            Console.WriteLine("Note that you need to be running as Administrator to continue.");
            Console.ReadLine();

            // We need to modify registry values in the same fashion, 
            // but in different root locations in the registry tree 
            // for files and folders, respectively.
            AddReg("*", "Copy full file name");
            AddReg("Directory", "Copy full folder name");

            // If execution got this far, the hard part's over.
            Console.WriteLine("Installation completed successfully.");
            Console.WriteLine("");

            Console.WriteLine("Right click on any file in Windows Explorer and you will ");
            Console.WriteLine("now see another option to 'Copy full file/folder name'");
            Console.WriteLine("");
            Console.WriteLine("NOTE: If you move this .exe file to another folder ");
            Console.WriteLine("location, you need to re-run this installation.");
            Console.ReadLine();
        }

        // These are the magical Registry keys needed to get our entry to show up in the
        // right-click (aka "context" menu) in Windows Explorer.
        private static void AddReg(string root, string text)
        {
            RegistryKey k1 = Registry.ClassesRoot.OpenSubKey(root + "\\shell", true);
            
            // This name is arbitrary, so long as nothing else is already using it to
            // mean something else:
            RegistryKey k2 = k1.CreateSubKey("copyfullname");

            // Supplies the human-readable text in the menu:
            k2.SetValue("MUIVerb", text, RegistryValueKind.String);

            // Establish this program as the one to invoke. '%1' refers to the 
            // name of the file or folder that was right-clicked on. 
            RegistryKey k3 = k2.CreateSubKey("command");
            k3.SetValue(null, Environment.CurrentDirectory + "\\CopyFullName.exe %1");
        }

        // Over the top? Bite me.
        private static void PrintUsage(string[] args)
        {
            // So that the user can see these messages at all... see previous comment.
            GetConsole();

            Console.WriteLine("Unexpected input. Was expecting either: ");
            Console.WriteLine("-- 0 arguments (to run in installation mode) or");
            Console.WriteLine("-- 1 argument (assumed filename to be copied to clipboard)");
            Console.WriteLine("But no. Instead, you provided: " + args.Length + " arguments.");
            for (int i = 2; i < args.Length; i++)
                Console.WriteLine("...Not " + i);
            if (args.Length > 2)
                Console.WriteLine("...but " + args.Length + " arguments. Wow.");
            Console.WriteLine("");
            Console.WriteLine("Press enter to exit and try again.");
            Console.ReadLine();
        }

        // I consider everything beyond this point to be a black-box. 
        // Don't know; don't care how it works. It's arcane, esoteric, and it offers 
        // little or no value in enriching future thought on this or any other topic.
        // It's unabashedly stolen from an unknown, faceless, nameless source online.
        private static void GetConsole()
        {
            IntPtr ptr = GetForegroundWindow();

            int u;

            GetWindowThreadProcessId(ptr, out u);

            Process process = Process.GetProcessById(u);

            if (process.ProcessName == "cmd")
                AttachConsole(process.Id);
            else
                AllocConsole();
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeConsole();

        [DllImport("kernel32", SetLastError = true)]
        static extern bool AttachConsole(int dwProcessId);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);
    }
}

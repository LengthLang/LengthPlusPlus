using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

[assembly:AssemblyVersionAttribute("1.0.0")]

namespace Nezbednik {
    public class Program {
        private static string HelpPrint(string command, string description, int size = 25) {
            return "  " + command + new String(' ', size - command.Length) + description;
        }

        private static string GetTempDir() {
            bool isUnique = false;
            string foldname = "";
            while (isUnique == false) {
                foldname = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetTempFileName()) + new Random().Next(0, 25000));
                if (!Directory.Exists(foldname)) isUnique = true;
            }
            Directory.CreateDirectory(foldname);
            return foldname;
        }

        private static bool IsWin(string x = "x") {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }

        private static void IfWinPrint() {
            if (!IsWin()) Console.WriteLine();
        }

        public static void Main(string[] args) {
            string stamp = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
            if (IsWin()) stamp += ".exe";
            List<string> presets = new List<string>{"vs2010", "g++"};

            // modify this!
            string version = "1.0.1";
            string buildver = "x86_64-linux";
            string buildauthor = "nezbednik";

            if (Array.IndexOf(args, "--help") != -1) {
                Console.WriteLine("Usage: " + stamp + " [options] file...");
                Console.WriteLine("Options:");
                Console.WriteLine(HelpPrint("--help", "Display this information."));
                Console.WriteLine(HelpPrint("--version", "Display compiler version information."));
                Console.WriteLine(HelpPrint("-o <file>", "Place the output into <file>."));
                Console.WriteLine(HelpPrint("--preset <preset>", "Values: " + String.Join(", ", presets.ToArray())));
                IfWinPrint();
                Environment.Exit(0);
            }

            if (Array.IndexOf(args, "--version") != -1) {
                Console.WriteLine(Path.GetFileNameWithoutExtension(stamp) + " (" + buildver + ", Built by " + buildauthor + ") " + version);
                Console.WriteLine();
                IfWinPrint();
                Environment.Exit(0);
            }

            bool printError = false;
            string customOutFile = "";
            int z = Array.IndexOf(args, "-o");
            if (z != -1) {
                z++;
                if (args.Length <= z) {
                    Console.Write(stamp + ":");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(" error: ");
                    Console.ResetColor();
                    Console.Write("missing filename after '");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("-o");
                    Console.ResetColor();
                    Console.WriteLine("'");
                    printError = true;
                }
                else customOutFile = args[z];
            }
            string[] input = Array.FindAll(args, x => (!x.StartsWith("-") && x.EndsWith(".len")));
            string specializedError = "no";
            if (input.Length != 1) printError = true;
            if (input.Length > 1) specializedError = "multiple";
            string filename = "";
            if (!printError) filename = input[0];

            string preset = "";
            int pres = Array.IndexOf(args, "--preset");
            if (pres != -1) {
                pres++;
                string preseterror = "";

                if (args.Length <= pres) preseterror = "missing";
                else if (!presets.Contains(args[pres])) preseterror = "invalid";

                if (preseterror.Length > 0) {
                    Console.Write(stamp + ":");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(" error: ");
                    Console.ResetColor();
                    Console.Write(preseterror + " preset name after '");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("--preset");
                    Console.ResetColor();
                    Console.WriteLine("'");
                    printError = true;
                }
                else preset = args[pres];
            }

            if (!File.Exists(filename)) printError = true;

            if (printError) {
                Console.Write(stamp + ":");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(" fatal error: ");
                Console.ResetColor();
                Console.WriteLine(specializedError + " input files");
                Console.Write("compilation terminated.");
                IfWinPrint();
                Environment.Exit(1);
            }

            List<int> program = new List<int>();
            List<int> included_instructions = new List<int>();

            foreach (string line in File.ReadAllLines(filename)) {
                program.Add(line.Length);
                if (!included_instructions.Contains(line.Length)) included_instructions.Add(line.Length);
            }

            string fileContent = "";
            if (preset == "vs2010") {
                fileContent += "#include <SDKDDKVer.h>\n";
                fileContent += "#include \"tchar.h\"\n";
                fileContent += "#include <string>\n";
            }
            fileContent += "#include <stack>\n";
            fileContent += "#include <iostream>\n\n";
            fileContent += "using namespace std;\n\n";

            if (preset != "vs2010") fileContent += "int main() {\n";
            else fileContent += "int _tmain(int argc, _TCHAR* argv[]) {\n";

            fileContent += "    int program[] = {" + String.Join(", ", program.ToArray()) + "};\n";
            fileContent += "    stack <int> a;\n";
            fileContent += "    for (int x = 0; x < (sizeof(program) / sizeof(program[0])); x++) {\n";
            fileContent += "        switch (program[x]) {\n";

            if (included_instructions.Contains(9)) {
                fileContent += "            case 9:\n";
                fileContent += "            {\n";
                fileContent += "                string input;\n";
                fileContent += "                cout << \"Input:\" << endl << \"\";\n";
                if (preset == "vs2010") fileContent += "                getline(cin, input);";
                else fileContent += "                cin >> input;\n";
                fileContent += "                a.push((int)input[0]);\n";
                fileContent += "                break;\n";
                fileContent += "            }\n";
            }
            if (included_instructions.Contains(10)) {
                fileContent += "            case 10:\n";
                fileContent += "            {\n";
                fileContent += "                int x = a.top();\n";
                fileContent += "                a.pop();\n";
                fileContent += "                int y = a.top();\n";
                fileContent += "                a.pop();\n";
                fileContent += "                a.push(x + y);\n";
                fileContent += "                break;\n";
                fileContent += "            }\n";
            }
            if (included_instructions.Contains(11)) {
                fileContent += "            case 11:\n";
                fileContent += "            {\n";
                fileContent += "                int x = a.top();\n";
                fileContent += "                a.pop();\n";
                fileContent += "                int y = a.top();\n";
                fileContent += "                a.pop();\n";
                fileContent += "                a.push(y - x);\n";
                fileContent += "                break;\n";
                fileContent += "            }\n";
            }
            if (included_instructions.Contains(12)) {
                fileContent += "            case 12:\n";
                fileContent += "            {\n";
                fileContent += "                a.push(a.top());\n";
                fileContent += "                break;\n";
                fileContent += "            }\n";
            }
            if (included_instructions.Contains(13)) {
                fileContent += "            case 13:\n";
                fileContent += "            {\n";
                fileContent += "                int y = a.top();\n";
                fileContent += "                a.pop();\n";
                fileContent += "                if (y == 0) {\n";
                fileContent += "                    x++;\n";
                fileContent += "                    if (program[x] == 14 || program[x] == 25) x++;\n";
                fileContent += "                }\n";
                fileContent += "                break;\n";
                fileContent += "            }\n";
            }
            if (included_instructions.Contains(14)) {
                fileContent += "            case 14:\n";
                fileContent += "            {\n";
                fileContent += "                x = program[x + 1] - 1;\n";
                fileContent += "                break;\n";
                fileContent += "            }\n";
            }
            if (included_instructions.Contains(15)) {
                fileContent += "            case 15:\n";
                fileContent += "            {\n";
                fileContent += "                int x = a.top();\n";
                fileContent += "                a.pop();\n";
                fileContent += "                cout << x;\n";
                fileContent += "                break;\n";
                fileContent += "            }\n";
            }
            if (included_instructions.Contains(16)) {
                fileContent += "            case 16:\n";
                fileContent += "            {\n";
                fileContent += "                int x = a.top();\n";
                fileContent += "                a.pop();\n";
                fileContent += "                cout << char(x);\n";
                fileContent += "                break;\n";
                fileContent += "            }\n";
            }
            if (included_instructions.Contains(20)) {
                fileContent += "            case 20:\n";
                fileContent += "            {\n";
                fileContent += "                int x = a.top();\n";
                fileContent += "                a.pop();\n";
                fileContent += "                int y = a.top();\n";
                fileContent += "                a.pop();\n";
                fileContent += "                a.push(x * y);\n";
                fileContent += "                break;\n";
                fileContent += "            }\n";
            }
            if (included_instructions.Contains(21)) {
                fileContent += "            case 21:\n";
                fileContent += "            {\n";
                fileContent += "                int x = a.top();\n";
                fileContent += "                a.pop();\n";
                fileContent += "                int y = a.top();\n";
                fileContent += "                a.pop();\n";
                fileContent += "                a.push(y / x);\n";
                fileContent += "                break;\n";
                fileContent += "            }\n";
            }
            if (included_instructions.Contains(25)) {
                fileContent += "            case 25:\n";
                fileContent += "            {\n";
                fileContent += "                a.push(program[x + 1]);\n";
                fileContent += "                x++;\n";
                fileContent += "                break;\n";
                fileContent += "            }\n";
            }
            fileContent += "        }\n";
            fileContent += "    }\n";
            fileContent += "    \n";
            if (!IsWin()) fileContent += "    cout << endl;\n";
            fileContent += "    return 0;\n";
            fileContent += "}";
            if (!IsWin()) fileContent += "\n";

            bool isCompiling = false;
            if (preset.Length > 0) isCompiling = true;
            if (customOutFile.Length <= 0) customOutFile = Path.GetFileNameWithoutExtension(filename) + (isCompiling ? (IsWin() ? ".exe" : "") : ".cpp");
            string tempdir = GetTempDir();
            string filenameout = Path.Combine(tempdir, "file.cpp");
            if (!isCompiling) filenameout = customOutFile;
            File.WriteAllText(filenameout, fileContent);
            if (!isCompiling) {
                IfWinPrint();
                Environment.Exit(0);
            }
            if (preset == "vs2010") {
                string vs2path = Path.Combine("C:\\", "Program Files (x86)", "Microsoft Visual Studio 10.0");
                string sdkpath = Path.Combine("C:\\", "Program Files (x86)", "Microsoft SDKs", "Windows", "v7.0A");
                string vc2include = Path.Combine(vs2path, "VC", "include");
                string sdkinclude = Path.Combine(sdkpath, "Include");
                string vc2010path = Path.Combine(vs2path, "Common7", "IDE");
                string vc2libfold = Path.Combine(vs2path, "VC", "lib");
                string sdklibfold = Path.Combine(sdkpath, "Lib");
                string compiler = Path.Combine(vs2path, "VC", "bin", "cl.exe");
                string errortext = "";
                if (!Directory.Exists(vs2path) || !Directory.Exists(vc2010path) || !Directory.Exists(vc2libfold) || !File.Exists(compiler)) errortext = "Visual Studio 2010";
                else if (!Directory.Exists(sdkpath) || !Directory.Exists(sdkinclude) || !Directory.Exists(sdklibfold)) errortext = "Windows SDK";
                if (errortext.Length > 0) {
                    Console.WriteLine(errortext + " is not installed!");
                    IfWinPrint();
                    Environment.Exit(1);
                }
                ProcessStartInfo info = new ProcessStartInfo();
                info.EnvironmentVariables["INCLUDE"] = String.Join(";", new List<string>{vc2include, sdkinclude}.ToArray());
                info.EnvironmentVariables["PATH"] = vc2010path;
                info.EnvironmentVariables["LIB"] = String.Join(";", new List<string>{vc2libfold, sdklibfold}.ToArray());
                info.UseShellExecute = false;
                info.RedirectStandardInput = true;
                info.RedirectStandardError = true;
                info.RedirectStandardOutput = true;
                info.FileName = compiler;
                info.Arguments = "/EHsc /Fo\"" + tempdir + "\" /Fe\"" + (Path.IsPathRooted(customOutFile) ? customOutFile : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, customOutFile)) + "\" \"" + filenameout + "\" /MT";
                Process.Start(info).WaitForExit();
                Directory.Delete(tempdir, true);
                IfWinPrint();
                Environment.Exit(0);
            }
            else if (preset == "g++") {
                try {
                    ProcessStartInfo info = new ProcessStartInfo();
                    info.UseShellExecute = false;
                    info.RedirectStandardInput = true;
                    info.RedirectStandardError = true;
                    info.RedirectStandardOutput = true;
                    info.FileName = "g++";
                    info.Arguments = "-o \"" + (Path.IsPathRooted(customOutFile) ? customOutFile : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, customOutFile)) + "\" \"" + filenameout + "\"";
                    Process.Start(info).WaitForExit();
                    Directory.Delete(tempdir, true);
                    IfWinPrint();
                    Environment.Exit(0);
                }
                catch(Exception e) {
                    IsWin(e.Message);
                    IfWinPrint();
                    Environment.Exit(1);
                }
            }
        }
    }
}

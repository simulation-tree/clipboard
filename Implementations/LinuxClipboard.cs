using System.Diagnostics;
using System.IO;
using System.Text;

namespace Clipboard.Implementations
{
    public unsafe readonly partial struct LinuxClipboard : IClipboardImplementation
    {
        //todo: find a way to interact with linux clipboard without spawning bash commands
        static string? IClipboardImplementation.GetText()
        {
            string tempFileName = Path.GetTempFileName();
            Run($"xsel -o --clipboard  > {tempFileName}");
            string text = File.ReadAllText(tempFileName);
            File.Delete(tempFileName);
            return text;
        }

        static void IClipboardImplementation.SetText(string text)
        {
            string tempFileName = Path.GetTempFileName();
            File.WriteAllText(tempFileName, text);
            Run($"cat {tempFileName} | xsel -i --clipboard ");
            File.Delete(tempFileName);
        }

        private static string Run(string commandLine)
        {
            StringBuilder errorBuilder = new();
            StringBuilder outputBuilder = new();
            string arguments = $"-c \"{commandLine}\"";
            using Process process = new()
            {
                StartInfo = new()
                {
                    FileName = "bash",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                }
            };

            process.Start();
            process.OutputDataReceived += (_, args) => { outputBuilder.AppendLine(args.Data); };
            process.BeginOutputReadLine();
            process.ErrorDataReceived += (_, args) => { errorBuilder.AppendLine(args.Data); };
            process.BeginErrorReadLine();
            if (!DoubleWaitForExit(process))
            {
                throw new($"Process timed out. Command line: bash {arguments}.\nOutput: {outputBuilder}\nError: {errorBuilder}");
            }

            if (process.ExitCode == 0)
            {
                return outputBuilder.ToString();
            }

            throw new($"Could not execute process. Command line: bash {arguments}.\nOutput: {outputBuilder}\nError: {errorBuilder}");
        }

        private static bool DoubleWaitForExit(Process process)
        {
            //to work around https://github.com/dotnet/runtime/issues/27128
            bool result = process.WaitForExit(500);
            if (result)
            {
                process.WaitForExit();
            }

            return result;
        }
    }
}
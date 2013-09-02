using System;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using System.Management;
using System.Collections.Generic;
using System.Runtime.InteropServices;

//*********************************************************************************************
//  Author:  Dave Kerr                                                                          
//  Country: United Kingdom                                                                     
//  Source:  http://www.codeproject.com/Articles/335909/Embedding-a-Console-in-a-C-Application  
//*********************************************************************************************
namespace xelvor.Core
{
    /// <summary>
    /// A ProcessEventHandler is a delegate for process input/output events.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="ProcessInterface.ProcessEventArgs"/> instance containing the event data.</param>
    public delegate void ProcessEventHanlder(object sender, ProcessEventArgs args);

    /// <summary>
    /// A class the wraps a process, allowing programmatic input and output.
    /// </summary>
    public class ProcessInterface
    {
        #region Variables

        /// <summary>
        /// The current process.
        /// </summary>
        private Process process;
        
        /// <summary>
        /// The input writer.
        /// </summary>
        private StreamWriter inputWriter;
        
        /// <summary>
        /// The output reader.
        /// </summary>
        private TextReader outputReader;
        
        /// <summary>
        /// The error reader.
        /// </summary>
        private TextReader errorReader;
        
        /// <summary>
        /// The output worker.
        /// </summary>
        private BackgroundWorker outputWorker = new BackgroundWorker();
        
        /// <summary>
        /// The error worker.
        /// </summary>
        private BackgroundWorker errorWorker = new BackgroundWorker();

        /// <summary>
        /// Message Stack Worker
        /// </summary>
        private BackgroundWorker mqWorker = new BackgroundWorker();

        /// <summary>
        /// Output Queue
        /// </summary>
        private Queue<MessagePackage> msgQueue = new Queue<MessagePackage>();

        /// <summary>
        /// Current process file name.
        /// </summary>
        private string processFileName;

        /// <summary>
        /// Arguments sent to the current process.
        /// </summary>
        private string processArguments;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance is process running.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is process running; otherwise, <c>false</c>.
        /// </value>
        public bool IsProcessRunning
        {
            get
            {
                try
                {
                    return (process != null && process.HasExited == false);
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the internal process.
        /// </summary>
        public Process Process
        {
            get { return process; }
        }

        /// <summary>
        /// Gets the name of the process.
        /// </summary>
        /// <value>
        /// The name of the process.
        /// </value>
        public string ProcessFileName
        {
            get { return processFileName; }
        }

        /// <summary>
        /// Gets the process arguments.
        /// </summary>
        public string ProcessArguments
        {
            get { return processArguments; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessInterface"/> class.
        /// </summary>
        public ProcessInterface()
        {
            //  Configure the output worker.
            outputWorker.WorkerReportsProgress = true;
            outputWorker.WorkerSupportsCancellation = true;
            outputWorker.DoWork += new DoWorkEventHandler(outputWorker_DoWork);
            outputWorker.ProgressChanged += new ProgressChangedEventHandler(outputWorker_ProgressChanged);

            //  Configure the error worker.
            errorWorker.WorkerReportsProgress = true;
            errorWorker.WorkerSupportsCancellation = true;
            errorWorker.DoWork += new DoWorkEventHandler(errorWorker_DoWork);
            errorWorker.ProgressChanged += new ProgressChangedEventHandler(errorWorker_ProgressChanged);
        }

        #endregion

        #region Private Events

        /// <summary>
        /// Handles the ProgressChanged event of the outputWorker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.ProgressChangedEventArgs"/> instance containing the event data.</param>
        void outputWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //  We must be passed a string in the user state.
            if (e.UserState is string)
            {
                //  Fire the output event.
                //FireProcessOutputEvent(e.UserState as string);

                msgQueue.Enqueue(new MessagePackage() { Message = e.UserState.ToString(), Type = MessageType.Message});
            }
        }

        /// <summary>
        /// Handles the DoWork event of the outputWorker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.DoWorkEventArgs"/> instance containing the event data.</param>
        void outputWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (outputWorker.CancellationPending == false)
            {
                //  Any lines to read?
                int count = 0;
                char[] buffer = new char[1024];

                do
                {
                    StringBuilder builder = new StringBuilder();
                    count = outputReader.Read(buffer, 0, 1024);
                    builder.Append(buffer, 0, count);
                    outputWorker.ReportProgress(0, builder.ToString());

                    // for debugging...
                    System.Diagnostics.Debug.Write(builder.ToString());
                } while (count > 0);

                System.Threading.Thread.Sleep(200);
            }
        }

        /// <summary>
        /// Handles the ProgressChanged event of the errorWorker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.ProgressChangedEventArgs"/> instance containing the event data.</param>
        void errorWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //  The userstate must be a string.
            if (e.UserState is string)
            {
                //  Fire the error event.
                //FireProcessErrorEvent(e.UserState as string);

                msgQueue.Enqueue(new MessagePackage() { Message = e.UserState.ToString(), Type = MessageType.Error });
            }
        }

        /// <summary>
        /// Handles the DoWork event of the errorWorker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.DoWorkEventArgs"/> instance containing the event data.</param>
        void errorWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (errorWorker.CancellationPending == false)
            {
                //  Any lines to read?
                int count = 0;
                char[] buffer = new char[1024];
                do
                {
                    StringBuilder builder = new StringBuilder();
                    count = errorReader.Read(buffer, 0, 1024);
                    builder.Append(buffer, 0, count);
                    errorWorker.ReportProgress(0, builder.ToString());

                    // for debugging...
                    System.Diagnostics.Debug.Write(builder.ToString());
                } while (count > 0);

                System.Threading.Thread.Sleep(200);
            }
        }

        /// <summary>
        /// Handles the Exited event of the currentProcess control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void currentProcess_Exited(object sender, EventArgs e)
        {
            //  Fire process exited.
            FireProcessExitEvent(process.ExitCode);

            //  Disable the threads.
            outputWorker.CancelAsync();
            errorWorker.CancelAsync();
            inputWriter = null;
            outputReader = null;
            errorReader = null;
            process = null;
            processFileName = null;
            processArguments = null;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Runs a process.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="arguments">The arguments.</param>
        public void StartProcess(string fileName, string arguments)
        {
            //  Create the process start info.
            var processStartInfo = new ProcessStartInfo(fileName, arguments);

            //  Set the options.
            processStartInfo.UseShellExecute = false;
            processStartInfo.ErrorDialog = false;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            //  Specify redirection.
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.RedirectStandardOutput = true;

            //  Create the process.
            process = new Process();
            process.EnableRaisingEvents = true;
            process.StartInfo = processStartInfo;
            process.Exited += new EventHandler(currentProcess_Exited);

            //  Start the process.
            try
            {
                bool processStarted = process.Start();
            }
            catch (Exception e)
            {
                //  Trace the exception.
                Trace.WriteLine("Failed to start process " + fileName + " with arguments '" + arguments + "'");
                Trace.WriteLine(e.ToString());
                return;
            }

            //  Store name and arguments.
            processFileName = fileName;
            processArguments = arguments;

            //  Create the readers and writers.
            inputWriter = process.StandardInput;
            outputReader = TextReader.Synchronized(process.StandardOutput);
            errorReader = TextReader.Synchronized(process.StandardError);

            //  Run the workers that read output and error.
            outputWorker.RunWorkerAsync();
            errorWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Stops the process.
        /// </summary>
        public void StopProcess()
        {
            //  Handle the trivial case.
            if (IsProcessRunning == false)
                return;

            //  Kill the process.
            process.Kill();
        }

        public List<int> GetChildProcessIds()
        {
            List<int> procList = new List<int>();
            Process[] procs = Process.GetProcesses();

            ManagementObjectSearcher objOSDetails = new ManagementObjectSearcher("SELECT * FROM Win32_Process WHERE ParentProcessId = " + process.Id);
            foreach (ManagementObject mo in objOSDetails.Get())
            {
                procList.Add(Convert.ToInt32(mo["Handle"]));
            }

            return procList;
        }

        public void KillProcess(int pid)
        {
            //try
            //{
            //    Process.GetProcessById(pid).Kill();
            //}
            //catch
            //{
            //}

            //This does not require the console window to be visible.
            if (AttachConsole((uint)pid))
            {
                //Disable Ctrl-C handling for our program
                SetConsoleCtrlHandler(null, true);
                GenerateConsoleCtrlEvent(CtrlTypes.CTRL_C_EVENT, 0);

                //Must wait here. If we don't and re-enable Ctrl-C handling below too fast, we might terminate ourselves.
                Process.GetProcessById(pid).WaitForExit();

                FreeConsole();

                //Re-enable Ctrl-C handling or any subsequently started programs will inherit the disabled state.
                SetConsoleCtrlHandler(null, false);
            }

        }

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate HandlerRoutine, bool Add);
        delegate Boolean ConsoleCtrlDelegate(CtrlTypes CtrlType);
        enum CtrlTypes : uint
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GenerateConsoleCtrlEvent(CtrlTypes dwCtrlEvent, uint dwProcessGroupId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();

        #endregion

        #region Private Methods

        /// <summary>
        /// Fires the process output event.
        /// </summary>
        /// <param name="content">The content</param>
        /// <param name="error">Type of the content, message, error</param>
        private void FireProcessOutputEvent(string content, bool error)
        {
        }

        /// <summary>
        /// Fires the process output event.
        /// </summary>
        /// <param name="content">The content.</param>
        private void FireProcessOutputEvent(string content)
        {
            //  Get the event and fire it.
            var theEvent = OnProcessOutput;
            if (theEvent != null)
                theEvent(this, new ProcessEventArgs(content));
        }

        /// <summary>
        /// Fires the process error output event.
        /// </summary>
        /// <param name="content">The content.</param>
        private void FireProcessErrorEvent(string content)
        {
            //  Get the event and fire it.
            var theEvent = OnProcessError;
            if (theEvent != null)
                theEvent(this, new ProcessEventArgs(content));
        }

        /// <summary>
        /// Fires the process input event.
        /// </summary>
        /// <param name="content">The content.</param>
        private void FireProcessInputEvent(string content)
        {
            //  Get the event and fire it.
            var theEvent = OnProcessInput;
            if (theEvent != null)
                theEvent(this, new ProcessEventArgs(content));
        }

        /// <summary>
        /// Fires the process exit event.
        /// </summary>
        /// <param name="code">The code.</param>
        private void FireProcessExitEvent(int code)
        {
            //  Get the event and fire it.
            var theEvent = OnProcessExit;
            if (theEvent != null)
                theEvent(this, new ProcessEventArgs(code));
        }

        /// <summary>
        /// Writes the input.
        /// </summary>
        /// <param name="input">The input.</param>
        public void WriteInput(string input)
        {
            if (IsProcessRunning)
            {
                inputWriter.WriteLine(input);
                inputWriter.Flush();
            }
        }

        #endregion

        #region Public Event Definition

        /// <summary>
        /// Occurs when process output is produced.
        /// </summary>
        public event ProcessEventHanlder OnProcessOutput;

        /// <summary>
        /// Occurs when process error output is produced.
        /// </summary>
        public event ProcessEventHanlder OnProcessError;

        /// <summary>
        /// Occurs when process input is produced.
        /// </summary>
        public event ProcessEventHanlder OnProcessInput;

        /// <summary>
        /// Occurs when the process ends.
        /// </summary>
        public event ProcessEventHanlder OnProcessExit;

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Runtime.InteropServices;

namespace NamedPipesHacker
{

    class NamedPipesHacker
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle CreateNamedPipe(
           String pipeName,
           uint dwOpenMode,
           uint dwPipeMode,
           uint nMaxInstances,
           uint nOutBufferSize,
           uint nInBufferSize,
           uint nDefaultTimeOut,
           IntPtr lpSecurityAttributes);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int ConnectNamedPipe(
           SafeFileHandle hNamedPipe,
           IntPtr lpOverlapped);


        public const uint DUPLEX = (0x00000003);
        public const uint FILE_FLAG_OVERLAPPED = (0x40000000);

        // In Windows Named Pipes have this naming convention.
        public const string PIPE_NAME = "\\\\.\\pipe\\tran.dat";
        public const uint BUFFER_SIZE = 4096;

        private FileStream fStream = null;
        SafeFileHandle clientPipeHandle;

        /// <summary>
        /// Server Listen method.  It creates a NamedPipe, it Reads a Message and Write a response.
        /// </summary>
        private void Listen()
        {
            for(int iclients=0;iclients<1;iclients++)
            {
                // Creates a synchronous named pipe of 255 buffer size.
                clientPipeHandle = CreateNamedPipe(
                   PIPE_NAME,
                   DUPLEX | FILE_FLAG_OVERLAPPED,
                   0,
                   255,
                   BUFFER_SIZE,
                   BUFFER_SIZE,
                   0,
                   IntPtr.Zero);

                //failed to create named pipe
                if (clientPipeHandle.IsInvalid)
                {
                    Console.WriteLine("Failed to created named pipe");
                    break;
                }

                Console.WriteLine("Named pipe has been created");

                int success = ConnectNamedPipe(
                   clientPipeHandle,
                   IntPtr.Zero);

                //failed to connect client pipe
                if (success != 1)
                {
                    Console.WriteLine("Failed to connect server pipe");
                    break;
                }

                Console.WriteLine("Connected to " + PIPE_NAME);

                fStream =
                   new FileStream(clientPipeHandle, FileAccess.ReadWrite, (int)BUFFER_SIZE, true);

                // The client is now connected, read something and write something
                Read();
                Write("/TDETAIL");
            }
        }

        /// <summary>
        /// Read method from the pipe
        /// </summary>
        private void Read()
        {
            byte[] buffer = new byte[BUFFER_SIZE];
            ASCIIEncoding encoder = new ASCIIEncoding();

            Console.WriteLine("Reading....");

            for (int i=0;i<1;i++)
            {
                int bytesRead = fStream.Read(buffer, 0, (int)BUFFER_SIZE);

                //could not read from file stream
                if (bytesRead == 0)
                {
                    Console.WriteLine("Could not read from the pipeline");
                    break;
                }

                Console.WriteLine("Message:" + encoder.GetString(buffer,0,bytesRead));

            }
        }

        /// <summary>
        /// Write Message to fStream (which needs to be already connected to the pipe)
        /// </summary>
        /// <param name="message"></param>
        private void Write(string message)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] sendBuffer = encoder.GetBytes(message);
            fStream.Write(sendBuffer, 0, sendBuffer.Length);
            fStream.Flush();
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle CreateFile(
           String pipeName,
           uint dwDesiredAccess,
           uint dwShareMode,
           IntPtr lpSecurityAttributes,
           uint dwCreationDisposition,
           uint dwFlagsAndAttributes,
           IntPtr hTemplate);


        /// <summary>
        /// Open an Existing Named Pipe, write something into it and get a response.
        /// </summary>
        public void clientMethod()
        {
            Console.WriteLine("Client Running...");

            uint GENERIC_READ = (0x80000000);
            uint GENERIC_WRITE = (0x40000000);
            uint OPEN_EXISTING = 3;
            uint FILE_FLAG_OVERLAPPED = (0x40000000);

            SafeFileHandle pipeHandle =
               CreateFile(
                  PIPE_NAME,
                  GENERIC_READ | GENERIC_WRITE,
                  0,
                  IntPtr.Zero,
                  OPEN_EXISTING,
                  FILE_FLAG_OVERLAPPED,
                  IntPtr.Zero);

            //could not get a handle to the named pipe
            if (pipeHandle.IsInvalid)
            {
                Console.WriteLine("Could not get a handle to the named pipe:" + PIPE_NAME);
                return;
            }

            fStream =
               new FileStream(pipeHandle, FileAccess.ReadWrite, (int)BUFFER_SIZE, true);

            Write("/TTDETAIL /MID=001 /PID=0001<0x1C>1,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,GVR,080001_D,,,,");
            Read();
 
        }

        /// <summary>
        /// What the server is doing....
        /// </summary>
        public void serverMethod()
        {
            Console.WriteLine("Server Running...");
            Listen();
        }

        /// <summary>
        /// Console app.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            if (args.Length>0 && args[0].Equals("server")) 
                (new NamedPipesHacker()).serverMethod();
            else 
                (new NamedPipesHacker()).clientMethod();
        }
    }
}

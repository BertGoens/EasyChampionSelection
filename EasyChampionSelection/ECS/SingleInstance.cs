using System;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace EasyChampionSelection.ECS {
    public class SingleInstance {

        private bool _isFirstInstance = false;
        private Semaphore sema;

        /// <summary>
        /// Occurs when [arguments received].
        /// </summary>
        public event EventHandler<GenericEventArgs<string>> ArgumentsReceived;

        /// <summary>
        /// Gets a value indicating whether this instance is first instance.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is first instance; otherwise, <c>false</c>.
        /// </value>
        public bool IsFirstInstance {
            get { return _isFirstInstance; }
            private set { _isFirstInstance = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleInstance"/> class.
        /// </summary>
        public SingleInstance() {
            //Check for duplicates running trough a semaphore
            string semaLock = getSemaLockName();
            bool result = Semaphore.TryOpenExisting(semaLock, out sema);
            if(!result) { //This is a unique / first instance
                try {
                    sema = new Semaphore(1, 1, semaLock);
                } catch {
                    App.Current.Shutdown(); //Errored out
                }
            }

            if(sema.WaitOne(0)) {
                IsFirstInstance = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            if(sema != null && IsFirstInstance) {
                sema.Release();
                sema.Dispose();
                sema = null;
            }
        }

        /// <summary>
        /// Passes the arguments to first instance.
        /// </summary>
        /// <param name="argument">The argument.</param>
        public void PassArgumentsToFirstInstance(string argument) {
            using(var client = new NamedPipeClientStream(getSemaLockName()))
            using(var writer = new StreamWriter(client)) {
                client.Connect(200);
                writer.WriteLine(argument);
            }
        }

        /// <summary>
        /// Listens for arguments from successive instances.
        /// </summary>
        public void ListenForArgumentsFromSuccessiveInstances() {
            Task.Factory.StartNew(() => {

                using(var server = new NamedPipeServerStream(getSemaLockName()))
                using(var reader = new StreamReader(server)) {
                    while(true) {
                        server.WaitForConnection();

                        var argument = string.Empty;
                        while(server.IsConnected) {
                            argument += reader.ReadLine();
                        }

                        CallOnArgumentsReceived(argument);
                        server.Disconnect();
                    }
                }
            });
        }

        /// <summary>
        /// Calls the on arguments received.
        /// </summary>
        /// <param name="state">The state.</param>
        public void CallOnArgumentsReceived(object state) {
            if(ArgumentsReceived != null) {
                if(state == null) {
                    state = string.Empty;
                }

                ArgumentsReceived(this, new GenericEventArgs<string>() { Data = state.ToString() });
            }
        }

        private string getSemaLockName() {
            string semaLock = WindowsIdentity.GetCurrent().Name + StaticSerializer.AppName;
            semaLock = semaLock.Replace("\\", ":");
            return semaLock;
        }
    }
}

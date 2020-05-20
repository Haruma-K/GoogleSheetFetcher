using System;
using System.Collections;
using System.Threading.Tasks;

namespace GoogleSheetFetcher.Editor.AsyncControl
{
    public class AsyncControlHandle : IEnumerator
    {
        private readonly IAsyncControl _internalControl;
        
        public AsyncControlHandle(IAsyncControl control)
        {
            _internalControl = control;
        }
        
        /// <summary>
        /// The event that is called at the end of the process.
        /// </summary>
        public event Action Completed
        {
            add => _internalControl.Completed += value;
            remove => _internalControl.Completed -= value;
        }
        /// <summary>
        /// The state of asynchronous processing.
        /// </summary>
        public AsyncControlStatus Status => _internalControl.Status;

        /// <summary>
        /// The exception when the processing failure.
        /// </summary>
        public Exception Exception => _internalControl.Exception;

        /// <summary>
        /// Whether or not the process completed.
        /// </summary>
        public bool IsDone => _internalControl.IsDone;

        /// <summary>
        /// Task
        /// </summary>
        public Task Task => _internalControl.Task;
        
        /// <summary>
        /// The progress of the process.
        /// </summary>
        public float Progress => _internalControl.GetProgress();

        /// <summary>
        /// Cancel the process.
        /// </summary>
        public void Cancel()
        {
            _internalControl.Cancel();
        }
        
        bool IEnumerator.MoveNext() => !IsDone;

        void IEnumerator.Reset() { }

        object IEnumerator.Current => null;
    }
    
    public class AsyncControlHandle<TResult> : IEnumerator
    {
        private readonly AsyncControlBase<TResult> _internalControl;
        
        public AsyncControlHandle(AsyncControlBase<TResult> control)
        {
            _internalControl = control;
        }
        
        /// <summary>
        /// The event that is called at the end of the process.
        /// </summary>
        public event Action Completed
        {
            add => _internalControl.Completed += value;
            remove => _internalControl.Completed -= value;
        }
        /// <summary>
        /// The state of asynchronous processing.
        /// </summary>
        public AsyncControlStatus Status => _internalControl.Status;

        /// <summary>
        /// The exception when the processing failure.
        /// </summary>
        public Exception Exception => _internalControl.Exception;

        /// <summary>
        /// Whether or not the process completed.
        /// </summary>
        public bool IsDone => _internalControl.IsDone;

        /// <summary>
        /// Task
        /// </summary>
        public Task<TResult> Task => _internalControl.Task;
        
        /// <summary>
        /// Result
        /// </summary>
        public TResult Result => _internalControl.Result;
        
        /// <summary>
        /// The progress of the process.
        /// </summary>
        public float Progress => _internalControl.GetProgress();

        /// <summary>
        /// Cancel the process.
        /// </summary>
        public void Cancel()
        {
            _internalControl.Cancel();
        }
        
        bool IEnumerator.MoveNext() => !IsDone;

        void IEnumerator.Reset() { }

        object IEnumerator.Current => null;
    }
}
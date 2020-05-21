using System;

using System.Threading;
using System.Threading.Tasks;

namespace GoogleSheetFetcher.Editor.AsyncControls
{
    public interface IAsyncControl
    {
        /// <summary>
        /// The event that is called at the end of the process.
        /// </summary>
        event Action Completed;
        /// <summary>
        /// The state of asynchronous processing.
        /// </summary>
        AsyncControlStatus Status { get; }
        /// <summary>
        /// The exception when the processing failure.
        /// </summary>
        Exception Exception { get; }
        /// <summary>
        /// Whether or not the process completed.
        /// </summary>
        bool IsDone { get; }
        /// <summary>
        /// Task
        /// </summary>
        Task Task { get; }

        /// <summary>
        /// Get the progress of the process.
        /// </summary>
        float GetProgress();

        /// <summary>
        /// Start processing.
        /// </summary>
        void Start();

        /// <summary>
        /// Cancel processing.
        /// </summary>
        void Cancel();
    }
    
    /// <summary>
    /// A base class for the implementation of asynchronous processing without result values.
    /// </summary>
    public abstract class AsyncControlBase : IAsyncControl
    {
        /// <summary>
        /// The event that is called at the end of the process.
        /// </summary>
        public event Action Completed;
        /// <summary>
        /// The state of asynchronous processing.
        /// </summary>
        public AsyncControlStatus Status { get; private set; }
        /// <summary>
        /// The exception when the processing failure.
        /// </summary>
        public Exception Exception { get; private set; }
        /// <summary>
        /// Whether or not the process completed.
        /// </summary>
        public bool IsDone => Status == AsyncControlStatus.Succeeded || Status == AsyncControlStatus.Failed;
        
        /// <summary>
        /// Task
        /// </summary>
        public Task Task
        {
            get
            {
                switch (Status)
                {
                    case AsyncControlStatus.Failed:
                    case AsyncControlStatus.Succeeded:
                        return Task.CompletedTask;
                    default:
                    {
                        var waitHandle = WaitHandle;
                        return Task.Factory.StartNew(state =>
                        {
                            waitHandle.WaitOne();
                        }, this);
                    }
                }
            }
        }
        
        protected virtual float Progress => 0;
        
        protected CancellationToken CancellationToken => _cancellationTokenSource.Token;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        
        private EventWaitHandle WaitHandle
        {
            get
            {
                if (_waitHandle == null)
                {
                    _waitHandle = new ManualResetEvent(false);
                }
                return _waitHandle;
            }
        }
        
        private EventWaitHandle _waitHandle;
        
        protected void Succeeded()
        {
            Status = AsyncControlStatus.Succeeded;
            Completed?.Invoke();
            WaitHandle?.Set();
        }
        
        protected void Failed(Exception exception)
        {
            Status = AsyncControlStatus.Failed;
            Exception = exception;
            Completed?.Invoke();
            WaitHandle?.Set();
        }
        
        /// <summary>
        /// Get the progress of the process.
        /// </summary>
        public float GetProgress()
        {
            return IsDone ? 1.0f : Progress;
        }

        /// <summary>
        /// Start processing.
        /// </summary>
        public void Start()
        {
            if (Status != AsyncControlStatus.None)
            {
                return;
            }

            Status = AsyncControlStatus.Processing;
            OnStart();
        }

        /// <summary>
        /// Cancel processing.
        /// </summary>
        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        protected abstract void OnStart();
    }
    
    /// <summary>
    /// A base class for the implementation of asynchronous processing.
    /// </summary>
    public abstract class AsyncControlBase<TResult> : IAsyncControl
    {
        /// <summary>
        /// The event that is called at the end of the process.
        /// </summary>
        public event Action Completed;
        /// <summary>
        /// The state of asynchronous processing.
        /// </summary>
        public AsyncControlStatus Status { get; private set; }
        /// <summary>
        /// The exception when the processing failure.
        /// </summary>
        public Exception Exception { get; private set; }
        /// <summary>
        /// The result of the processing.
        /// </summary>
        public TResult Result { get; private set; }
        /// <summary>
        /// Whether or not the process completed.
        /// </summary>
        public bool IsDone => Status == AsyncControlStatus.Succeeded || Status == AsyncControlStatus.Failed;
        
        /// <summary>
        /// Task
        /// </summary>
        public Task<TResult> Task
        {
            get
            {
                switch (Status)
                {
                    case AsyncControlStatus.Failed:
                        return System.Threading.Tasks.Task.FromResult(default(TResult));
                    case AsyncControlStatus.Succeeded:
                        return System.Threading.Tasks.Task.FromResult(Result);
                    default:
                    {
                        var waitHandle = WaitHandle;
                        return System.Threading.Tasks.Task.Factory.StartNew(state =>
                        {
                            waitHandle.WaitOne();
                            return Result;
                        }, this);
                    }
                }
            }
        }

        Task IAsyncControl.Task => Task;
        
        protected virtual float Progress => 0;
        
        protected CancellationToken CancellationToken => _cancellationTokenSource.Token;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        
        private EventWaitHandle WaitHandle
        {
            get
            {
                if (_waitHandle == null)
                {
                    _waitHandle = new ManualResetEvent(false);
                }
                return _waitHandle;
            }
        }
        
        private EventWaitHandle _waitHandle;
        
        protected void Succeeded(TResult result)
        {
            Result = result;
            Status = AsyncControlStatus.Succeeded;
            Completed?.Invoke();
            WaitHandle?.Set();
        }
        
        protected void Failed(Exception exception)
        {
            Status = AsyncControlStatus.Failed;
            Exception = exception;
            Completed?.Invoke();
            WaitHandle?.Set();
        }
        
        /// <summary>
        /// Get the progress of the process.
        /// </summary>
        public float GetProgress()
        {
            return IsDone ? 1.0f : Progress;
        }

        /// <summary>
        /// Start processing.
        /// </summary>
        public void Start()
        {
            if (Status != AsyncControlStatus.None)
            {
                return;
            }

            Status = AsyncControlStatus.Processing;
            OnStart();
        }

        /// <summary>
        /// Cancel processing.
        /// </summary>
        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        protected abstract void OnStart();
    }
}
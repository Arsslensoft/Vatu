using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VTC
{
    /// <summary>
    /// First In First Out Semaphore. A Semaphore which
    /// enforces FIFO behaviour.
    /// </summary>
    public class FIFOSemaphore : Semaphore
    {
        public readonly Queue<Thread> _WaitQueue;

        public FIFOSemaphore(int token)
            : base(token)
        { this._WaitQueue = new Queue<Thread>(); }

        #region Functions

        public override void Acquire()
        {
            bool doWait = false;
            Thread tempThread = null;
            lock (this._Lock)
            {
                if (this._Token > 0)
                { this._Token--; }
                else
                {
                    tempThread = Thread.CurrentThread;
                    this._WaitQueue.Enqueue(tempThread);
                    doWait = true;
                    Monitor.Enter(tempThread);
                }
            }

            if (doWait)
            {
                Monitor.Wait(tempThread);
                Monitor.Exit(tempThread);
            }
        }

        public override void Release()
        {
            lock (this._Lock)
            {
                if (this._WaitQueue.Count > 0)
                {
                    Thread tempThread = this._WaitQueue.Dequeue();
                    lock (tempThread)
                        Monitor.Pulse(tempThread);
                }
                else
                { this._Token++; }
            }
        }
        #endregion
    }


    /// <summary>
    /// Basic Semaphorewith expanded Acquire and Release functionalities.
    /// </summary>
    public class Semaphore : ISync
    {
        protected int _Token;
        protected readonly Object _Lock;

        /// <summary>
        /// Constructor of the semaphore class takes 1 parameter
        /// as initial number of token(s) available.
        /// </summary>
        /// <param name="token">Initial number of token(s).</param>
        public Semaphore(int token)
        {
            this._Lock = new Object();
            this._Token = token;
        }

        #region Functions

        /// <summary>
        /// Acquires the token from this semaphore. This acquire waits
        /// until it gets the token. For timeout acquire, try to look
        /// at TryAcquire().
        /// </summary>
        public virtual void Acquire()
        { this.TryAcquire(-1); }

        /// <summary>
        /// Acquires 1 token from this semaphore. If there is no token
        /// available, it will timeout after a particular time.
        /// </summary>
        /// <param name="ms">Timeout in millisecond.</param>
        /// <returns></returns>
        public virtual bool TryAcquire(int ms)
        {
            double endTime = (System.DateTime.Now.Ticks / 10000) + ms;
            lock (this._Lock)
            {
                for (; ; )
                {
                    if (this._Token > 0)
                    {
                        this._Token--;
                        return true;
                    }

                    if (ms != -1)
                    {
                        double now = System.DateTime.Now.Ticks / 10000;
                        ms = (int)(endTime - now);
                        if (ms <= 0)
                        { return false; }
                    }

                    Monitor.Wait(this._Lock, ms);
                }
            }
        }







        /// <summary>
        /// Releases 1 token from this semaphore
        /// Note : not interruptible, try ForceRelease().
        /// </summary>
        public virtual void Release()
        {
            lock (this._Lock)
            {
                this._Token++;
                Monitor.PulseAll(this._Lock);
            }
        }

        /// <summary>
        /// Releases 1 token from this semaphore, and this works 
        /// with interrupt.
        /// </summary>
        public virtual void ForceRelease()
        {
            bool wasInterrupted = false;
            for (; ; )
            {
                try
                {
                    this.Release();
                    break;
                }
                catch (ThreadInterruptedException)
                {
                    wasInterrupted = true;
                    Thread.Sleep(0);
                }
            }

            if (wasInterrupted)
            { Thread.CurrentThread.Interrupt(); }
        }
        #endregion
    }


    interface ISync
    {
        void Acquire();
        void Release();
    }
}

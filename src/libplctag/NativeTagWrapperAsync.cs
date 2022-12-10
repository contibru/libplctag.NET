// Copyright (c) libplctag.NET contributors
// https://github.com/libplctag/libplctag.NET
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("libplctag.Tests")]

namespace libplctag
{
    class NativeTagWrapperAsync : NativeTagWrapperBase, IDisposable
    {
        private const int TIMEOUT_VALUE_THAT_INDICATES_ASYNC_OPERATION = 0;

        public NativeTagWrapperAsync(INativeTag nativeMethods) : base(nativeMethods) { }

        ~NativeTagWrapperAsync()
        {
            Dispose();
        }

        public async Task InitializeAsync(CancellationToken token = default)
        {
            ThrowIfAlreadyDisposed();
            ThrowIfAlreadyInitialized();

            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                cts.CancelAfter(Timeout);

                using (
                    cts.Token.Register(() =>
                    {
                        if (createTasks.TryPop(out var createTask))
                        {
                            Abort();
                            RemoveEventsAndRemoveCallback();

                            if (token.IsCancellationRequested)
                                createTask.SetCanceled();
                            else
                                createTask.SetException(
                                    new LibPlcTagException(Status.ErrorTimeout)
                                );
                        }
                    })
                )
                {
                    SetUpEvents();

                    var createTask = new TaskCompletionSource<Status>(
                        TaskCreationOptions.RunContinuationsAsynchronously
                    );
                    createTasks.Push(createTask);

                    base.Initialize(TIMEOUT_VALUE_THAT_INDICATES_ASYNC_OPERATION);

                    if (GetStatus() == Status.Pending)
                        await createTask.Task;

                    ThrowIfStatusNotOk(createTask.Task.Result);

                    _isInitialized = true;
                }
            }
        }

        public async Task ReadAsync(CancellationToken token = default)
        {
            ThrowIfAlreadyDisposed();

            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                cts.CancelAfter(Timeout);

                await InitializeAsyncIfRequired(cts.Token);

                using (
                    cts.Token.Register(() =>
                    {
                        if (readTasks.TryPop(out var readTask))
                        {
                            Abort();

                            if (token.IsCancellationRequested)
                                readTask.SetCanceled();
                            else
                                readTask.SetException(new LibPlcTagException(Status.ErrorTimeout));
                        }
                    })
                )
                {
                    var readTask = new TaskCompletionSource<Status>(
                        TaskCreationOptions.RunContinuationsAsynchronously
                    );
                    readTasks.Push(readTask);

                    base.Read(TIMEOUT_VALUE_THAT_INDICATES_ASYNC_OPERATION);

                    await readTask.Task;
                    ThrowIfStatusNotOk(readTask.Task.Result);
                }
            }
        }

        public async Task WriteAsync(CancellationToken token = default)
        {
            ThrowIfAlreadyDisposed();

            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                cts.CancelAfter(Timeout);

                await InitializeAsyncIfRequired(cts.Token);

                using (
                    cts.Token.Register(() =>
                    {
                        if (writeTasks.TryPop(out var writeTask))
                        {
                            Abort();

                            if (token.IsCancellationRequested)
                                writeTask.SetCanceled();
                            else
                                writeTask.SetException(new LibPlcTagException(Status.ErrorTimeout));
                        }
                    })
                )
                {
                    var writeTask = new TaskCompletionSource<Status>(
                        TaskCreationOptions.RunContinuationsAsynchronously
                    );
                    writeTasks.Push(writeTask);
                    base.Write(TIMEOUT_VALUE_THAT_INDICATES_ASYNC_OPERATION);
                    await writeTask.Task;
                    ThrowIfStatusNotOk(writeTask.Task.Result);
                }
            }
        }

        private Task InitializeAsyncIfRequired(CancellationToken token)
        {
            if (!_isInitialized)
                return InitializeAsync(token);
            else
                return Task.CompletedTask;
        }

        void SetUpEvents()
        {
            // Used to finalize the asynchronous read/write task completion sources
            ReadCompleted += ReadTaskCompleter;
            WriteCompleted += WriteTaskCompleter;
            Created += CreatedTaskCompleter;
        }

        void RemoveEventsAndRemoveCallback()
        {
            // Used to finalize the  read/write task completion sources
            ReadCompleted -= ReadTaskCompleter;
            WriteCompleted -= WriteTaskCompleter;
            Created -= CreatedTaskCompleter;
        }

        private readonly ConcurrentStack<TaskCompletionSource<Status>> createTasks =
            new ConcurrentStack<TaskCompletionSource<Status>>();

        void CreatedTaskCompleter(object sender, TagEventArgs e)
        {
            if (createTasks.TryPop(out var createTask))
            {
                switch (e.Status)
                {
                    case Status.Pending:
                        // Do nothing, wait for another ReadCompleted callback
                        break;
                    default:
                        createTask?.SetResult(e.Status);
                        break;
                }
            }
        }

        private readonly ConcurrentStack<TaskCompletionSource<Status>> readTasks =
            new ConcurrentStack<TaskCompletionSource<Status>>();

        void ReadTaskCompleter(object sender, TagEventArgs e)
        {
            if (readTasks.TryPop(out var readTask))
            {
                switch (e.Status)
                {
                    case Status.Pending:
                        // Do nothing, wait for another ReadCompleted callback
                        break;
                    default:
                        readTask?.SetResult(e.Status);
                        break;
                }
            }
        }

        private readonly ConcurrentStack<TaskCompletionSource<Status>> writeTasks =
            new ConcurrentStack<TaskCompletionSource<Status>>();

        void WriteTaskCompleter(object sender, TagEventArgs e)
        {
            if (writeTasks.TryPop(out var writeTask))
            {
                switch (e.Status)
                {
                    case Status.Pending:
                        // Do nothing, wait for another WriteCompleted callback
                        break;
                    default:
                        writeTask?.SetResult(e.Status);
                        break;
                }
            }
        }
    }
}

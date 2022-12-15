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
    static class NativeTagWrapperAsyncExtensions
    {
        private const int TIMEOUT_VALUE_THAT_INDICATES_ASYNC_OPERATION = 0;

        public static async Task InitializeAsync(
            this NativeTagWrapperBase nativeTagWrapperBase,
            CancellationToken token = default
        )
        {
            nativeTagWrapperBase.ThrowIfAlreadyDisposed();
            nativeTagWrapperBase.ThrowIfAlreadyInitialized();

            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                cts.CancelAfter(nativeTagWrapperBase.Timeout);

                var createTasks = new ConcurrentStack<TaskCompletionSource<Status>>();
                EventHandler<TagEventArgs> createdTaskCompleter = (s, e) =>
                    TaskCompleter(createTasks, e);
                nativeTagWrapperBase.Created += createdTaskCompleter;

                using (
                    cts.Token.Register(() =>
                    {
                        if (createTasks.TryPop(out var createTask))
                        {
                            nativeTagWrapperBase.Abort();
                            nativeTagWrapperBase.Created -= createdTaskCompleter;

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
                    var createTask = new TaskCompletionSource<Status>(
                        TaskCreationOptions.RunContinuationsAsynchronously
                    );
                    createTasks.Push(createTask);

                    nativeTagWrapperBase.Initialize(TIMEOUT_VALUE_THAT_INDICATES_ASYNC_OPERATION);

                    if (nativeTagWrapperBase.GetStatus() == Status.Pending)
                        await createTask.Task;

                    nativeTagWrapperBase.Created -= createdTaskCompleter;
                    nativeTagWrapperBase.ThrowIfStatusNotOk(createTask.Task.Result);

                    nativeTagWrapperBase._isInitialized = true;
                }
            }
        }

        public static async Task ReadAsync(
            this NativeTagWrapperBase nativeTagWrapperBase,
            CancellationToken token = default
        )
        {
            nativeTagWrapperBase.ThrowIfAlreadyDisposed();

            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                cts.CancelAfter(nativeTagWrapperBase.Timeout);

                await InitializeAsyncIfRequired(nativeTagWrapperBase, cts.Token);

                var readTasks = new ConcurrentStack<TaskCompletionSource<Status>>();
                EventHandler<TagEventArgs> readTaskCompleter = (s, e) =>
                    TaskCompleter(readTasks, e);
                nativeTagWrapperBase.ReadCompleted += readTaskCompleter;

                using (
                    cts.Token.Register(() =>
                    {
                        if (readTasks.TryPop(out var readTask))
                        {
                            nativeTagWrapperBase.Abort();
                            nativeTagWrapperBase.ReadCompleted -= readTaskCompleter;

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

                    nativeTagWrapperBase.Read(TIMEOUT_VALUE_THAT_INDICATES_ASYNC_OPERATION);

                    await readTask.Task;
                    nativeTagWrapperBase.ReadCompleted -= readTaskCompleter;
                    nativeTagWrapperBase.ThrowIfStatusNotOk(readTask.Task.Result);
                }
            }
        }

        public static async Task WriteAsync(
            this NativeTagWrapperBase nativeTagWrapperBase,
            CancellationToken token = default
        )
        {
            nativeTagWrapperBase.ThrowIfAlreadyDisposed();

            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                cts.CancelAfter(nativeTagWrapperBase.Timeout);

                await InitializeAsyncIfRequired(nativeTagWrapperBase, cts.Token);

                var writeTasks = new ConcurrentStack<TaskCompletionSource<Status>>();
                EventHandler<TagEventArgs> writeTaskCompleter = (s, e) =>
                    TaskCompleter(writeTasks, e);
                nativeTagWrapperBase.WriteCompleted += writeTaskCompleter;

                using (
                    cts.Token.Register(() =>
                    {
                        if (writeTasks.TryPop(out var writeTask))
                        {
                            nativeTagWrapperBase.Abort();
                            nativeTagWrapperBase.WriteCompleted -= writeTaskCompleter;

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
                    nativeTagWrapperBase.Write(TIMEOUT_VALUE_THAT_INDICATES_ASYNC_OPERATION);
                    await writeTask.Task;
                    nativeTagWrapperBase.WriteCompleted -= writeTaskCompleter;

                    nativeTagWrapperBase.ThrowIfStatusNotOk(writeTask.Task.Result);
                }
            }
        }

        private static Task InitializeAsyncIfRequired(
            NativeTagWrapperBase nativeTagWrapperBase,
            CancellationToken token
        )
        {
            if (!nativeTagWrapperBase._isInitialized)
                return nativeTagWrapperBase.InitializeAsync(token);
            else
                return Task.CompletedTask;
        }

        private static void TaskCompleter(
            ConcurrentStack<TaskCompletionSource<Status>> stackTasks,
            TagEventArgs e
        )
        {
            if (stackTasks.TryPop(out var stackItem))
            {
                switch (e.Status)
                {
                    case Status.Pending:
                        // Do nothing, wait for another ReadCompleted callback
                        break;
                    default:
                        stackItem?.SetResult(e.Status);
                        break;
                }
            }
        }
    }
}

// Copyright (c) libplctag.NET contributors
// https://github.com/libplctag/libplctag.NET
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;

namespace libplctag.Tests
{
    public class ErrorNoDataExceptions
    {

        readonly TimeSpan REALISTIC_LATENCY_FOR_CREATE = TimeSpan.FromMilliseconds(50);
        readonly TimeSpan REALISTIC_LATENCY_FOR_READ = TimeSpan.FromMilliseconds(50);
        readonly TimeSpan REALISTIC_TIMEOUT_FOR_ALL_OPERATIONS = TimeSpan.FromMilliseconds(1000);


        [Fact]
        public async Task ErrorNoData_errors_are_thrown()
        {
            var nativeTag = GetMock();

            var tag = new NativeTagWrapper(nativeTag.Object)
            {
                Timeout = REALISTIC_TIMEOUT_FOR_ALL_OPERATIONS
            };

            // Act and Assert
            await Assert.ThrowsAsync<LibPlcTagException>(async () =>
            {
                await tag.ReadAsync();
            });
        }

        [Fact]
        public async Task ErrorNoData_exceptions_can_be_caught()
        {
            var nativeTag = GetMock();

            var tag = new NativeTagWrapper(nativeTag.Object)
            {
                Timeout = REALISTIC_TIMEOUT_FOR_ALL_OPERATIONS
            };

            // Act and Assert
            try
            {
                await tag.ReadAsync();
            }
            catch (LibPlcTagException ex) when (ex.Message == "ErrorNoData")
            {
            }
        }

        Mock<INativeTag> GetMock()
        {
            // Arrange
            const int tagId = 11;

            NativeImport.plctag.callback_func_ex callback = null;
            Status? status = null;

            var nativeTag = new Mock<INativeTag>();

            // The NativeTagWrapper should provide the native tag with a callback.
            // We will store this locally when a create call occurs, and fire it shortly after ...
            nativeTag
                .Setup(m => m.plc_tag_create_ex(It.IsAny<string>(), It.IsAny<NativeImport.plctag.callback_func_ex>(), It.IsAny<IntPtr>(), 0))
                .Callback<string, NativeImport.plctag.callback_func_ex, IntPtr, int>(async (attributeString, callbackFunc, userData, timeout) =>
                {
                    status = Status.Pending;
                    callback = callbackFunc;
                    await Task.Delay(REALISTIC_LATENCY_FOR_CREATE);
                    status = Status.ErrorNoData;
                    callback?.Invoke(tagId, (int)NativeImport.EVENT_CODES.PLCTAG_EVENT_CREATED, (int)NativeImport.STATUS_CODES.PLCTAG_ERR_NO_DATA, IntPtr.Zero);
                });

            // ... as well as when a read call occurs
            nativeTag
                .Setup(m => m.plc_tag_read(It.IsAny<int>(), 0))
                .Callback<int, int>(async (tagId, timeout) =>
                {
                    status = Status.Pending;
                    callback?.Invoke(tagId, (int)NativeImport.EVENT_CODES.PLCTAG_EVENT_READ_STARTED, (int)NativeImport.STATUS_CODES.PLCTAG_STATUS_PENDING, IntPtr.Zero);
                    await Task.Delay(REALISTIC_LATENCY_FOR_READ);
                    status = Status.ErrorNoData;
                    callback?.Invoke(tagId, (int)NativeImport.EVENT_CODES.PLCTAG_EVENT_READ_COMPLETED, (int)NativeImport.STATUS_CODES.PLCTAG_ERR_NO_DATA, IntPtr.Zero);
                });

            // the status was being tracked, so return it if asked
            nativeTag
                .Setup(m => m.plc_tag_status(It.IsAny<int>()))
                .Returns(() => (int)status.Value);

            return nativeTag;
        }

    }
}

// Copyright (c) libplctag.NET contributors
// https://github.com/libplctag/libplctag.NET
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("libplctag.Tests")]

namespace libplctag
{
    class NativeTagWrapper : NativeTagWrapperBase, IDisposable
    {
        public NativeTagWrapper(INativeTag nativeMethods) : base(nativeMethods) { }

        public void Initialize()
        {
            ThrowIfAlreadyDisposed();
            ThrowIfAlreadyInitialized();

            var millisecondTimeout = (int)Timeout.TotalMilliseconds;

            base.Initialize(millisecondTimeout);

            _isInitialized = true;
        }

        public void Read()
        {
            ThrowIfAlreadyDisposed();
            InitializeIfRequired();

            var millisecondTimeout = (int)Timeout.TotalMilliseconds;

            var result = base.Read(millisecondTimeout);

            ThrowIfStatusNotOk(result);
        }

        public void Write()
        {
            ThrowIfAlreadyDisposed();
            InitializeIfRequired();

            var millisecondTimeout = (int)Timeout.TotalMilliseconds;

            var result = base.Write(millisecondTimeout);
            ThrowIfStatusNotOk(result);
        }

        private void InitializeIfRequired()
        {
            if (!_isInitialized)
                Initialize();
        }
    }
}

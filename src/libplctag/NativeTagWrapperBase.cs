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
    abstract class NativeTagWrapperBase : IDisposable
    {
        private static readonly TimeSpan defaultTimeout = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan maxTimeout = TimeSpan.FromMilliseconds(int.MaxValue);

        private int nativeTagHandle;
        private libplctag.NativeImport.plctag.callback_func_ex coreLibCallbackFuncExDelegate;

        protected bool _isDisposed = false;
        internal bool _isInitialized = false;

        readonly INativeTag _native;

        public NativeTagWrapperBase(INativeTag nativeMethods)
        {
            _native = nativeMethods;
        }

        ~NativeTagWrapperBase()
        {
            Dispose();
        }

        // TODO remove. No longer used by Tag but would be a breaking change.
        public bool IsInitialized => _isInitialized;

        private string _name;
        public string Name
        {
            get => GetField(ref _name);
            set => SetField(ref _name, value);
        }

        private Protocol? _protocol;
        public Protocol? Protocol
        {
            get => GetField(ref _protocol);
            set => SetField(ref _protocol, value);
        }

        private string _gateway;
        public string Gateway
        {
            get => GetField(ref _gateway);
            set => SetField(ref _gateway, value);
        }

        private PlcType? _plcType;
        public PlcType? PlcType
        {
            get => GetField(ref _plcType);
            set => SetField(ref _plcType, value);
        }

        private string _path;
        public string Path
        {
            get => GetField(ref _path);
            set => SetField(ref _path, value);
        }

        private int? _elementSize;
        public int? ElementSize
        {
            get => GetField(ref _elementSize);
            set => SetField(ref _elementSize, value);
        }

        private int? _elementCount;
        public int? ElementCount
        {
            get => GetField(ref _elementCount);
            set => SetField(ref _elementCount, value);
        }

        private bool? _useConnectedMessaging;
        public bool? UseConnectedMessaging
        {
            get => GetField(ref _useConnectedMessaging);
            set => SetField(ref _useConnectedMessaging, value);
        }

        private bool? _allowPacking;
        public bool? AllowPacking
        {
            get => GetField(ref _allowPacking);
            set => SetField(ref _allowPacking, value);
        }

        private int? _readCacheMillisecondDuration;
        public int? ReadCacheMillisecondDuration
        {
            get
            {
                ThrowIfAlreadyDisposed();

                if (!_isInitialized)
                    return _readCacheMillisecondDuration;

                return GetIntAttribute("read_cache_ms");
            }
            set
            {
                ThrowIfAlreadyDisposed();

                if (!_isInitialized)
                {
                    _readCacheMillisecondDuration = value;
                    return;
                }

                SetIntAttribute("read_cache_ms", value.Value);
            }
        }

        private TimeSpan _timeout = defaultTimeout;
        public TimeSpan Timeout
        {
            get
            {
                ThrowIfAlreadyDisposed();
                return _timeout;
            }
            set
            {
                ThrowIfAlreadyDisposed();
                if (value <= TimeSpan.Zero || value > maxTimeout)
                    throw new ArgumentOutOfRangeException(
                        nameof(Timeout),
                        value,
                        "Must be greater than 0"
                    );
                _timeout = value;
            }
        }

        private TimeSpan? _autoSyncReadInterval;
        public TimeSpan? AutoSyncReadInterval
        {
            get => GetField(ref _autoSyncReadInterval);
            set => SetField(ref _autoSyncReadInterval, value);
        }

        private TimeSpan? _autoSyncWriteInterval;
        public TimeSpan? AutoSyncWriteInterval
        {
            get => GetField(ref _autoSyncWriteInterval);
            set => SetField(ref _autoSyncWriteInterval, value);
        }

        private DebugLevel _debugLevel = DebugLevel.None;
        public DebugLevel DebugLevel
        {
            get => GetField(ref _debugLevel);
            set => SetField(ref _debugLevel, value);
        }

        private string _int16ByteOrder;
        public string Int16ByteOrder
        {
            get => GetField(ref _int16ByteOrder);
            set => SetField(ref _int16ByteOrder, value);
        }

        private string _int32ByteOrder;
        public string Int32ByteOrder
        {
            get => GetField(ref _int32ByteOrder);
            set => SetField(ref _int32ByteOrder, value);
        }

        private string _int64ByteOrder;
        public string Int64ByteOrder
        {
            get => GetField(ref _int64ByteOrder);
            set => SetField(ref _int64ByteOrder, value);
        }

        private string _float32ByteOrder;
        public string Float32ByteOrder
        {
            get => GetField(ref _float32ByteOrder);
            set => SetField(ref _float32ByteOrder, value);
        }

        private string _float64ByteOrder;
        public string Float64ByteOrder
        {
            get => GetField(ref _float64ByteOrder);
            set => SetField(ref _float64ByteOrder, value);
        }

        private uint? _stringCountWordBytes;
        public uint? StringCountWordBytes
        {
            get => GetField(ref _stringCountWordBytes);
            set => SetField(ref _stringCountWordBytes, value);
        }

        private bool? _stringIsByteSwapped;
        public bool? StringIsByteSwapped
        {
            get => GetField(ref _stringIsByteSwapped);
            set => SetField(ref _stringIsByteSwapped, value);
        }

        private bool? _stringIsCounted;
        public bool? StringIsCounted
        {
            get => GetField(ref _stringIsCounted);
            set => SetField(ref _stringIsCounted, value);
        }

        private bool? _stringIsFixedLength;
        public bool? StringIsFixedLength
        {
            get => GetField(ref _stringIsFixedLength);
            set => SetField(ref _stringIsFixedLength, value);
        }

        private bool? _stringIsZeroTerminated;
        public bool? StringIsZeroTerminated
        {
            get => GetField(ref _stringIsZeroTerminated);
            set => SetField(ref _stringIsZeroTerminated, value);
        }

        private uint? _stringMaxCapacity;
        public uint? StringMaxCapacity
        {
            get => GetField(ref _stringMaxCapacity);
            set => SetField(ref _stringMaxCapacity, value);
        }

        private uint? _stringPadBytes;
        public uint? StringPadBytes
        {
            get => GetField(ref _stringPadBytes);
            set => SetField(ref _stringPadBytes, value);
        }

        private uint? _stringTotalLength;
        public uint? StringTotalLength
        {
            get => GetField(ref _stringTotalLength);
            set => SetField(ref _stringTotalLength, value);
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            if (_isInitialized)
            {
                RemoveEventsAndRemoveCallback();
                var result = (Status)_native.plc_tag_destroy(nativeTagHandle);
                ThrowIfStatusNotOk(result);
            }

            _isDisposed = true;
        }

        public void Abort()
        {
            ThrowIfAlreadyDisposed();
            var result = (Status)_native.plc_tag_abort(nativeTagHandle);
            ThrowIfStatusNotOk(result);
        }

        public int GetSize()
        {
            ThrowIfAlreadyDisposed();

            var result = _native.plc_tag_get_size(nativeTagHandle);
            if (result < 0)
                throw new LibPlcTagException((Status)result);
            else
                return result;
        }

        public void SetSize(int newSize)
        {
            ThrowIfAlreadyDisposed();
            var result = (Status)_native.plc_tag_set_size(nativeTagHandle, newSize);
            ThrowIfStatusNotOk(result);
        }

        public Status GetStatus()
        {
            ThrowIfAlreadyDisposed();
            return (Status)_native.plc_tag_status(nativeTagHandle);
        }

        public void GetBuffer(byte[] buffer)
        {
            ThrowIfAlreadyDisposed();
            var result = (Status)
                _native.plc_tag_get_raw_bytes(nativeTagHandle, 0, buffer, buffer.Length);
            ThrowIfStatusNotOk(result);
        }

        public byte[] GetBuffer()
        {
            ThrowIfAlreadyDisposed();

            var tagSize = GetSize();
            var temp = new byte[tagSize];

            var result = (Status)
                _native.plc_tag_get_raw_bytes(nativeTagHandle, 0, temp, temp.Length);
            ThrowIfStatusNotOk(result);

            return temp;
        }

        public void SetBuffer(byte[] buffer)
        {
            ThrowIfAlreadyDisposed();

            GetNativeValueAndThrowOnNegativeResult(_native.plc_tag_set_size, buffer.Length);
            var result = (Status)
                _native.plc_tag_set_raw_bytes(nativeTagHandle, 0, buffer, buffer.Length);
            ThrowIfStatusNotOk(result);
        }

        private int GetIntAttribute(string attributeName)
        {
            ThrowIfAlreadyDisposed();

            var result = _native.plc_tag_get_int_attribute(
                nativeTagHandle,
                attributeName,
                int.MinValue
            );
            if (result == int.MinValue)
                ThrowIfStatusNotOk();

            return result;
        }

        private void SetIntAttribute(string attributeName, int value)
        {
            ThrowIfAlreadyDisposed();

            var result = (Status)
                _native.plc_tag_set_int_attribute(nativeTagHandle, attributeName, value);
            ThrowIfStatusNotOk(result);
        }

        public bool GetBit(int offset)
        {
            ThrowIfAlreadyDisposed();

            var result = _native.plc_tag_get_bit(nativeTagHandle, offset);
            if (result == 0)
                return false;
            else if (result == 1)
                return true;
            else
                throw new LibPlcTagException((Status)result);
        }

        public void SetBit(int offset, bool value) =>
            SetNativeTagValue(_native.plc_tag_set_bit, offset, value == true ? 1 : 0);

        public ulong GetUInt64(int offset) =>
            GetNativeValueAndThrowOnSpecificResult(
                _native.plc_tag_get_uint64,
                offset,
                ulong.MaxValue
            );

        public void SetUInt64(int offset, ulong value) =>
            SetNativeTagValue(_native.plc_tag_set_uint64, offset, value);

        public long GetInt64(int offset) =>
            GetNativeValueAndThrowOnSpecificResult(
                _native.plc_tag_get_int64,
                offset,
                long.MinValue
            );

        public void SetInt64(int offset, long value) =>
            SetNativeTagValue(_native.plc_tag_set_int64, offset, value);

        public uint GetUInt32(int offset) =>
            GetNativeValueAndThrowOnSpecificResult(
                _native.plc_tag_get_uint32,
                offset,
                uint.MaxValue
            );

        public void SetUInt32(int offset, uint value) =>
            SetNativeTagValue(_native.plc_tag_set_uint32, offset, value);

        public int GetInt32(int offset) =>
            GetNativeValueAndThrowOnSpecificResult(_native.plc_tag_get_int32, offset, int.MinValue);

        public void SetInt32(int offset, int value) =>
            SetNativeTagValue(_native.plc_tag_set_int32, offset, value);

        public ushort GetUInt16(int offset) =>
            GetNativeValueAndThrowOnSpecificResult(
                _native.plc_tag_get_uint16,
                offset,
                ushort.MaxValue
            );

        public void SetUInt16(int offset, ushort value) =>
            SetNativeTagValue(_native.plc_tag_set_uint16, offset, value);

        public short GetInt16(int offset) =>
            GetNativeValueAndThrowOnSpecificResult(
                _native.plc_tag_get_int16,
                offset,
                short.MinValue
            );

        public void SetInt16(int offset, short value) =>
            SetNativeTagValue(_native.plc_tag_set_int16, offset, value);

        public byte GetUInt8(int offset) =>
            GetNativeValueAndThrowOnSpecificResult(
                _native.plc_tag_get_uint8,
                offset,
                byte.MaxValue
            );

        public void SetUInt8(int offset, byte value) =>
            SetNativeTagValue(_native.plc_tag_set_uint8, offset, value);

        public sbyte GetInt8(int offset) =>
            GetNativeValueAndThrowOnSpecificResult(
                _native.plc_tag_get_int8,
                offset,
                sbyte.MinValue
            );

        public void SetInt8(int offset, sbyte value) =>
            SetNativeTagValue(_native.plc_tag_set_int8, offset, value);

        public double GetFloat64(int offset) =>
            GetNativeValueAndThrowOnSpecificResult(
                _native.plc_tag_get_float64,
                offset,
                double.MinValue
            );

        public void SetFloat64(int offset, double value) =>
            SetNativeTagValue(_native.plc_tag_set_float64, offset, value);

        public float GetFloat32(int offset) =>
            GetNativeValueAndThrowOnSpecificResult(
                _native.plc_tag_get_float32,
                offset,
                float.MinValue
            );

        public void SetFloat32(int offset, float value) =>
            SetNativeTagValue(_native.plc_tag_set_float32, offset, value);

        public void SetString(int offset, string value) =>
            SetNativeTagValue(_native.plc_tag_set_string, offset, value);

        public int GetStringLength(int offset) =>
            GetNativeValueAndThrowOnNegativeResult(_native.plc_tag_get_string_length, offset);

        public int GetStringCapacity(int offset) =>
            GetNativeValueAndThrowOnNegativeResult(_native.plc_tag_get_string_capacity, offset);

        public int GetStringTotalLength(int offset) =>
            GetNativeValueAndThrowOnNegativeResult(_native.plc_tag_get_string_total_length, offset);

        public string GetString(int offset)
        {
            ThrowIfAlreadyDisposed();
            var stringLength = GetStringLength(offset);
            var sb = new StringBuilder(stringLength);
            var status = (Status)
                _native.plc_tag_get_string(nativeTagHandle, offset, sb, stringLength);
            ThrowIfStatusNotOk(status);
            return sb.ToString().Substring(0, stringLength);
        }

        internal void ThrowIfAlreadyDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        internal void ThrowIfAlreadyInitialized()
        {
            if (_isInitialized)
                throw new InvalidOperationException("Already initialized");
        }

        internal void ThrowIfStatusNotOk(Status? status = null)
        {
            var statusToCheck = status ?? GetStatus();
            if (statusToCheck != Status.Ok)
                throw new LibPlcTagException(statusToCheck);
        }

        private void SetNativeTagValue<T>(Func<int, int, T, int> nativeMethod, int offset, T value)
        {
            ThrowIfAlreadyDisposed();
            var result = (Status)nativeMethod(nativeTagHandle, offset, value);
            ThrowIfStatusNotOk(result);
        }

        private int GetNativeValueAndThrowOnNegativeResult(
            Func<int, int, int> nativeMethod,
            int offset
        )
        {
            ThrowIfAlreadyDisposed();
            var result = nativeMethod(nativeTagHandle, offset);
            if (result < 0)
                throw new LibPlcTagException((Status)result);
            return result;
        }

        private T GetNativeValueAndThrowOnSpecificResult<T>(
            Func<int, int, T> nativeMethod,
            int offset,
            T valueIndicatingPossibleError
        ) where T : struct
        {
            ThrowIfAlreadyDisposed();
            var result = nativeMethod(nativeTagHandle, offset);
            if (result.Equals(valueIndicatingPossibleError))
                ThrowIfStatusNotOk();
            return result;
        }

        private T GetField<T>(ref T field)
        {
            ThrowIfAlreadyDisposed();
            return field;
        }

        private void SetField<T>(ref T field, T value)
        {
            ThrowIfAlreadyDisposed();
            ThrowIfAlreadyInitialized();
            field = value;
        }

        internal void Initialize(int millisecondTimeout)
        {
            var attributeString = GetAttributeString();

            // Need to keep a reference to the delegate in memory so it doesn't get garbage collected
            coreLibCallbackFuncExDelegate = new libplctag.NativeImport.plctag.callback_func_ex(
                coreLibEventCallback
            );

            var result = _native.plc_tag_create_ex(
                attributeString,
                coreLibCallbackFuncExDelegate,
                IntPtr.Zero,
                millisecondTimeout
            );
            if (result < 0)
                throw new LibPlcTagException((Status)result);
            else
                nativeTagHandle = result;

            _isInitialized = true;
        }

        internal libplctag.Status Read(int millisecondTimeout)
        {
            var result = (Status)_native.plc_tag_read(nativeTagHandle, millisecondTimeout);
            return result;
        }

        internal libplctag.Status Write(int millisecondTimeout)
        {
            var result = (Status)_native.plc_tag_write(nativeTagHandle, millisecondTimeout);
            return result;
        }

        private string GetAttributeString()
        {
            string FormatNullableBoolean(bool? value) =>
                value.HasValue ? (value.Value ? "1" : "0") : null;

            string FormatPlcType(PlcType? type)
            {
                if (type == libplctag.PlcType.Omron)
                    return "omron-njnx";
                else
                    return type?.ToString().ToLowerInvariant();
            }

            var attributes = new Dictionary<string, string>
            {
                { "protocol", Protocol?.ToString() },
                { "gateway", Gateway },
                { "path", Path },
                { "plc", FormatPlcType(PlcType) },
                { "elem_size", ElementSize?.ToString() },
                { "elem_count", ElementCount?.ToString() },
                { "name", Name },
                { "read_cache_ms", ReadCacheMillisecondDuration?.ToString() },
                { "use_connected_msg", FormatNullableBoolean(UseConnectedMessaging) },
                { "allow_packing", FormatNullableBoolean(AllowPacking) },
                { "auto_sync_read_ms", AutoSyncReadInterval?.TotalMilliseconds.ToString() },
                { "auto_sync_write_ms", AutoSyncWriteInterval?.TotalMilliseconds.ToString() },
                { "debug", DebugLevel == DebugLevel.None ? null : ((int)DebugLevel).ToString() },
                { "int16_byte_order", Int16ByteOrder },
                { "int32_byte_order", Int32ByteOrder },
                { "int64_byte_order", Int64ByteOrder },
                { "float32_byte_order", Float32ByteOrder },
                { "float64_byte_order", Float64ByteOrder },
                { "str_count_word_bytes", StringCountWordBytes?.ToString() },
                { "str_is_byte_swapped", FormatNullableBoolean(StringIsByteSwapped) },
                { "str_is_counted", FormatNullableBoolean(StringIsCounted) },
                { "str_is_fixed_length", FormatNullableBoolean(StringIsFixedLength) },
                { "str_is_zero_terminated", FormatNullableBoolean(StringIsFixedLength) },
                { "str_max_capacity", StringMaxCapacity?.ToString() },
                { "str_pad_bytes", StringPadBytes?.ToString() },
                { "str_total_length", StringTotalLength?.ToString() },
            };

            string separator = "&";
            return string.Join(
                separator,
                attributes
                    .Where(attr => attr.Value != null)
                    .Select(attr => $"{attr.Key}={attr.Value}")
            );
        }

        void RemoveEventsAndRemoveCallback()
        {
            var callbackRemovalResult = (Status)
                _native.plc_tag_unregister_callback(nativeTagHandle);
            ThrowIfStatusNotOk(callbackRemovalResult);
        }

        public event EventHandler<TagEventArgs> ReadStarted;
        public event EventHandler<TagEventArgs> ReadCompleted;
        public event EventHandler<TagEventArgs> WriteStarted;
        public event EventHandler<TagEventArgs> WriteCompleted;
        public event EventHandler<TagEventArgs> Aborted;
        public event EventHandler<TagEventArgs> Destroyed;
        public event EventHandler<TagEventArgs> Created;

        void coreLibEventCallback(
            int eventTagHandle,
            int eventCode,
            int statusCode,
            IntPtr userdata
        )
        {
            var @event = (Event)eventCode;
            var status = (Status)statusCode;
            var eventArgs = new TagEventArgs() { Status = status };

            switch (@event)
            {
                case Event.ReadCompleted:
                    ReadCompleted?.Invoke(this, eventArgs);
                    break;
                case Event.ReadStarted:
                    ReadStarted?.Invoke(this, eventArgs);
                    break;
                case Event.WriteStarted:
                    WriteStarted?.Invoke(this, eventArgs);
                    break;
                case Event.WriteCompleted:
                    WriteCompleted?.Invoke(this, eventArgs);
                    break;
                case Event.Aborted:
                    Aborted?.Invoke(this, eventArgs);
                    break;
                case Event.Destroyed:
                    Destroyed?.Invoke(this, eventArgs);
                    break;
                case Event.Created:
                    Created?.Invoke(this, eventArgs);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}

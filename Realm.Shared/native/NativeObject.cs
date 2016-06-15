////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////
 
using System;
using System.Runtime.InteropServices;

namespace Realms
{
    internal static class NativeObject
    {
        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_row_index", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr get_row_index(ObjectHandle objectHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_is_attached", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr get_is_attached(ObjectHandle objectHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_destroy", CallingConvention = CallingConvention.Cdecl)]
        public static extern void destroy(IntPtr objectPtr);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_timestamp_milliseconds", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void set_timestamp_milliseconds(ObjectHandle objectHandle, IntPtr columnNdx, Int64 value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_timestamp_milliseconds", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Int64 get_timestamp_milliseconds(ObjectHandle objectHandle, IntPtr columnIndex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_nullable_timestamp_milliseconds", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_nullable_timestamp_milliseconds(ObjectHandle objectHandle, IntPtr columnIndex, ref long retVal);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_string", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void set_string(ObjectHandle objectHandle, IntPtr columnNdx,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_string_unique", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void set_string_unique(ObjectHandle objectHandle, IntPtr columnNdx,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_string", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_string(ObjectHandle objectHandle, IntPtr columnIndex,
            IntPtr buffer, IntPtr bufsize, [MarshalAs(UnmanagedType.I1)] out bool isNull);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_link", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void set_link(ObjectHandle objectHandle, IntPtr columnNdx, IntPtr targetRowNdx);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_clear_link", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void clear_link(ObjectHandle objectHandle, IntPtr columnNdx);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_link", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_link(ObjectHandle objectHandle, IntPtr columnIndex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_linklist", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_linklist(ObjectHandle objectHandle, IntPtr columnIndex);
        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_null", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void set_null(ObjectHandle objectHandle, IntPtr columnIndex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_bool", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void set_bool(ObjectHandle objectHandle, IntPtr columnIndex, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_bool", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_bool(ObjectHandle objectHandle, IntPtr columnIndex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_nullable_bool", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_nullable_bool(ObjectHandle objectHandle, IntPtr columnIndex, ref IntPtr retVal);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_int64", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void set_int64(ObjectHandle objectHandle, IntPtr columnNdx, Int64 value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_int64_unique", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void set_int64_unique(ObjectHandle objectHandle, IntPtr columnNdx, Int64 value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_int64", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Int64 get_int64(ObjectHandle objectHandle, IntPtr columnIndex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_nullable_int64", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_nullable_int64(ObjectHandle objectHandle, IntPtr columnIndex, ref Int64 retVal);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_float", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void set_float(ObjectHandle objectHandle, IntPtr columnNdx, float value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_float", CallingConvention = CallingConvention.Cdecl)]
        internal static extern float get_float(ObjectHandle objectHandle, IntPtr columnIndex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_nullable_float", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_nullable_float(ObjectHandle objectHandle, IntPtr columnIndex, ref float retVal);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_double", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void set_double(ObjectHandle objectHandle, IntPtr columnNdx, double value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_double", CallingConvention = CallingConvention.Cdecl)]
        internal static extern double get_double(ObjectHandle objectHandle, IntPtr columnIndex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_nullable_double", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_nullable_double(ObjectHandle objectHandle, IntPtr columnIndex, ref double retVal);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_binary", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr set_binary(ObjectHandle objectHandle, IntPtr columnIndex, IntPtr buffer, IntPtr bufferLength);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_binary", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_binary(ObjectHandle objectHandle, IntPtr columnIndex, out IntPtr retBuffer, out int retBufferLength);

    }
}

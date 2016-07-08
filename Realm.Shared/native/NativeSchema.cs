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
    internal static class NativeSchema
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct Property
        {
            internal static readonly int Size = Marshal.SizeOf<Property>();

            [MarshalAs(UnmanagedType.LPStr)]
            internal string name;

            [MarshalAs(UnmanagedType.U1)]
            internal Schema.PropertyType type;

            [MarshalAs(UnmanagedType.LPStr)]
            internal string object_type;

            [MarshalAs(UnmanagedType.I1)]
            internal bool is_nullable;

            [MarshalAs(UnmanagedType.I1)]
            internal bool is_primary;

            [MarshalAs(UnmanagedType.I1)]
            internal bool is_indexed;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Object
        {
            internal static readonly int Size = Marshal.SizeOf<Object>();

            [MarshalAs(UnmanagedType.LPStr)]
            internal string name;

            internal int properties_start;
            internal int properties_end;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct SchemaFromUnmanagedMarshalling
        {
            internal IntPtr handle;
            internal UInt64 schema_version;

            internal IntPtr /* Object[] */ objects;
            internal IntPtr /* IntPtr[] */ object_handles;
            internal int objects_len;

            internal IntPtr /* Property[] */ properties;

        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "schema_create", CallingConvention = CallingConvention.Cdecl)]
        internal static unsafe extern IntPtr create([MarshalAs(UnmanagedType.LPArray), In] Object[] objects, int objects_length,
                                                          [MarshalAs(UnmanagedType.LPArray), In] Property[] properties,
                                                          [MarshalAs(UnmanagedType.LPArray), Out] IntPtr[] object_schema_handles);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "schema_clone", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr clone(SchemaHandle schema, [MarshalAs(UnmanagedType.LPArray), In, Out] IntPtr[] object_schema_handles);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "schema_destroy", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void destroy(IntPtr schema);
    }
}

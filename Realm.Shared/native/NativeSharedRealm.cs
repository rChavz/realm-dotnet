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
    internal static class NativeSharedRealm
    {
        internal delegate void MigrationCallbackDelegate(IntPtr oldRealm, NativeSchema.SchemaFromUnmanagedMarshalling oldSchema, IntPtr newRealm, IntPtr data);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Configuration
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string path;
            internal IntPtr path_len;

            [MarshalAs(UnmanagedType.I1)]
            internal bool read_only;
            [MarshalAs(UnmanagedType.I1)]
            internal bool in_memory;

            [MarshalAs(UnmanagedType.LPArray)]
            internal byte[] encryption_key;

            internal IntPtr schema;
            internal UInt64 schema_version;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_open", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr open(Configuration configuration, MigrationCallbackDelegate migrationCallback, IntPtr migrationCallbackData);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_bind_to_managed_realm_handle", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void bind_to_managed_realm_handle(SharedRealmHandle sharedRealm, IntPtr managedRealmHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_destroy", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void destroy(IntPtr sharedRealm);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_begin_transaction",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern void begin_transaction(SharedRealmHandle sharedRealm);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_commit_transaction",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern void commit_transaction(SharedRealmHandle sharedRealm);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_cancel_transaction",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern void cancel_transaction(SharedRealmHandle sharedRealm);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_is_in_transaction",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr is_in_transaction(SharedRealmHandle sharedRealm);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_refresh",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr refresh(SharedRealmHandle sharedRealm);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_get_table",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_table(SharedRealmHandle sharedRealm, [MarshalAs(UnmanagedType.LPWStr)]string tableName, IntPtr tableNameLength);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_is_same_instance",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr is_same_instance(SharedRealmHandle lhs, SharedRealmHandle rhs);
    }
}

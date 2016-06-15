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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Realms
{
    /// <summary>
    /// Base for any object that can be persisted in a Realm.
    /// </summary>
    public class RealmObject
    {
        private Realm _realm;
        private ObjectHandle _objectHandle;
        private Metadata _metadata;

        internal Realm Realm => _realm;
        internal ObjectHandle ObjectHandle => _objectHandle;

        /// <summary>
        /// Allows you to check if the object has been associated with a Realm, either at creation or via Realm.Manage.
        /// </summary>
        public bool IsManaged => _realm != null;

        internal void _Manage(Realm realm, ObjectHandle objectHandle)
        {
            _realm = realm;
            _objectHandle = objectHandle;
            _metadata = realm.Metadata[GetType()];
        }

        internal class Metadata
        {
            internal TableHandle Table;

            internal Weaving.IRealmObjectHelper Helper;

            internal Dictionary<string, IntPtr> ColumnIndices;
        }

        internal void _CopyDataFromBackingFieldsToRow()
        {
            Debug.Assert(this.IsManaged);

            var thisType = this.GetType();
            var wovenProperties = from prop in thisType.GetProperties()
                                  let backingField = prop.GetCustomAttributes(false)
                                                         .OfType<WovenPropertyAttribute>()
                                                         .Select(a => a.BackingFieldName)
                                                         .SingleOrDefault()
                                  where backingField != null
                                  select new { Info = prop, Field = thisType.GetField(backingField, BindingFlags.Instance | BindingFlags.NonPublic) };

            foreach (var prop in wovenProperties)
            {
                var value = prop.Field.GetValue(this);
                if (prop.Info.PropertyType.IsGenericType)
                {
                    var genericType = prop.Info.PropertyType.GetGenericTypeDefinition();
                    if (genericType == typeof(RealmList<>))
                    {
                        continue;
                    }
                }

                prop.Info.SetValue(this, value, null);
            }
        }


        #region Getters
        protected string GetStringValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var badUTF8msg = $"Corrupted string UTF8 in {propertyName}";

            int bufferSizeNeededChars = 128;
            // First alloc this thread
            if (_realm.stringGetBuffer==IntPtr.Zero) {  // first get of a string in this Realm
                _realm.stringGetBuffer = Marshal.AllocHGlobal((IntPtr)(bufferSizeNeededChars * sizeof(char)));
                _realm.stringGetBufferLen = bufferSizeNeededChars;
            }    

            bool isNull = false;

            // try to read
            int bytesRead = (int)NativeObject.get_string(_objectHandle, _metadata.ColumnIndices[propertyName], _realm.stringGetBuffer,
                (IntPtr)_realm.stringGetBufferLen, out isNull);
            if (bytesRead == -1)
            {
                // bad UTF-8 data unable to transcode, vastly unlikely error but could be corrupt file
                throw new RealmInvalidDatabaseException(badUTF8msg);
            }
            if (bytesRead > _realm.stringGetBufferLen)  // need a bigger buffer
            {
                Marshal.FreeHGlobal(_realm.stringGetBuffer);
                _realm.stringGetBuffer = Marshal.AllocHGlobal((IntPtr)(bytesRead * sizeof(char)));
                _realm.stringGetBufferLen = bytesRead;
                // try to read with big buffer
                bytesRead = (int)NativeObject.get_string(_objectHandle, _metadata.ColumnIndices[propertyName], _realm.stringGetBuffer,
                    (IntPtr)_realm.stringGetBufferLen, out isNull);
                if (bytesRead == -1)  // bad UTF-8 in full string
                    throw new RealmInvalidDatabaseException(badUTF8msg);
                Debug.Assert(bytesRead <= _realm.stringGetBufferLen);
            }  // needed re-read with expanded buffer

            if (bytesRead == 0)
            {
                if (isNull)
                    return null;
                
                return "";
            }

            return Marshal.PtrToStringUni(_realm.stringGetBuffer, bytesRead);
            // leaving buffer sitting allocated for quick reuse next time we read a string                
        } // GetStringValue

        protected char GetCharValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var value = NativeObject.get_int64(_objectHandle, _metadata.ColumnIndices[propertyName]);
            return (char) value;
        }

        protected char? GetNullableCharValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var retVal = 0L;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeObject.get_nullable_int64(_objectHandle, _metadata.ColumnIndices[propertyName], ref retVal));
            return hasValue ? (char)retVal : (char?) null;
        }

        protected byte GetByteValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var value = NativeObject.get_int64(_objectHandle, _metadata.ColumnIndices[propertyName]);
            return (byte) value;
        }

        protected byte? GetNullableByteValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var retVal = 0L;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeObject.get_nullable_int64(_objectHandle, _metadata.ColumnIndices[propertyName], ref retVal));
            return hasValue ? (byte)retVal : (byte?) null;
        }

        protected short GetInt16Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var value = NativeObject.get_int64(_objectHandle, _metadata.ColumnIndices[propertyName]);
            return (short) value;
        }

        protected short? GetNullableInt16Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var retVal = 0L;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeObject.get_nullable_int64(_objectHandle, _metadata.ColumnIndices[propertyName], ref retVal));
            return hasValue ? (short)retVal : (short?) null;
        }

        protected int GetInt32Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var value = NativeObject.get_int64(_objectHandle, _metadata.ColumnIndices[propertyName]);
            return (int) value;
        }

        protected int? GetNullableInt32Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var retVal = 0L;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeObject.get_nullable_int64(_objectHandle, _metadata.ColumnIndices[propertyName], ref retVal));
            return hasValue ? (int)retVal : (int?) null;
        }

        protected long GetInt64Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeObject.get_int64(_objectHandle, _metadata.ColumnIndices[propertyName]);
        }

        protected long? GetNullableInt64Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var retVal = 0L;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeObject.get_nullable_int64(_objectHandle, _metadata.ColumnIndices[propertyName], ref retVal));
            return hasValue ? retVal : (long?) null;
        }

        protected float GetSingleValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeObject.get_float(_objectHandle, _metadata.ColumnIndices[propertyName]);
        }

        protected float? GetNullableSingleValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var retVal = 0.0f;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeObject.get_nullable_float(_objectHandle, _metadata.ColumnIndices[propertyName], ref retVal));
            return hasValue ? retVal : (float?) null;
        }

        protected double GetDoubleValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeObject.get_double(_objectHandle, _metadata.ColumnIndices[propertyName]);
        }

        protected double? GetNullableDoubleValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var retVal = 0.0d;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeObject.get_nullable_double(_objectHandle, _metadata.ColumnIndices[propertyName], ref retVal));
            return hasValue ? retVal : (double?) null;
        }

        protected bool GetBooleanValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return MarshalHelpers.IntPtrToBool(NativeObject.get_bool(_objectHandle, _metadata.ColumnIndices[propertyName]));
        }

        protected bool? GetNullableBooleanValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var retVal = IntPtr.Zero;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeObject.get_nullable_bool(_objectHandle, _metadata.ColumnIndices[propertyName], ref retVal));
            return hasValue ? MarshalHelpers.IntPtrToBool(retVal) : (bool?) null;
        }

        protected DateTimeOffset GetDateTimeOffsetValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var unixTimeMS = NativeObject.get_timestamp_milliseconds(_objectHandle, _metadata.ColumnIndices[propertyName]);
            return DateTimeOffsetExtensions.FromRealmUnixTimeMilliseconds(unixTimeMS);
        }

        protected DateTimeOffset? GetNullableDateTimeOffsetValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            long unixTimeMS = 0;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeObject.get_nullable_timestamp_milliseconds(_objectHandle, _metadata.ColumnIndices[propertyName], ref unixTimeMS));
            return hasValue ? DateTimeOffsetExtensions.FromRealmUnixTimeMilliseconds(unixTimeMS) : (DateTimeOffset?)null;
        }

        protected RealmList<T> GetListValue<T>(string propertyName) where T : RealmObject
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var listHandle = _metadata.Table.TableLinkList (_metadata.ColumnIndices[propertyName], _objectHandle);
            return new RealmList<T>(this, listHandle);
        }

        protected T GetObjectValue<T>(string propertyName) where T : RealmObject
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var linkedRowPtr = NativeObject.get_link (_objectHandle, _metadata.ColumnIndices[propertyName]);
            return (T)MakeRealmObject(typeof(T), linkedRowPtr);
        }

        protected byte[] GetByteArrayValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            int bufferSize;
            IntPtr buffer;
            if (NativeObject.get_binary(_objectHandle, _metadata.ColumnIndices[propertyName], out buffer, out bufferSize) != IntPtr.Zero)
            {
                var bytes = new byte[bufferSize];
                Marshal.Copy(buffer, bytes, 0, bufferSize);
                return bytes;
            }

            return null;
        }

        #endregion

        #region Setters

        protected void SetStringValue(string propertyName, string value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            if (value != null)
                NativeObject.set_string(_objectHandle, _metadata.ColumnIndices[propertyName], value, (IntPtr)value.Length);
            else
                NativeObject.set_null(_objectHandle, _metadata.ColumnIndices[propertyName]);
        }

        protected void SetStringValueUnique(string propertyName, string value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            if (value == null)
                throw new ArgumentException("Object identifiers cannot be null");

            NativeObject.set_string_unique(_objectHandle, _metadata.ColumnIndices[propertyName], value, (IntPtr)value.Length);
        }

        protected void SetCharValue(string propertyName, char value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeObject.set_int64(_objectHandle, _metadata.ColumnIndices[propertyName], value);
        }

        protected void SetCharValueUnique(string propertyName, char value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeObject.set_int64_unique(_objectHandle, _metadata.ColumnIndices[propertyName], value);
        }

        protected void SetNullableCharValue(string propertyName, char? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            if (value.HasValue)
                NativeObject.set_int64(_objectHandle, _metadata.ColumnIndices[propertyName], value.Value);
            else
                NativeObject.set_null(_objectHandle, _metadata.ColumnIndices[propertyName]);
        }

        protected void SetByteValue(string propertyName, byte value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeObject.set_int64(_objectHandle, _metadata.ColumnIndices[propertyName], value);
        }

        protected void SetByteValueUnique(string propertyName, byte value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeObject.set_int64_unique(_objectHandle, _metadata.ColumnIndices[propertyName], value);
        }

        protected void SetNullableByteValue(string propertyName, byte? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            if (value.HasValue)
                NativeObject.set_int64(_objectHandle, _metadata.ColumnIndices[propertyName], value.Value);
            else
                NativeObject.set_null(_objectHandle, _metadata.ColumnIndices[propertyName]);
        }

        protected void SetInt16Value(string propertyName, short value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeObject.set_int64(_objectHandle, _metadata.ColumnIndices[propertyName], value);
        }

        protected void SetInt16ValueUnique(string propertyName, short value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeObject.set_int64_unique(_objectHandle, _metadata.ColumnIndices[propertyName], value);
        }

        protected void SetNullableInt16Value(string propertyName, short? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            if (value.HasValue)
                NativeObject.set_int64(_objectHandle, _metadata.ColumnIndices[propertyName], value.Value);
            else
                NativeObject.set_null(_objectHandle, _metadata.ColumnIndices[propertyName]);
        }

        protected void SetInt32Value(string propertyName, int value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeObject.set_int64(_objectHandle, _metadata.ColumnIndices[propertyName], value);
        }

        protected void SetInt32ValueUnique(string propertyName, int value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeObject.set_int64_unique(_objectHandle, _metadata.ColumnIndices[propertyName], value);
        }

        protected void SetNullableInt32Value(string propertyName, int? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            if (value.HasValue)
                NativeObject.set_int64(_objectHandle, _metadata.ColumnIndices[propertyName], value.Value);
            else
                NativeObject.set_null(_objectHandle, _metadata.ColumnIndices[propertyName]);
        }

        protected void SetInt64Value(string propertyName, long value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeObject.set_int64(_objectHandle, _metadata.ColumnIndices[propertyName], value);
        }

        protected void SetInt64ValueUnique(string propertyName, long value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeObject.set_int64_unique(_objectHandle, _metadata.ColumnIndices[propertyName], value);
        }

        protected void SetNullableInt64Value(string propertyName, long? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            if (value.HasValue)
                NativeObject.set_int64(_objectHandle, _metadata.ColumnIndices[propertyName], value.Value);
            else
                NativeObject.set_null(_objectHandle, _metadata.ColumnIndices[propertyName]);
        }

        protected void SetSingleValue(string propertyName, float value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeObject.set_float(_objectHandle, _metadata.ColumnIndices[propertyName], value);
        }

        protected void SetNullableSingleValue(string propertyName, float? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            if (value.HasValue)
                NativeObject.set_float(_objectHandle, _metadata.ColumnIndices[propertyName], value.Value);
            else
                NativeObject.set_null(_objectHandle, _metadata.ColumnIndices[propertyName]);
        }

        protected void SetDoubleValue(string propertyName, double value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeObject.set_double(_objectHandle, _metadata.ColumnIndices[propertyName], value);
        }

        protected void SetNullableDoubleValue(string propertyName, double? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            if (value.HasValue)
                NativeObject.set_double(_objectHandle, _metadata.ColumnIndices[propertyName], value.Value);
            else
                NativeObject.set_null(_objectHandle, _metadata.ColumnIndices[propertyName]);
        }

        protected void SetBooleanValue(string propertyName, bool value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeObject.set_bool(_objectHandle, _metadata.ColumnIndices[propertyName], MarshalHelpers.BoolToIntPtr(value));
        }

        protected void SetNullableBooleanValue(string propertyName, bool? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            if (value.HasValue)
                NativeObject.set_bool(_objectHandle, _metadata.ColumnIndices[propertyName], MarshalHelpers.BoolToIntPtr(value.Value));
            else
                NativeObject.set_null(_objectHandle, _metadata.ColumnIndices[propertyName]);
        }

        protected void SetDateTimeOffsetValue(string propertyName, DateTimeOffset value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var marshalledValue = value.ToRealmUnixTimeMilliseconds();
            NativeObject.set_timestamp_milliseconds(_objectHandle, _metadata.ColumnIndices[propertyName], marshalledValue);
        }

        protected void SetNullableDateTimeOffsetValue(string propertyName, DateTimeOffset? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            if (value.HasValue)
            {
                var marshalledValue = value.Value.ToRealmUnixTimeMilliseconds();
                NativeObject.set_timestamp_milliseconds(_objectHandle, _metadata.ColumnIndices[propertyName], marshalledValue);
            }
            else
                NativeObject.set_null(_objectHandle, _metadata.ColumnIndices[propertyName]);
        }

        // TODO make not generic
        protected void SetObjectValue<T>(string propertyName, T value) where T : RealmObject
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            if (value == null)
            {
                NativeObject.clear_link(_objectHandle, _metadata.ColumnIndices[propertyName]);
            }
            else
            {
                if (!value.IsManaged)
                    _realm.Manage(value);
                NativeObject.set_link(_objectHandle, _metadata.ColumnIndices[propertyName], (IntPtr)value.ObjectHandle.RowIndex);
            }
        }

        protected unsafe void SetByteArrayValue(string propertyName, byte[] value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            if (value == null)
            {
                NativeObject.set_null(_objectHandle, _metadata.ColumnIndices[propertyName]);
            }
            else if (value.Length == 0)
            {
                // empty byte arrays are expressed in terms of a BinaryData object with a dummy pointer and zero size
                // that's how core differentiates between empty and null buffers
                NativeObject.set_binary(_objectHandle, _metadata.ColumnIndices[propertyName], (IntPtr)0x1, IntPtr.Zero);
            }
            else
            {
                fixed (byte* buffer = value)
                {
                    NativeObject.set_binary(_objectHandle, _metadata.ColumnIndices[propertyName], (IntPtr)buffer, (IntPtr)value.LongLength);
                }
            }
        }

        #endregion

        /**
         * Shared factory to make an object in the realm from a known row
         * @param rowPtr may be null if a relationship lookup has failed.
        */
        internal RealmObject MakeRealmObject(Type objectType, IntPtr objectPtr) {
            if (objectPtr == IntPtr.Zero)
                return null;  // typically no related object
            var ret = _realm.Metadata[objectType].Helper.CreateInstance();
            var relatedHandle = Realm.CreateObjectHandle (objectPtr, _realm.SharedRealmHandle);
            ret._Manage(_realm, relatedHandle);
            return ret;
        }


        /// <summary>
        /// Compare objects with identity query for persistent objects.
        /// </summary>
        /// <remarks>Persisted RealmObjects map their properties directly to the realm with no caching so multiple instances of a given object always refer to the same store.</remarks>
        /// <param name="obj"></param>
        /// <returns>True when objects are the same memory object or refer to the same persisted object.</returns>
        public override bool Equals(object obj)
        {
            // If parameter is null, return false. 
            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            // Optimization for a common success case. 
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            // If run-time types are not exactly the same, return false. 
            if (this.GetType() != obj.GetType())
                return false;

            // standalone objects cannot participate in the same store check
            if (!IsManaged)
                return false;

            // Return true if the fields match. 
            // Note that the base class is not invoked because it is 
            // System.Object, which defines Equals as reference equality. 
            return ObjectHandle.Equals(((RealmObject)obj).ObjectHandle);
        }

    }
}

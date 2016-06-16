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
 
#include <realm.hpp>
#include "error_handling.hpp"
#include "marshalling.hpp"
#include "realm_export_decls.hpp"
#include "shared_linklist.hpp"
#include "timestamp_helpers.hpp"

using namespace realm;
using namespace realm::binding;

typedef realm::Row Object;

extern "C" {

REALM_EXPORT void object_destroy(Object* object_ptr)
{
    handle_errors([&]() {
        delete object_ptr;
    });
}

REALM_EXPORT size_t object_get_row_index(const Object* object_ptr)
{
    return handle_errors([&]() {
        return object_ptr->get_index();
    });
}

REALM_EXPORT size_t object_get_is_attached(const Object* object_ptr)
{
    return handle_errors([&]() {
        return bool_to_size_t(object_ptr->is_attached());
    });
}

REALM_EXPORT Row* object_get_link(Object* obj_ptr, size_t column_ndx)
{
  return handle_errors([&]() -> Row* {
    const size_t link_row_ndx = obj_ptr->get_link(column_ndx);
    if (link_row_ndx == realm::npos)
      return nullptr;
    auto target_table_ptr = obj_ptr->get_link_target(column_ndx);
    return new Row((*target_table_ptr)[link_row_ndx]);
  });
}

REALM_EXPORT SharedLinkViewRef* object_get_linklist(Object* obj_ptr, size_t column_ndx)
{
  return handle_errors([&]() -> SharedLinkViewRef* {
    SharedLinkViewRef sr = std::make_shared<LinkViewRef>(obj_ptr->get_linklist(column_ndx));
    return new SharedLinkViewRef{ sr };  // weird double-layering necessary to get a raw pointer to a shared_ptr
  });
}


REALM_EXPORT size_t object_get_bool(const Object* obj_ptr, size_t column_ndx)
{
    return handle_errors([&]() {
        return bool_to_size_t(obj_ptr->get_bool(column_ndx));
    });
}

// Return value is a boolean indicating whether result has a value (i.e. is not null). If true (1), ret_value will contain the actual value.
REALM_EXPORT size_t object_get_nullable_bool(const Object* obj_ptr, size_t column_ndx, size_t& ret_value)
{
    return handle_errors([&]() {
        if (obj_ptr->is_null(column_ndx))
            return 0;

        ret_value = bool_to_size_t(obj_ptr->get_bool(column_ndx));
        return 1;
    });
}

REALM_EXPORT int64_t object_get_int64(const Object* obj_ptr, size_t column_ndx)
{
    return handle_errors([&]() {
        return obj_ptr->get_int(column_ndx);
    });
}

REALM_EXPORT size_t object_get_nullable_int64(const Object* obj_ptr, size_t column_ndx, int64_t& ret_value)
{
    return handle_errors([&]() {
        if (obj_ptr->is_null(column_ndx))
            return 0;

        ret_value = obj_ptr->get_int(column_ndx);
        return 1;
    });
}

REALM_EXPORT float object_get_float(const Object* obj_ptr, size_t column_ndx)
{
    return handle_errors([&]() {
        return obj_ptr->get_float(column_ndx);
    });
}

REALM_EXPORT size_t object_get_nullable_float(const Object* obj_ptr, size_t column_ndx, float& ret_value)
{
    return handle_errors([&]() {
        if (obj_ptr->is_null(column_ndx))
            return 0;

        ret_value = obj_ptr->get_float(column_ndx);
        return 1;
    });
}

REALM_EXPORT double object_get_double(const Object* obj_ptr, size_t column_ndx)
{
    return handle_errors([&]() {
        return obj_ptr->get_double(column_ndx);
    });
}

REALM_EXPORT size_t object_get_nullable_double(const Object* obj_ptr, size_t column_ndx, double& ret_value)
{
    return handle_errors([&]() {
        if (obj_ptr->is_null(column_ndx))
            return 0;

        ret_value = obj_ptr->get_double(column_ndx);
        return 1;
    });
}

REALM_EXPORT size_t object_get_string(const Object* obj_ptr, size_t column_ndx, uint16_t * datatochsarp, size_t bufsize, bool* is_null)
{
    return handle_errors([&]() -> size_t {
        StringData fielddata = obj_ptr->get_string(column_ndx);
        if ((*is_null = fielddata.is_null()))
            return 0;
        
        return stringdata_to_csharpstringbuffer(fielddata, datatochsarp, bufsize);
    });
}

REALM_EXPORT size_t object_get_binary(const Object* obj_ptr, size_t column_ndx, const char*& return_buffer, size_t& return_size)
{
    return handle_errors([&]() {
        auto fielddata = obj_ptr->get_binary(column_ndx);

        if (fielddata.is_null())
            return 0;

        return_buffer = fielddata.data();
        return_size = fielddata.size();
        return 1;
    });
}

REALM_EXPORT int64_t object_get_timestamp_milliseconds(const Object* obj_ptr, size_t column_ndx)
{
    return handle_errors([&]() {
        return to_milliseconds(obj_ptr->get_timestamp(column_ndx));
    });
}

REALM_EXPORT size_t object_get_nullable_timestamp_milliseconds(const Object* obj_ptr, size_t column_ndx, int64_t& ret_value)
{
    return handle_errors([&]() {
        if (obj_ptr->is_null(column_ndx))
            return 0;

        ret_value = to_milliseconds(obj_ptr->get_timestamp(column_ndx));
        return 1;
    });
}

REALM_EXPORT void object_set_link(Object* obj_ptr, size_t column_ndx, size_t target_row_ndx)
{
    return handle_errors([&]() {
        obj_ptr->set_link(column_ndx, target_row_ndx);
    });
}

REALM_EXPORT void object_clear_link(Object* obj_ptr, size_t column_ndx)
{
    return handle_errors([&]() {
        obj_ptr->nullify_link(column_ndx);
    });
}

REALM_EXPORT void object_set_null(Object* obj_ptr, size_t column_ndx)
{
    return handle_errors([&]() {
        //if (!obj_ptr->is_nullable(column_ndx))
        //    throw new std::invalid_argument("Column is not nullable");

        obj_ptr->set_null(column_ndx);
    });
}

REALM_EXPORT void object_set_bool(Object* obj_ptr, size_t column_ndx, size_t value)
{
    return handle_errors([&]() {
        obj_ptr->set_bool(column_ndx, size_t_to_bool(value));
    });
}

REALM_EXPORT void object_set_int64(Object* obj_ptr, size_t column_ndx, int64_t value)
{
    return handle_errors([&]() {
        obj_ptr->set_int(column_ndx, value);
    });
}

REALM_EXPORT void object_set_int64_unique(Object* obj_ptr, size_t column_ndx, int64_t value)
{
    return handle_errors([&]() {
        obj_ptr->set_int_unique(column_ndx, value);
    });
}

REALM_EXPORT void object_set_float(Object* obj_ptr, size_t column_ndx, float value)
{
    return handle_errors([&]() {
        obj_ptr->set_float(column_ndx, value);
    });
}

REALM_EXPORT void object_set_double(Object* obj_ptr, size_t column_ndx, double value)
{
    return handle_errors([&]() {
        obj_ptr->set_double(column_ndx, value);
    });
}

REALM_EXPORT void object_set_string(Object* obj_ptr, size_t column_ndx, uint16_t* value, size_t value_len)
{
    return handle_errors([&]() {
        Utf16StringAccessor str(value, value_len);
        obj_ptr->set_string(column_ndx, str);
    });
}

REALM_EXPORT void object_set_string_unique(Object* obj_ptr, size_t column_ndx, uint16_t* value, size_t value_len)
{
    return handle_errors([&]() {
        Utf16StringAccessor str(value, value_len);
        obj_ptr->set_string_unique(column_ndx, str);
    });
}

REALM_EXPORT void object_set_binary(Object* obj_ptr, size_t column_ndx, char* value, size_t value_len)
{
    return handle_errors([&]() {
        obj_ptr->set_binary(column_ndx, BinaryData(value, value_len));
    });
}

REALM_EXPORT void object_set_timestamp_milliseconds(Object* obj_ptr, size_t column_ndx, int64_t value)
{
    return handle_errors([&]() {
        obj_ptr->set_timestamp(column_ndx, from_milliseconds(value));
    });
}
}   // extern "C"

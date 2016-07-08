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
#include <realm/lang_bind_helper.hpp>
#include "error_handling.hpp"
#include "realm_export_decls.hpp"
#include "marshalling.hpp"
#include "object-store/src/object_store.hpp"
#include "object-store/src/shared_realm.hpp"
#include "object-store/src/schema.hpp"
#include "object-store/src/binding_context.hpp"
#include <list>
#include "schema_cs.hpp"


using namespace realm;
using namespace realm::binding;

using NotifyRealmChangedT = void(*)(void* managed_realm_handle);
NotifyRealmChangedT notify_realm_changed = nullptr;

namespace realm {
namespace binding {

class CSharpBindingContext: public BindingContext {
public:
    CSharpBindingContext(void* managed_realm_handle) : m_managed_realm_handle(managed_realm_handle) {}

    void did_change(std::vector<ObserverState> const&, std::vector<void*> const&) override
    {
        notify_realm_changed(m_managed_realm_handle);
    }

private:
    void* m_managed_realm_handle;
};

}
}

extern "C" {

REALM_EXPORT void register_notify_realm_changed(NotifyRealmChangedT notifier)
{
    notify_realm_changed = notifier;
}

struct SchemaForManagedMarshalling
{
    Schema* handle;
    uint64_t schema_version;
    
    SchemaObject* objects;
    ObjectSchema** object_handles;
    int objects_len;
    
    SchemaProperty* properties;
};
    
typedef void (MigrationCallbackDelegate)(SharedRealm* old_realm, SchemaForManagedMarshalling old_schema, SharedRealm* new_realm, void* data);

struct Configuration
{
    uint16_t* path;
    size_t path_len;
    
    bool read_only;
    
    bool in_memory;
    
    char* encryption_key;
    
    Schema* schema;
    uint64_t schema_version;
};
    
REALM_EXPORT SharedRealm* shared_realm_open(const Configuration configuration, MigrationCallbackDelegate migrationCallback, void* migrationCallbackData)
{
    return handle_errors([&]() {
        Utf16StringAccessor pathStr(configuration.path, configuration.path_len);

        Realm::Config config;
        config.path = pathStr.to_string();
        config.read_only = configuration.read_only;
        config.in_memory = configuration.in_memory;

        // by definition the key is only allowwed to be 64 bytes long, enforced by C# code
        if (configuration.encryption_key != nullptr)
          config.encryption_key = std::vector<char>(configuration.encryption_key, configuration.encryption_key + 64);

        config.schema.reset(configuration.schema);
        config.schema_version = configuration.schema_version;

        if (migrationCallback) {
            config.migration_function = [=](SharedRealm old, SharedRealm current) {
                std::vector<SchemaObject> schema_objects;
                std::vector<ObjectSchema*> object_handles;
                std::vector<SchemaProperty> schema_properties;
                
                for (auto& object : *old->config().schema) {
                    schema_objects.push_back(SchemaObject::for_marshalling(object, schema_properties));
                    object_handles.push_back(&object);
                }
                
                SchemaForManagedMarshalling schema {
                    old->config().schema.get(),
                    old->config().schema_version,
                    
                    schema_objects.data(),
                    object_handles.data(),
                    static_cast<int>(schema_objects.size()),
                    
                    schema_properties.data()
                };
                
                migrationCallback(new SharedRealm(old), schema, new SharedRealm(current), migrationCallbackData);
            };
        }

        return new SharedRealm{Realm::get_shared_realm(config)};
    });
}


REALM_EXPORT void shared_realm_bind_to_managed_realm_handle(SharedRealm* realm, void* managed_realm_handle)
{
    handle_errors([&]() {
        (*realm)->m_binding_context = std::unique_ptr<realm::BindingContext>(new CSharpBindingContext(managed_realm_handle));
    });
}

REALM_EXPORT void shared_realm_destroy(SharedRealm* realm)
{
    handle_errors([&]() {
        delete realm;
    });
}

REALM_EXPORT Table* shared_realm_get_table(SharedRealm* realm, uint16_t* object_type, size_t object_type_len)
{
    return handle_errors([&]() {
        Group* g = (*realm)->read_group();
        Utf16StringAccessor str(object_type, object_type_len);

        std::string table_name = ObjectStore::table_name_for_object_type(str);
        return LangBindHelper::get_table(*g, table_name);
    });
}

REALM_EXPORT void shared_realm_begin_transaction(SharedRealm* realm)
{
    handle_errors([&]() {
        (*realm)->begin_transaction();
    });
}

REALM_EXPORT void shared_realm_commit_transaction(SharedRealm* realm)
{
    handle_errors([&]() {
        (*realm)->commit_transaction();
    });
}

REALM_EXPORT void shared_realm_cancel_transaction(SharedRealm* realm)
{
    handle_errors([&]() {
        (*realm)->cancel_transaction();
    });
}

REALM_EXPORT size_t shared_realm_is_in_transaction(SharedRealm* realm)
{
    return handle_errors([&]() {
        return bool_to_size_t((*realm)->is_in_transaction());
    });
}


REALM_EXPORT size_t shared_realm_is_same_instance(SharedRealm* lhs, SharedRealm* rhs)
{
    return handle_errors([&]() {
        return *lhs == *rhs;  // just compare raw pointers inside the smart pointers
    });
}

REALM_EXPORT size_t shared_realm_refresh(SharedRealm* realm)
{
    return handle_errors([&]() {
        return bool_to_size_t((*realm)->refresh());
    });
}

}

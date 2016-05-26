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

using NUnit.Framework;
using Realms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IntegrationTests.Shared
{

    public class Gnu : RealmObject
    {
        public string Name { get; set; }

        public int Age { get; set; }
    }

    public class GnuOwner : RealmObject
    {
        public string Name { get; set; }

        public Gnu Gnu { get; set; }
    }


    [TestFixture]
    public class StandAloneObjectTests
    {
        private Person _person;

        [SetUp]
        public void SetUp()
        {
            _person = new Person();
            Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);
        }

        [Test]
        public void PropertyGet()
        {
            string firstName = null;
            Assert.DoesNotThrow(() => firstName = _person.FirstName);
            Assert.That(string.IsNullOrEmpty(firstName));
        }

        [Test]
        public void PropertySet()
        {
            const string name = "John";
            Assert.DoesNotThrow(() => _person.FirstName = name);
            Assert.AreEqual(name, _person.FirstName);
        }

        [Test]
        public void AddToRealm()
        {
            _person.FirstName = "Arthur";
            _person.LastName = "Dent";
            _person.IsInteresting = true;

            using (var realm = Realm.GetInstance())
            {
                using (var transaction = realm.BeginWrite())
                {
                    realm.Manage(_person);
                    transaction.Commit();
                }

                Assert.That(_person.IsManaged);

                var p = realm.All<Person>().ToList().Single();
                Assert.That(p.FirstName, Is.EqualTo("Arthur"));
                Assert.That(p.LastName, Is.EqualTo("Dent"));
                Assert.That(p.IsInteresting);
            }
        }

        [Test]
        public void CreateRelatedThenManage()
        {
            Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);

            // create a standalone object
            var g = new Gnu();

            // set & read properties
            g.Name = "Rex";
            g.Age = 9;


            // realms are used to group data together
            var realm = Realm.GetInstance(); // create realm pointing to default file

            // save your object
            using (var transaction = realm.BeginWrite())
            {
                realm.Manage(g);
                transaction.Commit();
            }
        }

    }
}

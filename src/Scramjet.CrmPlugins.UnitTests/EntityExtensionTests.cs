using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xrm.Sdk;
using NUnit.Framework;
using Shouldly;

// ReSharper disable MemberCanBePrivate.Global

namespace Scramjet.CrmPlugins.UnitTests {
    [TestFixture]
    public class EntityExtensionTests {
        public static IEnumerable<Decimal?> TestMoneyValues {
            get {
                yield return 1.0m;
                yield return 0.0m;
                yield return -1.0m;
                yield return 0.000001m;
                yield return 1.111111m;
                yield return 123.45m;
                yield return -123.45m;
                yield return -0m;
                yield return 123.4567m;
                yield return null;
            }
        }

        public static IEnumerable<DateTime?> TestDateValues {
            get {
                yield return DateTime.MinValue;
                yield return DateTime.MaxValue;
                yield return new DateTime(2016, 3, 23, 16, 13, 15, 447);
                yield return DateTime.Parse("2016-02-29 23:59:59.999");
                yield return null;
            }
        }

        [Test]
        [TestCaseSource(nameof(TestDateValues))]
        public void Flatten_DateTime_Works(DateTime dt) {
            EntityExtensions.Flatten(dt).ShouldBe(dt);
        }

        public object MakeThing() {
            return (new {
                name = "thing",
                guid = Guid.Empty
            });
        }

        [Test]
        public void Things1() {
            var thing1 = MakeThing();
            var thing2 = new { name = "thing", guid = Guid.Empty };
            thing1.ShouldBe(thing2);
        }
        [Test]
        public void Things2() {
            var thing1 = MakeThing();
            thing1.ShouldBe(new { name = "thing", guid = Guid.Empty });
        }

        [Test]
        public void Flatten_EntityReference() {
            var er = new EntityReference("contact", Guid.Empty);
            var s = EntityExtensions.Flatten(er);

            s.ShouldBe(new {
                name = "contact",
                id = Guid.Empty
            });
        }
        [Test]
        public void Fnord() {
            Object value = new EntityReference("fnord", Guid.Empty);
            var f = new {
                name = ((EntityReference)value).LogicalName,
                id = ((EntityReference)value).Id
            };
            var p = EntityExtensions.Flatten(value);
            f.ShouldBe(p);
            p.ShouldBe(f);
            //f.ShouldBe(new {
            //    name = "fnord",
            //    id = Guid.Empty
            //});
        }

        [Test]
        [TestCaseSource(nameof(TestMoneyValues))]
        public void Flatten_Money(Decimal m) {
            var cash = new Money(m);
            var s = EntityExtensions.Flatten(cash);
            s.ShouldBe(m);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(Int32.MaxValue)]
        public void Flatten_OptionSetValue(Int32 v) {
            var osv = new OptionSetValue(v);
            EntityExtensions.Flatten(osv).ShouldBe(v);
        }
    }
}
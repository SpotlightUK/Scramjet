using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xrm.Sdk;
using NUnit.Framework;
using NUnit.Framework.Internal;
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

        [Test]
        public void Flatten_EntityReference() {
            var er = new EntityReference("contact", Guid.Empty);
            var s = EntityExtensions.Flatten(er);
            s.ShouldBe(new ScramjetEntityReference() {
                Name = "contact",
                Guid = Guid.Empty
            });
        }

        [Test]
        [TestCaseSource(nameof(TestMoneyValues))]
        public void Flatten_Money(decimal m) {
            var cash = new Money(m);
            var s = EntityExtensions.Flatten(cash);
            s.ShouldBe(m);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(Int32.MaxValue)]
        public void Flatten_OptionSetValue(int v) {
            var osv = new OptionSetValue(v);
            EntityExtensions.Flatten(osv).ShouldBe(v);
        }
    }
}
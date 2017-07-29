using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace uShip.Logging.Tests
{
    [TestFixture]
    public class FluentLoggerDataBuilderTestscs
    {
        private Logger.FluentLogDataBuilder ClassUnderTest { get; set; }

        [SetUp]
        public void InitDataDictionary()
        {
            //reinit the class, assuring a fresh FluentLogDataBuilder each test.
            ClassUnderTest = new Logger.FluentLogDataBuilder(null, null);
        }

        [Test]
        public void Can_map_object_with_primitives()
        {
            string testString = "test";
            int testInt = 32;
            long testLong = 64L;
            double testDouble = (double) 2.0;
            float testFloat = (float)3.0;

            var obj = new
            {
                StringProp = testString,
                IntegerProp = testInt,
                LongProp = testLong,
                DoubleProp = testDouble,
                FloatProp = testFloat                                          
            };
            ClassUnderTest.Data(obj);
            var objName = obj.GetType().Name + "_";
            Assert.AreEqual(ClassUnderTest._data[objName + "StringProp"], testString);
            Assert.AreEqual(ClassUnderTest._data[objName + "IntegerProp"], testInt);
            Assert.AreEqual(ClassUnderTest._data[objName + "LongProp"], testLong);
            Assert.AreEqual(ClassUnderTest._data[objName + "DoubleProp"], testDouble);
            Assert.AreEqual(ClassUnderTest._data[objName + "FloatProp"], testFloat);
            ClassUnderTest._data.Count.Should().Be(5);
        }

        [Test]
        public void Can_map_object_with_complex_types()
        {
            string testString = "test";
            int testInt = 32;
            long testLong = 64L;
            double testDouble = (double)2.0;
            float testFloat = (float)3.0;

            var obj = new
            {
                StringProp = testString,
                IntegerProp = testInt,
                LongProp = testLong,
                DoubleProp = testDouble,
                FloatProp = testFloat,
                ComplexClass = new {
                    NestedProperty = "Oh no!"
                }
            };
            ClassUnderTest.Data(obj);
            var objName = obj.GetType().Name + "_";
            Assert.AreEqual(ClassUnderTest._data[objName + "StringProp"], testString);
            Assert.AreEqual(ClassUnderTest._data[objName + "IntegerProp"], testInt);
            Assert.AreEqual(ClassUnderTest._data[objName + "LongProp"], testLong);
            Assert.AreEqual(ClassUnderTest._data[objName + "DoubleProp"], testDouble);
            Assert.AreEqual(ClassUnderTest._data[objName + "FloatProp"], testFloat);
            ClassUnderTest._data.Count.Should().Be(5);
            ClassUnderTest._data.ContainsKey(objName + "ComplexClass").Should().Be(false);
            ClassUnderTest._data.ContainsKey(objName + "ComplexClass_NestedProperty").Should().Be(false);
        }

        [Test]
        public void Can_map_object_with_dates()
        {
            string testString = "test";
            int testInt = 32;
            long testLong = 64L;
            double testDouble = (double)2.0;
            float testFloat = (float)3.0;
            DateTime testDate = new DateTime(66, 6, 6);
            DateTimeOffset testDateOffset = new DateTimeOffset(testDate, TimeSpan.FromHours(2));

            var obj = new
            {
                StringProp = testString,
                IntegerProp = testInt,
                LongProp = testLong,
                DoubleProp = testDouble,
                FloatProp = testFloat,
                ComplexClass = new
                {
                    NestedProperty = "Oh no!"
                },
                DateTimeProp = testDate,
                DateTimeOffsetProp = testDateOffset
            };
            ClassUnderTest.Data(obj);
            var objName = obj.GetType().Name + "_";
            Assert.AreEqual(ClassUnderTest._data[objName + "StringProp"], testString);
            Assert.AreEqual(ClassUnderTest._data[objName + "IntegerProp"], testInt);
            Assert.AreEqual(ClassUnderTest._data[objName + "LongProp"], testLong);
            Assert.AreEqual(ClassUnderTest._data[objName + "DoubleProp"], testDouble);
            Assert.AreEqual(ClassUnderTest._data[objName + "FloatProp"], testFloat);
            Assert.AreEqual(ClassUnderTest._data[objName + "DateTimeProp"], testDate);
            Assert.AreEqual(ClassUnderTest._data[objName + "DateTimeOffsetProp"], testDateOffset);
            ClassUnderTest._data.Count.Should().Be(7);
            ClassUnderTest._data.ContainsKey(objName + "ComplexClass").Should().Be(false);
            ClassUnderTest._data.ContainsKey(objName + "ComplexClass_NestedProperty").Should().Be(false);
        }
    }
}

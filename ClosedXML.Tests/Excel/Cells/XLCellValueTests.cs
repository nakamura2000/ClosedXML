﻿using NUnit.Framework;
using System;
using System.Globalization;
using ClosedXML.Excel;
using ClosedXML.Excel.CalcEngine;

namespace ClosedXML.Tests.Excel.Cells
{
    [TestFixture]
    public class XLCellValueTests
    {
        [Test]
        public void Creation_Blank()
        {
            XLCellValue blank = Blank.Value;
            Assert.AreEqual(XLDataType.Blank, blank.Type);
            Assert.True(blank.IsBlank);
        }

        [Test]
        public void Creation_Boolean()
        {
            XLCellValue logical = true;
            Assert.AreEqual(XLDataType.Boolean, logical.Type);
            Assert.True(logical.GetBoolean());
            Assert.True(logical.IsBoolean);
        }

        [Test]
        public void Creation_Number()
        {
            XLCellValue number = 14.0;
            Assert.AreEqual(XLDataType.Number, number.Type);
            Assert.True(number.IsNumber);
            Assert.AreEqual(14.0, number.GetNumber());
        }

        [TestCase(Double.NaN)]
        [TestCase(Double.PositiveInfinity)]
        [TestCase(Double.NegativeInfinity)]
        public void Creation_Number_CantBeNonNumber(Double nonNumber)
        {
            Assert.Throws<ArgumentException>(() => _ = (XLCellValue)nonNumber);
        }

        [Test]
        public void Creation_Text()
        {
            XLCellValue text = "Hello World";
            Assert.AreEqual(XLDataType.Text, text.Type);
            Assert.AreEqual("Hello World", text.GetText());
        }

        [Test]
        public void Creation_Text_CantBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => _ = (XLCellValue)(string)null);
        }

        [Test]
        public void Creation_Text_HasLimitedLength()
        {
            var longText = new string('A', 32768);
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = (XLCellValue)longText);
        }

        [Test]
        public void Creation_Error()
        {
            XLCellValue error = XLError.NumberInvalid;
            Assert.AreEqual(XLDataType.Error, error.Type);
            Assert.True(error.IsError);
            Assert.AreEqual(XLError.NumberInvalid, error.GetError());
        }

        [Test]
        public void Creation_DateTime()
        {
            XLCellValue dateTime = new DateTime(2021, 1, 1);
            Assert.AreEqual(XLDataType.DateTime, dateTime.Type);
            Assert.True(dateTime.IsDateTime);
            Assert.AreEqual(new DateTime(2021, 1, 1), dateTime.GetDateTime());
        }

        [Test]
        public void Creation_TimeSpan()
        {
            XLCellValue dateTime = new TimeSpan(10, 1, 2, 3, 456);
            Assert.AreEqual(XLDataType.TimeSpan, dateTime.Type);
            Assert.True(dateTime.IsTimeSpan);
            Assert.AreEqual(new TimeSpan(10, 1, 2, 3, 456), dateTime.GetTimeSpan());
        }

        [Test]
        public void UnifiedNumber_IsFormOf_Number_DateTime_And_TimeSpan()
        {
            XLCellValue value = Blank.Value;
            Assert.False(value.IsUnifiedNumber);

            value = true;
            Assert.False(value.IsUnifiedNumber);

            value = 14;
            Assert.True(value.IsUnifiedNumber);
            Assert.AreEqual(14.0, value.GetUnifiedNumber());

            value = new DateTime(1900, 1, 1);
            Assert.True(value.IsUnifiedNumber);
            Assert.AreEqual(1.0, value.GetUnifiedNumber());

            value = new TimeSpan(2, 12, 0, 0);
            Assert.True(value.IsUnifiedNumber);
            Assert.AreEqual(2.5, value.GetUnifiedNumber());

            value = "Text";
            Assert.False(value.IsUnifiedNumber);

            value = XLError.CellReference;
            Assert.False(value.IsUnifiedNumber);
        }

        [TestCase("1900-01-01", 1)]
        [TestCase("1900-01-02", 2)]
        [TestCase("1900-02-01", 32)]
        [TestCase("1900-02-28", 59)] // Excel assumes 1900 was a leap year and 29.1.1900 existed
        [TestCase("1900-03-01", 61)]
        [TestCase("2017-01-01", 42736)]
        public void SerialDateTime(string dateString, double expectedSerial)
        {
            XLCellValue date = DateTime.Parse(dateString);
            Assert.AreEqual(expectedSerial, date.GetUnifiedNumber());
        }

        [Test]
        [Culture("cs-CZ")]
        public void ToString_RespectsCulture()
        {
            XLCellValue v = Blank.Value;
            Assert.AreEqual(String.Empty, v.ToString());

            v = true;
            Assert.AreEqual("TRUE", v.ToString());

            v = 25.4;
            Assert.AreEqual("25,4", v.ToString());

            v = "Hello";
            Assert.AreEqual("Hello", v.ToString());

            v = XLError.IncompatibleValue;
            Assert.AreEqual("#VALUE!", v.ToString());

            v = new DateTime(1900, 1, 2);
            Assert.AreEqual("02.01.1900 0:00:00", v.ToString());

            v = new DateTime(1900, 3, 1, 4, 10, 5);
            Assert.AreEqual("01.03.1900 4:10:05", v.ToString());

            v = new TimeSpan(4, 5, 6, 7, 82);
            Assert.AreEqual("101:06:07,082", v.ToString());
        }

        [Test]
        public void TryConvert_Blank()
        {
            XLCellValue value = Blank.Value;
            Assert.True(value.TryConvert(out Blank blank));
            Assert.AreEqual(Blank.Value, blank);

            value = String.Empty;
            Assert.True(value.TryConvert(out blank));
            Assert.AreEqual(Blank.Value, blank);
        }

        [Test]
        public void TryConvert_Boolean()
        {
            XLCellValue value = true;
            Assert.True(value.TryConvert(out Boolean boolean));
            Assert.True(boolean);

            value = "True";
            Assert.True(value.TryConvert(out boolean));
            Assert.True(boolean);

            value = "False";
            Assert.True(value.TryConvert(out boolean));
            Assert.False(boolean);

            value = 0;
            Assert.True(value.TryConvert(out boolean));
            Assert.False(boolean);

            value = 0.001;
            Assert.True(value.TryConvert(out boolean));
            Assert.True(boolean);
        }

        [Test]
        public void TryConvert_Number()
        {
            var c = CultureInfo.GetCultureInfo("cs-CZ");
            XLCellValue value = 5;
            Assert.True(value.TryConvert(out Double number, c));
            Assert.AreEqual(5.0, number);

            value = "1,5";
            Assert.True(value.TryConvert(out number, c));
            Assert.AreEqual(1.5, number);

            value = "1 1/4";
            Assert.True(value.TryConvert(out number, c));
            Assert.AreEqual(1.25, number);

            value = "3.1.1900";
            Assert.True(value.TryConvert(out number, c));
            Assert.AreEqual(3, number);

            value = true;
            Assert.True(value.TryConvert(out number, c));
            Assert.AreEqual(1.0, number);

            value = false;
            Assert.True(value.TryConvert(out number, c));
            Assert.AreEqual(0.0, number);

            value = new DateTime(2020, 4, 5, 10, 14, 5);
            Assert.True(value.TryConvert(out number, c));
            Assert.AreEqual(43926.42644675926, number);

            value = new TimeSpan(18, 0, 0);
            Assert.True(value.TryConvert(out number, c));
            Assert.AreEqual(0.75, number);
        }

        [Test]
        public void TryConvert_DateTime()
        {
            XLCellValue v = new DateTime(2020, 1, 1);
            Assert.True(v.TryConvert(out DateTime dt));
            Assert.AreEqual(new DateTime(2020, 1, 1), dt);

            var lastSerialDate = 2958465;
            v = lastSerialDate;
            Assert.True(v.TryConvert(out dt));
            Assert.AreEqual(new DateTime(9999, 12, 31), dt);

            v = lastSerialDate + 1;
            Assert.False(v.TryConvert(out dt));

            v = new TimeSpan(14, 0, 0, 0);
            Assert.True(v.TryConvert(out dt));
            Assert.AreEqual(new DateTime(1900, 1, 14), dt);
        }

        [Test]
        public void TryConvert_TimeSpan()
        {
            var c = CultureInfo.GetCultureInfo("cs-CZ");
            XLCellValue v = new TimeSpan(10, 15, 30);
            Assert.True(v.TryConvert(out TimeSpan ts, c));
            Assert.AreEqual(new TimeSpan(10, 15, 30), ts);

            v = "26:15:30,5";
            Assert.True(v.TryConvert(out ts, c));
            Assert.AreEqual(new TimeSpan(1, 2, 15, 30, 500), ts);

            v = 0.75;
            Assert.True(v.TryConvert(out ts, c));
            Assert.AreEqual(new TimeSpan(18, 0, 0), ts);
        }
    }
}

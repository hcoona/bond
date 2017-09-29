namespace UnitTest.Convert
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Bond;
    using NUnit.Framework;
    using Bond.IO.Safe;
    using Bond.Protocols;

    [global::Bond.Schema]
    public partial class Decimals
    {
        [global::Bond.Id(0), global::Bond.Type(typeof(global::Bond.Tag.blob))]
        public System.Decimal Price { get; set; }

        [global::Bond.Id(1), global::Bond.Type(typeof(List<global::Bond.Tag.blob>))]
        public List<decimal> Numbers { get; set; }

        public Decimals()
            : this("UnitTest.Convert.Decimals", "Decimals")
        { }

        protected Decimals(string fullName, string name)
        {
            Price = new decimal();
            Numbers = new List<decimal>();
        }
    }

    public static class BondTypeAliasConverter
    {
        public static int ConvertToDecimalCount = 0;
        public static int ConvertToArraySegmentCount = 0;

        public static decimal Convert(ArraySegment<byte> value, decimal unused)
        {
            var bits = new int[value.Count / sizeof(int)];
            Buffer.BlockCopy(value.Array, value.Offset, bits, 0, bits.Length * sizeof(int));

            Interlocked.Increment(ref ConvertToDecimalCount);

            return new decimal(bits);
        }

        public static ArraySegment<byte> Convert(decimal value, ArraySegment<byte> unused)
        {
            var bits = decimal.GetBits(value);
            var data = new byte[bits.Length * sizeof(int)];
            Buffer.BlockCopy(bits, 0, data, 0, data.Length);

            Interlocked.Increment(ref ConvertToArraySegmentCount);

            return new ArraySegment<byte>(data);
        }
    }


    [TestFixture]
    public class TypeAliasTests
    {
        [Test]
        public void CorrectSerializeConvertCount()
        {
            var foo = new Decimals();
            foo.Price = new decimal(19.91);

            var outputStream = new OutputBuffer();
            var writer = new CompactBinaryWriter<OutputBuffer>(outputStream);
            var serializer = new Serializer<CompactBinaryWriter<OutputBuffer>>(typeof(Decimals));

            serializer.Serialize(foo, writer);

            Assert.AreEqual(1, BondTypeAliasConverter.ConvertToArraySegmentCount);
            Assert.AreEqual(0, BondTypeAliasConverter.ConvertToDecimalCount);
        }

        [Test]
        // this test currently fails
        public void CorrectSerializeListConvertCount()
        {
            var foo = new Decimals();
            foo.Price = new decimal(19.91);
            foo.Numbers = new List<decimal> { new decimal(10.10), new decimal(11.11) };

            var outputStream = new OutputBuffer();
            var writer = new CompactBinaryWriter<OutputBuffer>(outputStream);
            var serializer = new Serializer<CompactBinaryWriter<OutputBuffer>>(typeof(Decimals));

            serializer.Serialize(foo, writer);

            Assert.AreEqual(1+foo.Numbers.Count, BondTypeAliasConverter.ConvertToArraySegmentCount);
            Assert.AreEqual(0, BondTypeAliasConverter.ConvertToDecimalCount);
        }
    }
}

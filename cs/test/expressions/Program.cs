namespace ExpressionsTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Bond;
    using Bond.Expressions;
    using Bond.Protocols;
    using Bond.IO.Unsafe;
    using Bond.Internal.Reflection;

    internal static class DebugViewHelper
    {
        static readonly PropertyInfo debugView;

        static DebugViewHelper()
        {
            debugView = typeof(Expression).GetDeclaredProperty("DebugView", typeof(string));
        }

        public static string ToString(Expression expression)
        {
            return (string)debugView.GetValue(expression);
        }

        public static string ToString(IEnumerable<Expression> expressions)
        {
            return string.Concat(expressions.Select(ToString));
        }
    }

    interface IDebugView
    {
        string DebugView { get; }
    }

    internal class Transcoder<R, W> : IDebugView
    {
        readonly string debugView;
        readonly Action<R, W>[] transcode = null;

        public Transcoder(Type type)
            : this(type, ParserFactory<R>.Create(type))
        {}

        public Transcoder(RuntimeSchema schema)
            : this(schema, ParserFactory<R>.Create(schema))
        { }

        public Transcoder()
            : this(new RuntimeSchema())
        {}

        Transcoder(Type type, IParser parser)
        {
            var serializerTransform = SerializerGeneratorFactory<R, W>.Create(
                (r, w, i) => transcode[i](r, w), type);
            var expressions = serializerTransform.Generate(parser);
            debugView = DebugViewHelper.ToString(expressions);
        }

        Transcoder(RuntimeSchema schema, IParser parser)
        {
            var serializerTransform = SerializerGeneratorFactory<R, W>.Create(
                (r, w, i) => transcode[i](r, w), schema);
            var expressions = serializerTransform.Generate(parser);
            debugView = DebugViewHelper.ToString(expressions);
        }

        string IDebugView.DebugView { get { return debugView; } }
    }

    internal class DeserializerDebugView<R> : IDebugView
    {
        readonly string debugView;
        readonly Func<R, object>[] deserialize = null;

        public DeserializerDebugView(Type type)
        {
            var parser = ParserFactory<R>.Create(type);
            var expressions = new DeserializerTransform<R>(
                (r, i) => deserialize[i](r))
                .Generate(parser, type);
            debugView = DebugViewHelper.ToString(expressions);
        }

        string IDebugView.DebugView { get { return debugView; } }
    }

    internal class SerializerDebugView<W> : IDebugView
    {
        readonly string debugView;
        readonly Action<object, W>[] serialize = null;

        public SerializerDebugView(Type type)
        {
            var parser = new ObjectParser(type);
            var serializerTransform = SerializerGeneratorFactory<object, W>.Create(
                (o, w, i) => serialize[i](o, w), type);
            var expressions = serializerTransform.Generate(parser);
            debugView = DebugViewHelper.ToString(expressions);
        }

        string IDebugView.DebugView { get { return debugView; } }
    }

    static class Program
    {
        static void Write(string name, IDebugView codegen)
        {
            using (var file = File.Create(name))
            {
                using (var sw = new StreamWriter(file))
                {
                    sw.Write(codegen.DebugView);
                }
            }
        }

        static void Main(string[] args)
        {
            /*
            Write("TranscodeCBCB.expressions",
                new Transcoder<CompactBinaryReader<InputStream>, CompactBinaryWriter<OutputStream>>());

            Write("TranscodeSPCB.expressions",
                new Transcoder<SimpleBinaryReader<InputStream>, CompactBinaryWriter<OutputStream>>(typeof(Example)));

            Write("TranscodeCBSP.expressions",
                new Transcoder<CompactBinaryReader<InputStream>, SimpleBinaryWriter<OutputStream>>(typeof(Example)));

                */
            Write("DeserializeCB.expressions",
                new DeserializerDebugView<CompactBinaryReader<InputStream>>(typeof(HourlyElementAdUsage)));

            /*
            Write("DeserializeSP.expressions",
                new DeserializerDebugView<SimpleBinaryReader<InputStream>>(typeof(Example)));

            Write("DeserializeXml.expressions",
                new DeserializerDebugView<SimpleXmlReader>(typeof(Example)));

            Write("DeserializeJson.expressions",
                new DeserializerDebugView<SimpleJsonReader>(typeof(Example)));

            Write("SerializeSP.expressions",
                new SerializerDebugView<SimpleBinaryWriter<OutputStream>>(typeof(Example)));
                */

            Write("SerializeCB.expressions",
                new SerializerDebugView<CompactBinaryWriter<OutputStream>>(typeof(HourlyElementAdUsage)));

            /*
            Write("SerializeCB_with.expressions",
                new SerializerDebugView<CompactBinaryWriter<OutputStream>>(typeof(StructWithBlobs)));
                */

            /*
            Write("SerializeXml.expressions",
                new SerializerDebugView<SimpleXmlWriter>(typeof(Example)));
                */

            var temp = new HourlyElementAdUsage();
            temp.Numbers = new List<decimal> { new decimal(12.12), new decimal(23.23)};
            //temp.TOTALAMOUNT = new decimal(23.23);

            var outputStream = new OutputBuffer();
            var writer = new CompactBinaryWriter<OutputBuffer>(outputStream);
            var serializer = new Serializer<CompactBinaryWriter<OutputBuffer>>(typeof(HourlyElementAdUsage));

            serializer.Serialize(temp, writer);

            HourlyElementAdUsage item = new HourlyElementAdUsage();

            var inputStream = new InputBuffer(outputStream.Data);
            var reader = new CompactBinaryReader<InputBuffer>(inputStream);
            var deserializer = new Deserializer<CompactBinaryReader<InputBuffer>>(typeof(HourlyElementAdUsage));

            item = deserializer.Deserialize<HourlyElementAdUsage>(reader);


            Console.ReadKey();

        }
    }

    [global::Bond.Schema]
    [System.CodeDom.Compiler.GeneratedCode("gbc", "0.10.0.0")]
    public partial class HourlyElementAdUsage
    {
        /*
        [global::Bond.Id(16), global::Bond.Type(typeof(global::Bond.Tag.blob))]
        public System.Decimal TOTALAMOUNT { get; set; }
        */

        [global::Bond.Id(1), global::Bond.Type(typeof(List<global::Bond.Tag.blob>))]
        public List<decimal> Numbers { get; set; }

        public HourlyElementAdUsage()
            : this("FastBITest.Entity.HourlyElementAdUsage", "HourlyElementAdUsage")
        { }

        protected HourlyElementAdUsage(string fullName, string name)
        {
            //TOTALAMOUNT = new System.Decimal();
            Numbers = new List<decimal>();
        }
    }

    [global::Bond.Schema]
    [System.CodeDom.Compiler.GeneratedCode("gbc", "0.10.0.0")]
    public partial class StructWithBlobs
    {
        [global::Bond.Id(0)]
        public System.ArraySegment<byte> b { get; set; }

        [global::Bond.Id(1)]
        public List<System.ArraySegment<byte>> lb { get; set; }

        [global::Bond.Id(2), global::Bond.Type(typeof(global::Bond.Tag.nullable<System.ArraySegment<byte>>))]
        public System.ArraySegment<byte> nb { get; set; }

        public StructWithBlobs()
            : this("UnitTest.StructWithBlobs", "StructWithBlobs")
        {}

        protected StructWithBlobs(string fullName, string name)
        {
            b = new System.ArraySegment<byte>();
            lb = new List<System.ArraySegment<byte>>();
        }
    }
    public static class BondTypeAliasConverter
    {
        public static decimal Convert(ArraySegment<byte> value, decimal unused)
        {
            var bits = new int[value.Count / sizeof(int)];
            Buffer.BlockCopy(value.Array, value.Offset, bits, 0, bits.Length * sizeof(int));
            return new decimal(bits);
        }

        public static ArraySegment<byte> Convert(decimal value, ArraySegment<byte> unused)
        {
            var bits = decimal.GetBits(value);
            var data = new byte[bits.Length * sizeof(int)];
            Buffer.BlockCopy(bits, 0, data, 0, data.Length);
            return new ArraySegment<byte>(data);
        }

        public static long Convert(DateTime value, long unused)
        {
            return value.Ticks;
        }

        public static DateTime Convert(long value, DateTime unused)
        {
            return new DateTime(value);
        }
    }


}

using System;

namespace DickinsonBros.SQL.Runner.Services.AccountDB.Models
{
    public class MixedDTO
    {
        public bool Bool { get; set; }
        public int Int { get; set; }
        public float Float { get; set; }
        public double Double { get; set; }
        public SampleEnum SampleEnum { get; set; }
        public char Char { get; set; }
        public byte Byte { get; set; }
        public byte[] ByteArray { get; set; }
        public Guid Guid { get; set; }
        public System.DateTime DateTime { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public string String { get; set; }
        public bool? NullValueType { get; set; }
        public string NullString { get; set; }

    }
}

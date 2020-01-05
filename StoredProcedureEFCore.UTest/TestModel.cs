using System;

namespace StoredProcedureEFCore.UTest
{
    internal class TestModel : ICloneable
    {
        public TestModel()
        {
        }

        public TestModel(sbyte sByte, char c, short s, int i, long l, byte b, ushort us, uint ui, ulong ul, float f, double d, bool bo, string str, DateTime date, Decimal dec, YN en)
        {
            Sb = sByte;
            C = c;
            S = s;
            I = i;
            L = l;
            B = b;
            Us = us;
            Ui = ui;
            Ul = ul;
            F = f;
            D = d;
            Bo = bo;
            Str = str;
            Date = date;
            Dec = dec;
            En = en;
        }


        public sbyte Sb { get; set; }
        public char C { get; set; }
        public short S { get; set; }
        public int I { get; set; }
        public long L { get; set; }

        public byte B { get; set; }
        public ushort Us { get; set; }
        public uint Ui { get; set; }
        public ulong Ul { get; set; }

        public float F { get; set; }
        public double D { get; set; }

        public bool Bo { get; set; }

        public string Str { get; set; }

        public DateTime Date { get; set; }
        public Decimal Dec { get; set; }
        public YN En { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    internal enum YN
    {
        Yes = 1,
        No = 2,
        Perhaps = 3
    }
}

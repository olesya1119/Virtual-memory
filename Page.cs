using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virtual_memory
{
    internal struct Page
    {
        private bool modify;
        private DateTime time;
        private byte number;
        private byte[] bitsArray;
        private int[] data;

        public bool Modify { get { return modify; } set { modify = value; } }
        public DateTime Time { get { return time; } set { time = value; } }
        public byte Number { get { return number; } set { number = value; } }
        public byte[] BitsArray { get { return bitsArray; } set { bitsArray = value; } }
        public int[] Data { get { return data; } set { data = value; } }


        public Page(byte number)
        {
            modify = false;
            time = DateTime.Now;
            this.number = number;
            bitsArray = new byte[16];
            data = new int[128];

            for (int i = 0; i < 128; i++) data[i] = 0;
            for(int i = 0; i < 16; i++) bitsArray[i] = 0;

        }
    }
}

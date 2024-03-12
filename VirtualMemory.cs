using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virtual_memory
{
    internal class VirtualMemory
    {
        private FileStream fileStream;
        private Page[] pages;

        public VirtualMemory(uint arraySize = 10000, string filename = "file.bin")
        {
            if (arraySize < 10000) { throw new ArgumentException("Размер массива не может быть меньше 10000!"); }

            if (File.Exists(filename) == false)
            {
                fileStream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);

                fileStream.Write(new byte[2] { (byte)'V', (byte)'M' }, 0, 2);
                for (int i = 0; i < arraySize; i++) fileStream.Write(new byte[4] { 0, 0, 0, 0 }, 0, 4);

                pages = new Page[(int)Math.Ceiling((decimal)arraySize / 128)];
                for (byte i = 0; i < pages.Length; i++) pages[i] = new Page(i);
            }

            else fileStream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }

        public void UpdateFile()
        {

        }

        public int this[int index]
        {
            get
            {
                int numberOfPage = (int)Math.Ceiling((decimal)index / 128);
                int indexOnPage = numberOfPage * 128 - index;
                Page page = pages[numberOfPage];

                return page.Data[indexOnPage];
            }
            set
            {
                int numberOfPage = (int)Math.Ceiling((decimal)index / 128);
                int indexOnPage = numberOfPage * 128 - index;
                Page page = pages[numberOfPage];

                page.Data[indexOnPage] = value;
                page.Modify = true;
                page.Time = DateTime.Now;

                //Работа с битовой картой
                int indexInBitsArray = indexOnPage / 8;
                string BinaryCode = Convert.ToString(page.BitsArray[indexInBitsArray], 2);
                if (BinaryCode[indexInBitsArray%8] != '1') {
                    page.BitsArray[indexInBitsArray] += (byte)Math.Pow(2, (double)indexInBitsArray % 8);
                }


            }
        }


    }
}

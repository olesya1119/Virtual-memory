using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virtual_memory
{
    using System;
    using System.IO;

    public class VirtualMemoryManager
    {
        private const string Signature = "VM";
        private const int PageSize = 512;
        private const int BufferSize = 3;

        private string fileName;
        private FileStream fileStream;
        private Page[] buffer;

        public VirtualMemoryManager(string fileName = "virtual_memory.bin", long arraySize)
        {
            this.fileName = fileName;
            buffer = new Page[BufferSize];

            bool fileExists = File.Exists(fileName);
            fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            if (!fileExists)
            {
                // Write signature and fill the file with zeros
                byte[] signatureBytes = System.Text.Encoding.ASCII.GetBytes(Signature);
                fileStream.Write(signatureBytes, 0, signatureBytes.Length);

                long pageCount = (arraySize + PageSize - 1) / PageSize;
                for (int i = 0; i < pageCount; i++)
                {
                    byte[] pageData = new byte[PageSize];
                    fileStream.Write(pageData, 0, pageData.Length);
                }
            }

            // Read the initial pages into the buffer
            for (int i = 0; i < BufferSize; i++)
            {
                buffer[i] = ReadPage(i);
            }
        }

        private Page ReadPage(int pageIndex)
        {
            byte[] pageData = new byte[PageSize];
            fileStream.Seek(pageIndex * PageSize + Signature.Length, SeekOrigin.Begin);
            fileStream.Read(pageData, 0, pageData.Length);

            Page page = new Page();
            page.Index = pageIndex;
            page.Status = 0;
            page.LastAccessTime = DateTime.Now;
            page.Bitmap = new byte[PageSize / 8];
            page.Values = new int[PageSize / sizeof(int)];

            Buffer.BlockCopy(pageData, 0, page.Bitmap, 0, page.Bitmap.Length);
            Buffer.BlockCopy(pageData, page.Bitmap.Length, page.Values, 0, page.Values.Length);

            return page;
        }

        private void WritePage(Page page)
        {
            byte[] pageData = new byte[PageSize];
            Buffer.BlockCopy(page.Bitmap, 0, pageData, 0, page.Bitmap.Length);
            Buffer.BlockCopy(page.Values, 0, pageData, page.Bitmap.Length, page.Values.Length);

            fileStream.Seek(page.Index * PageSize + Signature.Length, SeekOrigin.Begin);
            fileStream.Write(pageData, 0, pageData.Length);
        }

        private int GetPageIndex(int index)
        {
            return index / (PageSize / sizeof(int));
        }

        private int GetPageOffset(int index)
        {
            return index % (PageSize / sizeof(int));
        }

        public int? GetPageInBuffer(int index)
        {
            for (int i = 0; i < BufferSize; i++)
            {
                if (buffer[i] != null && buffer[i].ContainsIndex(index))
                {
                    return i;
                }
            }
            return null;
        }

        public int? GetPageIndexInBuffer(int index)
        {
            int? bufferIndex = GetPageInBuffer(index);
            if (bufferIndex.HasValue)
            {
                return buffer[bufferIndex.Value].GetPageIndex(index);
            }
            return null;
        }

        public int Read(int index)
        {
            int? bufferIndex = GetPageInBuffer(index);
            if (bufferIndex.HasValue)
            {
                Page page = buffer[bufferIndex.Value];
                page.LastAccessTime = DateTime.Now;
                return page.Values[page.GetPageOffset(index)];
            }
            return -1;
        }

        public bool Write(int index, int value)
        {
            int? bufferIndex = GetPageInBuffer(index);
            if (bufferIndex.HasValue)
            {
                Page page = buffer[bufferIndex.Value];
                page.LastAccessTime = DateTime.Now;
                page.Values[page.GetPageOffset(index)] = value;
                page.Status = 1;
                return true;
            }
            return false;
        }

        public int this[int index]
        {
            get { return Read(index); }
            set { Write(index, value); }
        }

        public void Dispose()
        {
            // Write modified pages back to the file
            for (int i = 0; i < BufferSize; i++)
            {
                if (buffer[i] != null && buffer[i].Status == 1)
                {
                    WritePage(buffer[i]);
                }
            }

            fileStream.Dispose();
        }

        private class Page
        {
            public int Index { get; set; }
            public byte Status { get; set; }
            public DateTime LastAccessTime { get; set; }
            public byte[] Bitmap { get; set; }
            public int[] Values { get; set; }

            public bool ContainsIndex(int index)
            {
                int pageIndex = GetPageIndex(index);
                return pageIndex == Index;
            }

            public int GetPageIndex(int index)
            {
                return index % (PageSize / sizeof(int));
            }
        }
    }

}

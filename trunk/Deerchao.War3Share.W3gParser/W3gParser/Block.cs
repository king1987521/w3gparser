using System.IO;
using System.IO.Compression;
using Deerchao.War3Share.Utility;

namespace Deerchao.War3Share.W3gParser
{
    internal class W3gBlock
    {
        ushort dataSize;
        ushort decompressSize;
        uint unknown;
        byte[] decopressedData;

        public byte[] DecopressedData
        {
            get { return decopressedData; }
            set { decopressedData = value; }
        }

        public static W3gBlock Parse(BinaryReader reader)
        {
            W3gBlock b = new W3gBlock();

            b.dataSize = reader.ReadUInt16();
            b.decompressSize = reader.ReadUInt16();
            b.unknown = reader.ReadUInt32();
            ProcessCompressedData(reader, b);

            return b;
        }

        private static void ProcessCompressedData(BinaryReader reader, W3gBlock b)
        {
            byte[] compressedData = reader.ReadBytes(b.dataSize);
            compressedData[2] += 1;
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(compressedData, 2, compressedData.Length - 2 - 4);

                ms.Position = 0;
                using (DeflateStream zipStream = new DeflateStream(ms, CompressionMode.Decompress))
                {
                    byte[] temp = new byte[b.decompressSize + 100];
                    int length = BinaryHelper.ReadAllBytesFromStream(zipStream, temp);

                    b.decopressedData = new byte[length];
                    for (int i = 0; i < b.decopressedData.Length; i++)
                        b.decopressedData[i] = temp[i];
                }
            }
        }
    }
}
#region 3.0 [Data block header]
//===============================================================================

//Each compressed data block consists of a header followed by compressed data.
//The first data block starts at the address denoted in the replay file header.
//All following addresses are relative to the start of the data block header.
//The decompressed data blocks append to a single continueous data stream
//(disregarding the block headers). The content of this stream (see section 4) is
//completely independent of the original block boundaries.

//offset | size/type | Description
//-------+-----------+-----------------------------------------------------------
//0x0000 |  1  word  | size n of compressed data block (excluding header)
//0x0002 |  1  word  | size of decompressed data block (currently 8k)
//0x0004 |  1 dword  | unknown (probably checksum)
//0x0008 |  n bytes  | compressed data (decompress using zlib)

//To decompress one block with zlib:
// 1. call 'inflate_init'
// 2. call 'inflate' with Z_SYNC_FLUSH for the block

//The last block is padded with 0 bytes up to the 8K border. These bytes can
//be disregarded.

////TODO: add decompression details and move explanation to seperate file

#endregion
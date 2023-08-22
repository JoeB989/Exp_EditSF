using System;
using System.IO;
using System.Text;

namespace EsfLibrary
{
    /**
     * <summary>A class to represent the underlying data of a <see cref="Type26Node"/>.</summary>
     * <remarks>This has an odd representation.  Its formatting may not be correct.  The currently observed forms are a long form (10, 18, 35, or 51 bytes) varying based on installed dlc and a short form (9 bytes).  So far everything has been of form: typecode; a byte, if a multiple of 8, then the byte is an indicator of following data length, otherwise the byte is the first of a quad-word; finally, either nothing or a byte with a value of 0x9C.</remarks>
     * TODO When the method of determining the length of the data is clarified, make the constructors and setters enforce proper data structure.  Also look at cleaning up the representation of data.
     * XXX This will probably need revisiting to correct the binary read when more samples are available.
     * Values from startpos:
     * 0x26 00 00 00 00 00 00 00 00 (short startpos (and save in Compressed_Data)
     * 0x26 08 01 00 00 00 00 00 00 00 (long startpos)
     * Values from saved games:
     * 0x26 01 00 01 20 00 00 00 00 (short community generated)
     * 0x26 10 01 00 00 00 00 00 00 00 ff e9 ff 3f 00 00 00 00 (long community generated with The Hunter & The Beast)
     * 0x26 10 01 00 00 00 00 00 00 00 ff ff 3d 1d 00 00 00 00 (long locally generated with all owned dlc enabled (no The Hunter & The Beast) (Warhammer 2 (Tlaqua) (SAVE_GAME_HEADER) and possibly others))
     * 0x26 01 08 01 00 00 00 00 00 (short locally generated irregardless of dlc (The Fay Enchantress))
     * 0x26 10 01 00 00 00 00 00 00 00 51 a8 3d 0c 00 00 00 00 (long locally generated with some dlc disabled (The Fay Enchantress))
     * 0x26 10 01 00 00 00 00 00 00 00 05 00 00 00 00 00 00 00 (long Three Kingdoms unmodded (Gongsun Zan))
     * 0x26 05 00 00 00 00 00 00 00 (short Three Kingdoms (Compressed_Data|Campaign_Env|Campaign_Setup|Campaign_Players_Setup|Players_Array|Players_Array - #|Campaign_Player_Setup))
     * 0x26 FF FF 3D 1D 00 00 00 00 (short Warhammer 2 (Tiq Tak To) (Compressed_Data|Campaign_Env|Campaign_Setup|Campaign_Players_Setup|Players_Array|Players_Array - #|Campaign_Player_Setup))
     * 0x26 01 00 00 04 00 00 00 00 (short locally genrated Warhammer 2 (Tlaqua) (SAVE_GAME_HEADER))
     * 0x26 20 01 00 00 00 00 00 00 00 D7 FF FF 7F 0F 00 00 00 D7 FF FF 7F 1F 00 00 00 D7 FF FF FF 1F 00 00 00 9C (2x long community generated with Grom The Paunch in multiplayer)
     * 0x26 30 01 00 00 00 00 00 00 00 DF FF EF 0D 00 00 00 00 DF FF FF 1D 01 00 00 00 DF FF FF 1D 0B 00 00 00 DF FF FF 9F 0B 00 00 00 DF FF FF DF 1B 00 00 00 9C (long community generated with Grom The Paunch in multiplayer)
     * 0x26 D7 FF FF FF 1F 00 00 00 (short community generated with Grom the Paunch in multiplayer (Compressed_Data|Campaign_Env|Campaign_Setup|Campaign_Players_Setup|Players_Array|Players_Array - #|Campaign_Player_Setup))
     * 0x26 D7 FD BD 5D 0F 00 00 00 (short community generated with Grom the Paunch in multiplayer (Compressed_Data|Campaign_Env|Campaign_Setup|Campaign_Players_Setup|Players_Array|Players_Array - #|Campaign_Player_Setup))
     * 0x26 93 BF 7F 3F 0F 00 00 00 (short community generated with Grom the Paunch in multiplayer (Compressed_Data|Campaign_Env|Campaign_Model|World|Faction_Array|Faction_Array - #|Faction|CAMPAIGN_PLAYER_SETUP))
     */
    public class Type26
    {
        #region Constructors
        ///<summary>Initializes a Type26 typical of an entry with all <see cref="Data"/> equal to zero.</summary>
        public Type26()
        {
            FirstByte = 0;
            Data = new byte[7];
        }

        /**
         * <summary>Initializes a Type26 from a binary source.</summary>
         * <remarks>It is currently assumed that when reading from a binary source, if the first byte is a multiple of 8, it indicates the length of the following data unless the following data is trailed by 0x9C, in which case the 0x9C is also part of the data.  If the first byte isn't a multiple of 8, the following data is 7 bytes long.</remarks>
         * 
         * <param name="reader">A <see cref="BinaryReader"/> looking at the binary source.  It does not get closed.</param>
         */
        public Type26(BinaryReader reader)
        {
            byte[] temp;
            byte trailingByte;

            FirstByte = reader.ReadByte();
            if(FirstByte % 8 == 0 && FirstByte != 0)
                temp = reader.ReadBytes(FirstByte);
            else
                temp = reader.ReadBytes(7);

            trailingByte = reader.ReadByte();
            if(trailingByte == 0x9C)
            {
                Data = new byte[temp.Length + 1];
                Array.Copy(temp, Data, temp.Length);
                Data[temp.Length] = trailingByte;
            }
            else
            {
                Data = temp;
                --reader.BaseStream.Position;
            }
#if DEBUG
            Console.Error.Write("Type26: FirstByte: {0:X2}; Data:", FirstByte);
            foreach(byte x in Data)
                Console.Error.Write(" {0:X2}", x);
            Console.Error.Write(";\n Trailing:");
            foreach(byte x in reader.ReadBytes(60))
                Console.Error.Write(" {0:X2}", x);
            Console.Error.WriteLine();
            reader.BaseStream.Position -= 60;
#endif
        }

        /**
         * <summary>Initializes a Type26 from a string.</summary>
         * 
         * <param name="value">A string that contains a human-readable representation of a Type26.</param>
         */
        public Type26(string value)
        {
            string[] subStrings = value.Split(new char[] { ' ', ',' });
            Data = new byte[subStrings.Length - 6];

            FirstByte = Byte.Parse(subStrings[2]);
            for(uint i = 6u; i < subStrings.Length; ++i)
                Data[i - 6] = Byte.Parse(subStrings[i]);
        }

        /**
         * <summary>Initializes a deep copy of <paramref name="toCopy"/>.</summary>
         * 
         * <param name="toCopy">A Type26 to be copied.</param>
         */
        public Type26(Type26 toCopy)
        {
            FirstByte = toCopy.FirstByte;
            Data = (byte[])toCopy.Data.Clone();
        }
        #endregion

        #region Methods
        /**
         * <summary>Writes the Type26's binary representation with a <see cref="BinaryWriter"/>.</summary>
         * 
         * <param name="writer">The writer to be used.  It is not closed afterward.</param>
         */
        public void ToBinary(BinaryWriter writer)
        {
                writer.Write(FirstByte);
                writer.Write(Data);
        }

        /**
         * <summary>Outputs a human-readable representation of the Type26.</summary>
         * 
         * <returns>A string representing the data of the Type26.</returns>
         */
        public override string ToString()
        {
            int dataLength = Data.Length;
            StringBuilder builder = new StringBuilder(23 + 4 * dataLength);
            builder.AppendFormat("FirstByte = {0}, Data =", FirstByte);
            //XXX This loop can be simplified to an AppendJoin once .NET Standard 2.1 is released
            for(uint i = 0u; i < dataLength; ++i)
            {
                builder.Append(' ');
                builder.Append(Data[i]);
            }
            return builder.ToString();
        }
        #endregion

        #region Fields and Properties
        ///<summary>The data of the Type26.</summary>
        public byte[] Data { get; set; }

        ///<summary>The first byte of the Type26.</summary>
        public byte FirstByte { get; set; }
        #endregion
    }
}
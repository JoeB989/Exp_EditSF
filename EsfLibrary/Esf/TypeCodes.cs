using System;

namespace EsfLibrary
{
    public enum EsfType
    {
        // marker type
        INVALID = 0x00,

        // Primitives
        BOOL = 0x01,
        INT8 = 0x02,
        INT16 = 0x03,
        INT32 = 0x04,
        INT64 = 0x05,
        UINT8 = 0x06,
        UINT16 = 0x07,
        UINT32 = 0x08,
        UINT64 = 0x09,
        SINGLE = 0x0a,
        DOUBLE = 0x0b,
        COORD2D = 0x0c,
        COORD3D = 0x0d,
        UTF16 = 0x0e,
        ASCII = 0x0f,
        ANGLE = 0x10,

        // RoninX: TW Warhammer, ASCII?
        ASCII_W21 = 0x21,
        ASCII_W25 = 0x25,

        UNKNOWN_23 = 0x23,
        UNKNOWN_24 = 0x24,

        // Three Kingdoms DLC Eight Princes types
        UNKNOWN_26 = 0x26,

        // Arrays
        BOOL_ARRAY = 0x41,
        INT8_ARRAY = 0x42,
        INT16_ARRAY = 0x43,
        INT32_ARRAY = 0x44,
        INT64_ARRAY = 0x45,
        UINT8_ARRAY = 0x46,
        UINT16_ARRAY = 0x47,
        UINT32_ARRAY = 0x48,
        UINT64_ARRAY = 0x49,
        SINGLE_ARRAY = 0x4a,
        DOUBLE_ARRAY = 0x4b,
        COORD2D_ARRAY = 0x4c,
        COORD3D_ARRAY = 0x4d,
        UTF16_ARRAY = 0x4e,
        ASCII_ARRAY = 0x4f,
        ANGLE_ARRAY = 0x50,

        // Records and Blocks
        RECORD = 0x80,
        RECORD_BLOCK = 0x81,
        RECORD_BLOCK_ENTRY = -1,

        // Optimized Primitives
        BOOL_TRUE = 0x12,
        BOOL_FALSE = 0x13,
        UINT32_ZERO = 0x14,
        UINT32_ONE = 0x15,
        UINT32_BYTE = 0x16,
        UINT32_SHORT = 0x17,
        UINT32_24BIT = 0x18,
        INT32_ZERO = 0x19,
        INT32_BYTE = 0x1a,
        INT32_SHORT = 0x1b,
        INT32_24BIT = 0x1c,
        SINGLE_ZERO = 0x1d,

        // Optimized Arrays
        BOOL_TRUE_ARRAY = 0x52, // makes no sense
        BOOL_FALSE_ARRAY = 0x53, // makes no sense
        UINT_ZERO_ARRAY = 0x54, // makes no sense
        UINT_ONE_ARRAY = 0x55, // makes no sense
        UINT32_BYTE_ARRAY = 0x56,
        UINT32_SHORT_ARRAY = 0x57,
        UINT32_24BIT_ARRAY = 0x58,
        INT32_ZERO_ARRAY = 0x59, // makes no sense
        INT32_BYTE_ARRAY = 0x5a,
        INT32_SHORT_ARRAY = 0x5b,
        INT32_24BIT_ARRAY = 0x5c,
        SINGLE_ZERO_ARRAY = 0x5d,  // makes no sense

        LONG_RECORD = 0xa0,
        LONG_RECORD_BLOCK = 0xe0
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/* BIT MANIPULATION NOTES!!
Here are some notes for writing bit-manip stuff

In general you first create a mask, then then apply the desired operator between value and mask.

Setting bits can be done by using an OR operator
Unsetting bits can be done by using a combination of an AND and a NOT (By this i mean AND value with a mask where all bits are set except the one you want to unset)
Flipping/Inverting bits can be done by using an XOR operator

Bit selection can be done using an AND operator. Create mask with desired bits all set, then AND it with value

Counting number of bits set is identically equivalent to calculaing the Hamming weight of the binary string.





I dont know if there is a way of setting up the argument list to accept byte->ulong and sbyte->long, ive just put ulong as there is no issue ever occuring with its bit value.
*/

namespace BinaryMethods
{
    //So basically im fine with this for unsigned integers. This all works well and is good
    //Issue is i have no clue what to do for signed integers...
    //If youre using signed ints, there are many issues that can occur
    //For example if you take an Int32, XOR with 0x80_00_00_00 (ie top bit set), it expects long output as 0x80... is a long, NOT an int.
    //So instead you have to not it (~)
    //Might be worth a rethink, or equivalently just always use unsigned

    public static class BitMethods
    {
        #region Count Bits

        /// <summary>
        /// Count the number of bits set to 1 in an unsigned integer.
        /// </summary>
        /// <param name="value">Value to be checked.</param>
        /// <param name="maxBit">Highest bit to check. 0 indexed.</param>
        /// <returns>Number of bits set.</returns>
        public static byte CountBitsSet(this ulong value, int maxBit = 63)
        {
            //An explanation of how this works can be found in https://stackoverflow.com/questions/109023/how-to-count-the-number-of-set-bits-in-a-32-bit-integer

            if (maxBit < 1 || maxBit > 63)
            {
                throw new ArgumentException("Invalid number of bits chosen. Value must be between 1 and 63.", "maxBit");
            }
            else if (maxBit != 63)
            {
                //If maxBit is not 63, then we only want to count the bits up to that bit. To do so, we basically select bits 0 to maxBit
                //Could use method below but more efficient to just do the mask here ourselved (to avoid the if statements)
                value &= AllBitsSet(maxBit);
            }


            ulong result = value - ((value >> 1) & 0x5555555555555555UL);
            result = (result & 0x3333333333333333UL) + ((result >> 2) & 0x3333333333333333UL);

            //unchecked means dont flag overflow!
            return (byte)(unchecked(((result + (result >> 4)) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL) >> 56);
        }

        /// <summary>
        /// Count the number of bits set to 1 in a signed integer.
        /// </summary>
        /// <param name="value">Value to be checked.</param>
        /// <param name="maxBit">Highest bit to check. 0 indexed.</param>
        /// <returns>Number of bits set.</returns>
        public static byte CountBitsSet(this long value, int maxBit = 63)
        {
            if (maxBit < 1 || maxBit > 63)
            {
                throw new ArgumentException("Invalid number of bits chosen. Value must be between 1 and 63.", "maxBit");
            }
            else if (maxBit != 63)
            {
                value &= AllBitsSetSigned(maxBit);
            }


            long result = value - ((value >> 1) & 0x5555555555555555L);
            result = (result & 0x3333333333333333L) + ((result >> 2) & 0x3333333333333333L);

            //unchecked means dont flag overflow!
            return (byte)(unchecked(((result + (result >> 4)) & 0xF0F0F0F0F0F0F0FL) * 0x101010101010101L) >> 56);
        }

        #endregion


        #region Reverse Bits

        /// <summary>
        /// Reverses all bits in an unsigned integer.
        /// </summary>
        /// <param name="value">Value to be reversed.</param>
        /// <returns>Unsigned integer representing value with all bits reversed (note that reversed and flipped are NOT the same.)</returns>
        public static ulong ReverseBits(this ulong value, int maxBit = 63)
        {
            if (maxBit < 1 || maxBit > 63)
            {
                throw new ArgumentException("Invalid number of bits chosen. Value must be between 1 and 63.", "maxBit");
            }

            ulong revNum = 0, temp;

            for (int i = 0; i < maxBit; i++)
            {
                //Check ith bit. If its set, set the maxBit-i-1 th bit
                temp = value & (1UL << i);
                if (temp != 0)
                {
                    revNum |= 1UL << (maxBit - i - 1);
                }
            }

            return revNum;
        }

        /// <summary>
        /// Reverses all bits in a signed integer.
        /// </summary>
        /// <param name="value">Value to be reversed.</param>
        /// <returns>Unsigned integer representing value with all bits reversed (note that reversed and flipped are NOT the same.)</returns>
        public static long ReverseBits(this long value, int maxBit = 63)
        {
            if (maxBit < 1 || maxBit > 63)
            {
                throw new ArgumentException("Invalid number of bits chosen. Value must be between 1 and 63.", "maxBit");
            }

            long revNum = 0, temp;

            for (int i = 0; i < maxBit; i++)
            {
                //Check ith bit. If its set, set the maxBit-i-1 th bit
                temp = value & (1L << i);
                if (temp != 0)
                {
                    revNum |= 1L << (maxBit - i - 1);
                }
            }

            return revNum;
        }

        #endregion


        #region Select Bits

        /// <summary>
        /// Select a number of bits from an unsigned integer.
        /// </summary>
        /// <param name="value">Value to select bits from.</param>
        /// <param name="startBit">0 based index of the start bit.</param>
        /// <param name="endBit">0 based index of the end bit. This is inclusive, ie this bit will be included.</param>
        /// <returns>Unsigned integer containing selected bits, shifted such that startBit is bit 0.</returns>
        public static ulong SelectBits(this ulong value, int startBit = 0, int endBit = 63)
        {
            if (endBit < 0 || endBit > 63)
            {
                throw new ArgumentException("Invalid number of bits chosen. Value must be between 0 and 63.", "endBit");
            }
            else if (startBit < 0 || startBit > 63)
            {
                throw new ArgumentException("Invalid number of bits chosen. Value must be between 0 and 63.", "startBit");
            }
            else if (endBit <= startBit)
            {
                throw new ArgumentException("End bit was less than or equal to start bit.", "endBit");
            }

            //Left shift by startBit, & with 0b111... where there are endBit-startBit 1s
            //1 << n gives 0b100...00 wither bit n+1 is set. So subtract 1 to get n bits set.
            ulong mask = AllBitsSet(endBit - startBit); 
            return (value >> startBit) & mask;
        }

        /// <summary>
        /// Select a number of bits from a signed integer.
        /// </summary>
        /// <param name="value">Value to select bits from.</param>
        /// <param name="startBit">0 based index of the start bit.</param>
        /// <param name="endBit">0 based index of the end bit. This is inclusive, ie this bit will be included.</param>
        /// <returns>Signed integer containing selected bits, shifted such that startBit is bit 0.</returns>
        public static long SelectBits(this long value, int startBit = 0, int endBit = 63)
        {
            if (endBit < 0 || endBit > 63)
            {
                throw new ArgumentException("Invalid number of bits chosen. Value must be between 0 and 63.", "endBit");
            }
            else if (startBit < 0 || startBit > 63)
            {
                throw new ArgumentException("Invalid number of bits chosen. Value must be between 0 and 63.", "startBit");
            }
            else if (endBit <= startBit)
            {
                throw new ArgumentException("End bit was less than or equal to start bit.", "endBit");
            }

            //Left shift by startBit, & with 0b111... where there are endBit-startBit 1s
            long mask = AllBitsSetSigned(endBit - startBit); 
            return (value >> startBit) & mask;
        }

        #endregion


        #region All Bits Set

        /// <summary>
        /// Gets an unsigned integer value with all bits up to maxBit set (0 indexed).
        /// </summary>
        /// <param name="maxBit">Index of maximum bit to be set (0 indexed).</param>
        /// <returns>Unsigned long integer with all bits upto maxBit set.</returns>
        public static ulong AllBitsSet(int maxBit)
        {
            if (maxBit < 0 || maxBit > 63)
            {
                throw new ArgumentException("Invalid number of bits. Parameter numBits must be between 0 and 63.", "maxBit");
            }
            else if(maxBit == 63)
            {
                return ulong.MaxValue;
            }
            else
            {
                return (1UL << maxBit + 1) - 1;
            }
        }

        /// <summary>
        /// Gets a signed integer value with all bits up to maxBit set (0 indexed).
        /// </summary>
        /// <param name="maxBit">Index of maximum bit to be set (0 indexed).</param>
        /// <returns>Signed long integer with all bits upto maxBit set.</returns>
        public static long AllBitsSetSigned(int maxBit)
        {
            if (maxBit < 0 || maxBit > 63)
            {
                throw new ArgumentException("Invalid number of bits. Parameter numBits must be between 0 and 63.", "maxBit");
            }
            else if (maxBit == 63)
            {
                return -1;
            }
            else
            {
                return (1L << maxBit + 1) - 1;
            }
        }


        #endregion


        #region Set Bits

        /// <summary>
        /// Sets a selection of bits within an unsigned integer.
        /// </summary>
        /// <param name="value">Value to have bits set.</param>
        /// <param name="bitsToSet">Selection of bits to set. Bit indicies are 0 indexed.</param>
        /// <returns>Value with the chosen bits set (or equivalently set to be 1).</returns>
        public static ulong SetBits(this ulong value, byte[] bitsToSet)
        {
            //TODO: check that all bitsToSet are unique and that they are between 0 and 63

            foreach (byte bit in bitsToSet)
            {
                value |= (1UL << bit); //this sets the nth bit to be 1 
            }

            return value;
        }

        /// <summary>
        /// Sets a selection of bits within a signed integer.
        /// </summary>
        /// <param name="value">Value to have bits set.</param>
        /// <param name="bitsToSet">Selection of bits to set. Bit indicies are 0 indexed.</param>
        /// <returns>Value with the chosen bits set (or equivalently set to be 1).</returns>
        public static long SetBits(this long value, byte[] bitsToSet)
        {
            foreach (byte bit in bitsToSet)
            {
                value |= (1L << bit); //this sets the nth bit to be 1 
            }

            return value;
        }

        /// <summary>
        /// Sets bits between start and end values within a given unsigned integer.
        /// </summary>
        /// <param name="value">Value to have bits set.</param>
        /// <param name="startBit">0 based index of the start bit.</param>
        /// <param name="endBit">0 based index of the end bit. This is inclusive, ie this bit will be included.</param>
        /// <returns>Unsigned integer with the chosen bits set (or equivalently set to be 1).</returns>
        public static ulong SetBits(this ulong value, int startBit = 0, int endBit = 63)
        {
            if (endBit < 0 || endBit > 63)
            {
                throw new ArgumentException("Invalid number of bits chosen. Value must be between 0 and 63.", "endBit");
            }
            else if (startBit < 0 || startBit > 63)
            {
                throw new ArgumentException("Invalid number of bits chosen. Value must be between 0 and 63.", "startBit");
            }
            else if (endBit <= startBit)
            {
                throw new ArgumentException("End bit was less than or equal to start bit.", "endBit");
            }

            ulong mask = AllBitsSet(endBit - startBit) << startBit;
            return value | mask;
        }

        /// <summary>
        /// Sets bits between start and end values within a given unsigned integer.
        /// </summary>
        /// <param name="value">Value to have bits set.</param>
        /// <param name="startBit">0 based index of the start bit.</param>
        /// <param name="endBit">0 based index of the end bit. This is inclusive, ie this bit will be included.</param>
        /// <returns>Unsigned integer with the chosen bits set (or equivalently set to be 1).</returns>
        public static long SetBits(this long value, int startBit = 0, int endBit = 63)
        {
            if (endBit < 0 || endBit > 63)
            {
                throw new ArgumentException("Invalid number of bits chosen. Value must be between 0 and 63.", "endBit");
            }
            else if (startBit < 0 || startBit > 63)
            {
                throw new ArgumentException("Invalid number of bits chosen. Value must be between 0 and 63.", "startBit");
            }
            else if (endBit <= startBit)
            {
                throw new ArgumentException("End bit was less than or equal to start bit.", "endBit");
            }

            long mask = AllBitsSetSigned(endBit - startBit) << startBit;
            return value | mask;
        }

        #endregion


        #region Unset Bits

        /// <summary>
        /// Unsets a selection of bits within an unsigned integer value.
        /// </summary>
        /// <param name="value">Unsigned integer to have bits set.</param>
        /// <param name="bitsToSet">Selection of bits to unset. Bit indicies are 0 indexed.</param>
        /// <returns>Unsigned integer with the chosen bits unset (or equivalently set to be 0).</returns>
        public static ulong UnsetBits(this ulong value, byte[] bitsToSet)
        {
            //TODO: check that all bitsToSet are unique and that they are between 0 and 63

            foreach (byte bit in bitsToSet)
            {
                value &= ~(1UL << bit); //this unsets the nth bit
            }

            return value;
        }

        /// <summary>
        /// Unsets a selection of bits within a signed integer.
        /// </summary>
        /// <param name="value">Signed integer to have bits set.</param>
        /// <param name="bitsToSet">Selection of bits to unset. Bit indicies are 0 indexed.</param>
        /// <returns>Signed integer with the chosen bits unset (or equivalently set to be 0).</returns>
        public static long UnsetBits(this long value, byte[] bitsToSet)
        {
            //TODO: check that all bitsToSet are unique and that they are between 0 and 63

            foreach (byte bit in bitsToSet)
            {
                value &= ~(1L << bit); //this unsets the nth bit
            }

            return value;
        }

        /// <summary>
        /// Unsets all bits between start and end bits of an unsigned integer (inclusive, 0 indexed).
        /// </summary>
        /// <param name="value">value to have chosen bits unset.</param>
        /// <param name="startBit">0 based index of the start bit.</param>
        /// <param name="endBit">0 based index of the end bit. This is inclusive, ie this bit will be included.</param>
        /// <returns>Unsigned integer with bits between start and end bit unset.</returns>
        public static ulong UnsetBits(this ulong value, int startBit = 0, int endBit = 63)
        {
            if (endBit < 0 || endBit > 63)
            {
                throw new ArgumentException("Invalid number of bits chosen. Value must be between 0 and 63.", "endBit");
            }
            else if (startBit < 0 || startBit > 63)
            {
                throw new ArgumentException("Invalid number of bits chosen. Value must be between 0 and 63.", "startBit");
            }
            else if (endBit <= startBit)
            {
                throw new ArgumentException("End bit was less than or equal to start bit.", "endBit");
            }

            ulong mask = AllBitsSet(endBit - startBit) << startBit;
            return value & ~mask;
        }

        /// <summary>
        /// Unsets all bits between start and end bits of a signed integer (inclusive, 0 indexed).
        /// </summary>
        /// <param name="value">value to have chosen bits unset.</param>
        /// <param name="startBit">0 based index of the start bit.</param>
        /// <param name="endBit">0 based index of the end bit. This is inclusive, ie this bit will be included.</param>
        /// <returns>Signed integer with bits between start and end bit unset.</returns>
        public static long UnsetBits(this long value, int startBit = 0, int endBit = 63)
        {
            if (endBit < 0 || endBit > 63)
            {
                throw new ArgumentException("Invalid number of bits chosen. Value must be between 0 and 63.", "endBit");
            }
            else if (startBit < 0 || startBit > 63)
            {
                throw new ArgumentException("Invalid number of bits chosen. Value must be between 0 and 63.", "startBit");
            }
            else if (endBit <= startBit)
            {
                throw new ArgumentException("End bit was less than or equal to start bit.", "endBit");
            }

            long mask = AllBitsSetSigned(endBit - startBit) << startBit;
            return value & ~mask;
        }

        #endregion


        #region Invert Bits

        /// <summary>
        /// Inverts / flips chosen bits in an unsigned integer.
        /// </summary>
        /// <param name="value">Unsigned integer to have bits inverted.</param>
        /// <param name="bitsToInvert">Bits to invert.</param>
        /// <returns>Unsigned integer with chosen bits inverted (ie, 0->1 and 1->0).</returns>
        public static ulong InvertBits(this ulong value, byte[] bitsToInvert)
        {
            //TODO: check that all bitsToSet are unique and that they are between 0 and 63
            ulong mask = SetBits(0UL, bitsToInvert);
            return value ^ mask;
        }

        /// <summary>
        /// Inverts / flips chosen bits in a signed integer.
        /// </summary>
        /// <param name="value">Signed integer to have bits inverted.</param>
        /// <param name="bitsToInvert">Bits to invert.</param>
        /// <returns>Signed integer with chosen bits inverted (ie, 0->1 and 1->0).</returns>
        public static long InvertBits(this long value, byte[] bitsToInvert)
        {
            //TODO: check that all bitsToSet are unique and that they are between 0 and 63
            long mask = SetBits(0, bitsToInvert);
            return value ^ mask;
        }

        /// <summary>
        /// Inverts / flips bits between start and end bits of an unsigned integer (inclusive, 0 indexed).
        /// </summary>
        /// <param name="value">Unsigned integer to have bits invert.</param>
        /// <param name="startBit">0 based index of the start bit.</param>
        /// <param name="endBit">0 based index of the end bit. This is inclusive, ie this bit will be included.</param>
        /// <returns>Unsigned integer with chosen bits inverted (ie, 0->1 and 1->0).</returns>
        public static ulong InvertBits(this ulong value, int startBit = 0, int endBit = 63)
        {
            if (endBit < 0 || endBit > 63)
            {
                throw new ArgumentException("Invalid number of bits chosen. Value must be between 0 and 63.", "endBit");
            }
            else if (startBit < 0 || startBit > 63)
            {
                throw new ArgumentException("Invalid number of bits chosen. Value must be between 0 and 63.", "startBit");
            }
            else if (endBit <= startBit)
            {
                throw new ArgumentException("End bit was less than or equal to start bit.", "endBit");
            }

            //Invert bits between start and end bits
            //Create mask with bits set between start and end bits, then XOR it with value

            ulong mask = AllBitsSet(endBit - startBit) << startBit;
            return value ^ mask;
        }

        /// <summary>
        /// Inverts / flips bits between start and end bits of a signed integer (inclusive, 0 indexed).
        /// </summary>
        /// <param name="value">Signed integer to have bits invert.</param>
        /// <param name="startBit">0 based index of the start bit.</param>
        /// <param name="endBit">0 based index of the end bit. This is inclusive, ie this bit will be included.</param>
        /// <returns>Signed integer with chosen bits inverted (ie, 0->1 and 1->0).</returns>
        public static long InvertBits(this long value, int startBit = 0, int endBit = 63)
        {
            if (endBit < 0 || endBit > 63)
            {
                throw new ArgumentException("Invalid number of bits chosen. Value must be between 0 and 63.", "endBit");
            }
            else if (startBit < 0 || startBit > 63)
            {
                throw new ArgumentException("Invalid number of bits chosen. Value must be between 0 and 63.", "startBit");
            }
            else if (endBit <= startBit)
            {
                throw new ArgumentException("End bit was less than or equal to start bit.", "endBit");
            }

            //Invert bits between start and end bits
            //Create mask with bits set between start and end bits, then XOR it with value

            long mask = AllBitsSetSigned(endBit - startBit) << startBit;
            return value ^ mask;
        }

        #endregion


        #region Hamming Distance

        /// <summary>
        /// Calculates the Hamming Distance between two unsigned integers. This is defined as the number of differing bits.
        /// </summary>
        /// <param name="value1">First value.</param>
        /// <param name="value2">Second value.</param>
        /// <returns>Returns the Hamming Distance, which is the number of bits set in value1 XOR value2.</returns>
        public static byte HammingDistance(ulong value1, ulong value2)
        {
            return CountBitsSet(value1 ^ value2);
        }

        /// <summary>
        /// Calculates the Hamming Distance between two signed integers. This is defined as the number of differing bits.
        /// </summary>
        /// <param name="value1">First value.</param>
        /// <param name="value2">Second value.</param>
        /// <returns>Returns the Hamming Distance, which is the number of bits set in value1 XOR value2.</returns>
        public static byte HammingDistance(long value1, long value2)
        {
            return CountBitsSet(value1 ^ value2);
        }

        #endregion


        #region Bit Length

        /// <summary>
        /// Determines the number of bits required to represent the given value in binary, excluding leading zeroes.
        /// </summary>
        /// <param name="value">Chosen integer value.</param>
        /// <returns>Returns number of bits required to represent value.</returns>
        public static byte BitLength(this ulong value)
        {
            //determine number of bits required to represent value
            //i think a good way to do this is detemrmine how many left shifts are needed 

            byte output = 0;
            while (value >> output != 0)
            {
                output += 1;
            }
            return output;
        }

        /// <summary>
        /// Determines the number of bits required to represent the given value in binary, excluding leading zeroes.
        /// </summary>
        /// <param name="value">Chosen integer value.</param>
        /// <returns>Returns number of bits required to represent value.</returns>
        public static byte BitLength(this long value)
        {
            byte output = 0;
            while (value >> output != 0)
            {
                output += 1;
            }
            return output;
        }

        #endregion
    }
}

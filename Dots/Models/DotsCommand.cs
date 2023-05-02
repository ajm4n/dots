using System;
using System.Text;


namespace Dots.Models
{
    public abstract class DotsCommand
    {
        public abstract string Name { get; }
        public abstract DotsProperties DotsProperty { get; set; }
        public abstract string Execute(string[] args);
        public static byte[] Zor(byte[] input, string key)
        {
            int _key = Int32.Parse(key);
            byte[] mixed = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                mixed[i] = (byte)(input[i] ^ _key);
            }
            return mixed;
        }
    };
}

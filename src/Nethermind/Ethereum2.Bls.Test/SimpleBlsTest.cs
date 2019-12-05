using Nethermind.Bls;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ethereum2.Bls.Test
{
    public class SimpleBlsTest
    {
        [Test]
        public void TestPrivateKeyToPublic()
        {
            var privateKeyBytes = new byte[] {
                0x47, 0xb8, 0x19, 0x2d, 0x77, 0xbf, 0x87, 0x1b,
                0x62, 0xe8, 0x78, 0x59, 0xd6, 0x53, 0x92, 0x27,
                0x25, 0x72, 0x4a, 0x5c, 0x03, 0x1a, 0xfe, 0xab,
                0xc6, 0x0b, 0xce, 0xf5, 0xff, 0x66, 0x51, 0x38 };

            Console.WriteLine("Serialized private key: {0}", BitConverter.ToString(privateKeyBytes));

            BlsProxy.GetPublicKey(privateKeyBytes, out var publicKeySpan);
            var publicKeyBytes = publicKeySpan.ToArray();

            Console.WriteLine("Expecting public key b301803f...");
            Console.WriteLine("Serialized public key: {0}", BitConverter.ToString(publicKeyBytes));

            Assert.AreEqual((byte)0xb3, publicKeyBytes[0]);
            Assert.AreEqual((byte)0x01, publicKeyBytes[1]);
            Assert.AreEqual((byte)0x80, publicKeyBytes[2]);
            Assert.AreEqual((byte)0x3f, publicKeyBytes[3]);
        }
    }
}

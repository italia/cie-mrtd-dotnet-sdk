using CIE.MRTD.SDK.Crypto;
using CIE.MRTD.SDK.Util;
using System;
using System.Numerics;

namespace CIE.MRTD.SDK.EAC

{
    internal class DHKey
    { 
        public ByteArray Public;
        public ByteArray Private;
    }
    internal interface IPACEMapping
    {
        IPACEAlgo Map(byte[] secret, byte[] nonce);
    }
    internal interface IPACEAlgo : ICloneable
    {
        DHKey GenerateKeyPair();
        byte[] GetSharedSecret(byte[] otherPubKey);
        byte[] Encrypt(byte[] data);
    }
    internal class PACEAlgo
    {
        public ASN1Tag DG14Tag;
        public IPACEMapping mapping;
        public IPACEAlgo algo1;
        public IPACEAlgo algo2;
        public byte[] GetSharedSecret1(byte[] otherPubKey) 
        {
            return algo1.GetSharedSecret(otherPubKey);
        }

        public byte[] GetSharedSecret2(byte[] otherPubKey)
        {
            return algo2.GetSharedSecret(otherPubKey);
        }
        
        public void DoMapping(byte[] secret, byte[] nonce) {
            algo2 = mapping.Map(secret, nonce);
        }

        public PACEAlgo(ASN1Tag tag)
        {
            // dovrei inizializzare i vari componenti in base all'OID del tag.
            // per adesso mi limito a DH_GM
            DG14Tag = tag;
            algo1 = new DHAlgo(DG14Tag);
            mapping = new GenericMapping(algo1);
        }

        public DHKey GenerateEphimeralKey1()
        {
            return algo1.GenerateKeyPair();
        }

        public DHKey GenerateEphimeralKey2()
        {
            return algo2.GenerateKeyPair();
        }
    }

    internal class GenericMapping : IPACEMapping
    {
        public IPACEAlgo algo;
        public GenericMapping(IPACEAlgo algo)
        {
            this.algo = algo;
        }
        public IPACEAlgo Map(byte[] secret, byte[] nonce) {
            IPACEAlgo newAlgo=algo.Clone() as IPACEAlgo;
            if (newAlgo is DHAlgo) {
                var dhNewAlgo = newAlgo as DHAlgo;

                var temp = BigInteger.ModPow(dhNewAlgo.Group, new ByteArray(nonce), dhNewAlgo.Prime);
                var temp2 = BigInteger.Multiply(temp, new ByteArray(secret));
                dhNewAlgo.Group = BigInteger.Remainder(temp2, dhNewAlgo.Prime);

            }
            return newAlgo;
        }
    }
    internal class DHAlgo : IPACEAlgo
    {
        public object Clone() {
            return new DHAlgo(DG14Tag)
            {
                Group = Group,
                Order = Order,
                Prime = Prime,
                Key = Key
            };
        }
        public byte[] Encrypt(byte[] data) {
            return null;
        }

        public byte[] GetSharedSecret(byte[] otherPubKey) {
            return DiffieHellmann.ComputeKey(Group, Prime, Key.Private, otherPubKey);
        }
        public static ByteArray StandardDHParam2Prime = new ByteArray("87A8E61D B4B6663C FFBBD19C 65195999 8CEEF608 660DD0F2 5D2CEED4 435E3B00 E00DF8F1 D61957D4 FAF7DF45 61B2AA30 16C3D911 34096FAA 3BF4296D 830E9A7C 209E0C64 97517ABD 5A8A9D30 6BCF67ED 91F9E672 5B4758C0 22E0B1EF 4275BF7B 6C5BFC11 D45F9088 B941F54E B1E59BB8 BC39A0BF 12307F5C 4FDB70C5 81B23F76 B63ACAE1 CAA6B790 2D525267 35488A0E F13C6D9A 51BFA4AB 3AD83477 96524D8E F6A167B5 A41825D9 67E144E5 14056425 1CCACB83 E6B486F6 B3CA3F79 71506026 C0B857F6 89962856 DED4010A BD0BE621 C3A3960A 54E710C3 75F26375 D7014103 A4B54330 C198AF12 6116D227 6E11715F 693877FA D7EF09CA DB094AE9 1E1A1597");
        public static ByteArray StandardDHParam2Group = new ByteArray("3FB32C9B 73134D0B 2E775066 60EDBD48 4CA7B18F 21EF2054 07F4793A 1A0BA125 10DBC150 77BE463F FF4FED4A AC0BB555 BE3A6C1B 0C6B47B1 BC3773BF 7E8C6F62 901228F8 C28CBB18 A55AE313 41000A65 0196F931 C77A57F2 DDF463E5 E9EC144B 777DE62A AAB8A862 8AC376D2 82D6ED38 64E67982 428EBC83 1D14348F 6F2F9193 B5045AF2 767164E1 DFC967C1 FB3F2E55 A4BD1BFF E83B9C80 D052B985 D182EA0A DB2A3B73 13D3FE14 C8484B1E 052588B9 B7D2BBD2 DF016199 ECD06E15 57CD0915 B3353BBB 64E0EC37 7FD02837 0DF92B52 C7891428 CDC67EB6 184B523D 1DB246C3 2F630784 90F00EF8 D647D148 D4795451 5E2327CF EF98C582 664B4C0F 6CC41659");
        public static ByteArray StandardDHParam2Order = new ByteArray("8CF83642 A709A097 B4479976 40129DA2 99B1A47D 1EB3750B A308B0FE 64F5FBD3");

        public ByteArray Prime;
        public ByteArray Group;
        public ByteArray Order;
        public DHKey Key;

        public DHKey GenerateKeyPair() {
            Key = new DHKey();
            DiffieHellmann.GenerateKey(Group, Prime, ref Key.Private, ref Key.Public);
            return Key;
        }

        public ASN1Tag DG14Tag;
        public DHAlgo(ASN1Tag tag)
        {
            var paramId = new ByteArray(tag.CheckTag(0x30).Child(2, 0x02).Data).ToUInt;
            if (paramId != 2)
                throw new Exception("Parametri di default : " + paramId.ToString() + " non supportati");

            Prime = StandardDHParam2Prime;
            Group = StandardDHParam2Group;
            Order = StandardDHParam2Order;

            DG14Tag = tag;

        }
    }
}

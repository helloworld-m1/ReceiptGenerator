using System;
using System.Security.Cryptography;
using System.Text;
using Nethereum.Signer;
using Nethereum.Util;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Math;

namespace ReceiptGenerator.Crypto
{
    public static class WalletGenerator
    {
        private static readonly Sha3Keccack Sha3 = new Sha3Keccack();
        private static readonly BigInteger Secp256k1N = SecNamedCurves.GetByName("secp256k1").N;

        public struct Wallet
        {
            public string PrivateKeyHex;
            public string Address;
        }

        public static Wallet Generate(string chain)
        {
            if (string.IsNullOrWhiteSpace(chain))
                chain = "ERC20";

            if (string.Equals(chain, "TRC20", StringComparison.OrdinalIgnoreCase))
                return GenerateTron();

            return GenerateEthLike();
        }

        // 使用系统 RNG 生成私钥并校验范围
        private static EthECKey CreateEthKey()
        {
            var priv = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                do
                {
                    rng.GetBytes(priv);
                } while (!IsValidSecp256k1PrivateKey(priv));
            }
            return new EthECKey(priv, true);
        }

        private static bool IsValidSecp256k1PrivateKey(byte[] priv)
        {
            if (priv == null || priv.Length != 32) return false;
            var d = new BigInteger(1, priv); // 正数
            return d.CompareTo(BigInteger.One) >= 0 && d.CompareTo(Secp256k1N) < 0;
        }

        public static Wallet GenerateEthLike()
        {
            var key = CreateEthKey();
            var privateKeyHex = key.GetPrivateKey();
            var address = key.GetPublicAddress();

            return new Wallet
            {
                PrivateKeyHex = privateKeyHex,
                Address = address
            };
        }

        public static Wallet GenerateTron()
        {
            var key = CreateEthKey();
            var privateKeyHex = key.GetPrivateKey();

            var pubNoPrefix = key.GetPubKeyNoPrefix();

            var hash = Sha3.CalculateHash(pubNoPrefix);
            var address20 = new byte[20];
            Buffer.BlockCopy(hash, 12, address20, 0, 20);

            var tronPayload = new byte[21];
            tronPayload[0] = 0x41;
            Buffer.BlockCopy(address20, 0, tronPayload, 1, 20);

            var tronBase58 = Base58CheckEncode(tronPayload);

            return new Wallet
            {
                PrivateKeyHex = privateKeyHex,
                Address = tronBase58
            };
        }

        private static string Base58CheckEncode(byte[] payload)
        {
            var checksum = DoubleSHA256(payload);
            var withChecksum = new byte[payload.Length + 4];
            Buffer.BlockCopy(payload, 0, withChecksum, 0, payload.Length);
            Buffer.BlockCopy(checksum, 0, withChecksum, payload.Length, 4);
            return Base58.Encode(withChecksum);
        }

        private static byte[] DoubleSHA256(byte[] data)
        {
            using (var sha = SHA256.Create())
            {
                var first = sha.ComputeHash(data);
                return sha.ComputeHash(first);
            }
        }

        private static class Base58
        {
            private const string Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

            public static string Encode(byte[] data)
            {
                if (data == null || data.Length == 0) return string.Empty;

                int zeros = 0;
                while (zeros < data.Length && data[zeros] == 0) zeros++;

                var input = new byte[data.Length];
                Buffer.BlockCopy(data, 0, input, 0, data.Length);

                var temp = new char[input.Length * 2];
                int j = temp.Length;

                int startAt = zeros;
                while (startAt < input.Length)
                {
                    int mod = DivMod58(input, startAt);
                    if (input[startAt] == 0) startAt++;
                    temp[--j] = Alphabet[mod];
                }

                while (zeros-- > 0)
                {
                    temp[--j] = '1';
                }

                return new string(temp, j, temp.Length - j);
            }

            private static int DivMod58(byte[] input, int startAt)
            {
                int remainder = 0;
                for (int i = startAt; i < input.Length; i++)
                {
                    int num = ((int)input[i] & 0xFF);
                    int temp = remainder * 256 + num;
                    input[i] = (byte)(temp / 58);
                    remainder = temp % 58;
                }
                return remainder;
            }
        }
    }
}
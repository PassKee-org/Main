global using SHA256 = DgNet.Keepass.BrowserSha256;
global using SHA512 = DgNet.Keepass.BrowserSha512;
global using HMACSHA256 = DgNet.Keepass.BrowserHmac256;
global using RandomNumberGenerator = DgNet.Keepass.BrowserRandom;

using System;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace DgNet.Keepass;

internal static class BrowserSha256 {
	public static byte[] HashData(byte[] data) => Digest(new Sha256Digest(), data);
	internal static byte[] Digest(IDigest digest, byte[] data) {
		digest.BlockUpdate(data, 0, data.Length);
		var output = new byte[digest.GetDigestSize()];
		digest.DoFinal(output, 0);
		return output;
	}
}

internal static class BrowserSha512 {
	public static byte[] HashData(byte[] data) => BrowserSha256.Digest(new Sha512Digest(), data);
}

internal sealed class BrowserHmac256 : IDisposable {
	private readonly byte[] _key;
	public BrowserHmac256(byte[] key) => _key = (byte[])key.Clone();
	public byte[] ComputeHash(byte[] data) {
		var mac = new HMac(new Sha256Digest());
		mac.Init(new KeyParameter(_key));
		mac.BlockUpdate(data, 0, data.Length);
		var output = new byte[mac.GetMacSize()];
		mac.DoFinal(output, 0);
		return output;
	}
	public void Dispose() => Array.Clear(_key);
}

internal static class BrowserRandom {
	private static readonly SecureRandom Random = new();
	public static byte[] GetBytes(int count) {
		var output = new byte[count];
		Fill(output);
		return output;
	}
	public static void Fill(byte[] data) => Random.NextBytes(data);
}

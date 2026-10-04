using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;

namespace DgNet.Keepass;

public class AesKdf : IKdf {

	public static readonly Guid Uuid = new("c9d9f39a-628a-4460-bf74-0d08c18a4fea");

	public byte[] Seed   { get; }
	public ulong  Rounds { get; }

	public AesKdf(byte[] seed, ulong rounds) {
		Seed   = seed;
		Rounds = rounds;
	}

	// DerivedKey = SHA256(ECB(left, seed, rounds) ∥ ECB(right, seed, rounds))
	// where left = rawKey[0..16], right = rawKey[16..32]
	public byte[] Transform(byte[] rawKey) {
		if (Rounds > 100_000_000) throw new NotSupportedException("AES-KDF exceeds the browser work limit.");
		var encryptor = new AesEngine();
		encryptor.Init(true, new KeyParameter(Seed));

		var left    = rawKey[..16];
		var right   = rawKey[16..];
		var leftBuf = new byte[16];
		var rightBuf = new byte[16];

		for (ulong i = 0; i < Rounds; i++) {
			encryptor.ProcessBlock(left, 0, leftBuf, 0);
			encryptor.ProcessBlock(right, 0, rightBuf, 0);
			(left, leftBuf)   = (leftBuf,  left);
			(right, rightBuf) = (rightBuf, right);
		}

		var combined = new byte[32];
		left.CopyTo(combined, 0);
		right.CopyTo(combined, 16);
		return SHA256.HashData(combined);
	}

	public VariantMap Parameters() => new(new Dictionary<string, object> {
		["$UUID"] = GuidRfc4122.ToBytes(Uuid),
		["S"]     = Seed,
		["R"]     = Rounds,
	});
}

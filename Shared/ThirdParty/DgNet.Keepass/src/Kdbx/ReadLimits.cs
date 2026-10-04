using System;
using System.IO;

namespace DgNet.Keepass;

public static class ReadLimits {
	public const int MaxFileBytes = 100 * 1024 * 1024;
	public const int MaxExpandedBytes = 256 * 1024 * 1024;
	public const int MaxDepth = 128;

	internal static byte[] ReadBytes(BinaryReader reader, int length, int maximum = MaxExpandedBytes) {
		if (length < 0 || length > maximum) throw new InvalidDataException("KDBX field exceeds the browser size limit.");
		var data = reader.ReadBytes(length);
		if (data.Length != length) throw new EndOfStreamException("Truncated KDBX field.");
		return data;
	}

	internal static void Copy(Stream source, Stream destination) {
		var buffer = new byte[81920];
		int count;
		while ((count = source.Read(buffer, 0, buffer.Length)) != 0) {
			if (destination.Length + count > MaxExpandedBytes)
				throw new InvalidDataException("Expanded KDBX exceeds the browser size limit.");
			destination.Write(buffer, 0, count);
		}
	}
}

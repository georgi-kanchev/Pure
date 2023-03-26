namespace Pure.Window;

using System.IO.Compression;

using SFML.Graphics;

internal static class DefaultGraphics
{
	// image format
	// each byte (8 bits) goes as
	// packed bit (1) | color bit | up to 64 repeats (6 bits)
	// not packed bit (0) | 7 literal sequence of raw bits

	private const string DEFAULT_GRAPHICS_BASE64 = "3VXrUxvXFdc+0K7ISlrJwqyMzK5etowNXlkgBMjS1QM9QFgyQRiEZVZeBywjW4sJsBJCfJXAuDaevNxOJmkyk3rSNklnOtNOZ7pf8Yzz+NLJ1077N/RDP6V3wW3SJM0fUO2Mztxz9vzO77fnnns1X2m+aqXifYXp/Pm+zbGwrD85UbDTMzPd//ggkb/3N2J5LdszcuXelntNGCwWNyYejPfNzuT77uTHXphPxh4s+R4DIRAKEtmd9+P5gb9bp8tOkkS3Ync3pJ5qry/2XfDa1MkJBoDu673VYeRGbc3GvAdr/PV7OPv/qfDFD6q/p9ZApsvnXebOieg/3XfLTt2EbrDQb53k8I6OZR+dO9W/PrTQjk5n8bi5PBjsZ1MDoyvEcvjc2fRU19QCWLpw031yEoH+C2wv5j7sPeNxXzXhLjA7aY8vE5dBLoabzBOtwvR8yuHuykSnE/jEeSbt9mkx7aU79ehu2t3+n8F2IUzPhYLayZ0fxTXhhX7jK2fvv3KlUKpV75y773P7/JJ1EiJMqwotXqv95KT7VeugBuSub1zqxNw3Zhd4tHMi+M033/yFcE+XB+en4BvT1SMbA0tjl29Uh603lsbG7lWHDy+XhECw9x72nICdGLu82DuJgPLgnL6bcAMhWL/YxNyZja2AkR2OTZd9tl7j8EJpYyt4+iSGrJXV7vsW7grB7RuGSXdGkiE0sXC3CnGaWzEA19eJwx8C/wSnMGT9pS1MZJNbjZDIaio+TJ8IhY1ZUdxOiN65peHafMLnYygMo3jMRxk2e/TQPx/TQrtpjVT8PUhC4BnKC/RzfPgGXOsTAsVU/PPyksG7mNH21BrL4USrimHbTQhYWZNKic1wEivBAoJ3jtL2QOAQpfdv6RvVkEbUWmvNkNZA+a1TGYGnKt6Y/jrvna/4Met1kWLE5Nh1RvIuxpbFbcB6s8mlEUiUZYjZEZkP4UyFFxuKojwVHgoaCQEMxhB/Fj6rgRCSjWFbbwrU6ay4JjfvsVpKpSqGQXXJup0IQUTNmo1fgQz9PVMgCK3Pm9GGIcNhuZQQKWQqEutXP0kFgwrEoSQmrelVBRi2RfHaoFZ/hMerCuZnbcSVY3xKAxVY55ihucrw6jbAKKuYrM1RAruYFGuQsTdJJEZqqoJWhZACdXKMWU7KUMETiQmxsWxprSGH6JTLrMOjq0xqoFs3ftMUuO4qiRTVy1OU/4Q+ftPLZM6YSNdNEx11mNK42RvTSKX5hoDZMpe89BWYjN600x6/ucMQG7EmKCOREXpkJshOuu+XgtMX/Yuv+beGmmXjQscsw8tYGFS80aDWTI7fjDApl4UcR+10xtOtoyJJjKJEb4LkvZa425S+ZT5HxZ3mlOu+1TJhYxeXBa/cYFgj+brZSkWcptu3Rq2GBHc1dSsANSl/Ahxv6ySwiCAFoCBB+SjH43gHhgCatujPiKTyqxxNooQmwnEeqsNhp/87oYQIEo9TBALtKJt9DWsBkOMZFEUjMxWv8vH3kpVnQAowqBYD0J42LksqB+XD6MMMInF0w/AAfDbBUTy+F9+Lph6kaeCxcCQeA4j8fipFUeh+7lEqxCJRDeA4rqszW0Lm3k6ZKYwQJftOCN+PRDgbjhLCVU+KcoEnKTyGaUAr9zjVCLkyiAYAgHfpUCf/VsqMUch+fC9iaQEt3dmBagBpM8fBu6nUH6JtsAto0hgRODtOfTjBNUKQ5NtiSxACNhTfz+HgQUQD9iLzcZIgEnwUZ+A4olA5R1tQev8ql+shtK+dk/0+azZZAjyHkwgsznlIjkLQkWDAH873EyUQXDFRmlI7Ah/41TjgAggQG61KnqWM4hYXZ7S4Rg1BWJrWCC0hGGjtnEOrsZZ4IU5RWuRlkKPfFPhKu5ILa5ewmtzQMhgG+0HS4XHjpnwgCT+rXMOxrCTYUSjkDSnwrEKSbwAShCi1kZ9KjadSwI+jbwKai5g7xx2vNp5JmZ8LvItE34C+4qmuvrwu+ExgnqUOpF1YMm87eb5L53om8E+l3wFALp7J91nSrj9KTJ8W+y10ACcZd9CfwvOC6oSqnkX2kuJtKWBTlL0d5fdCW+DpPYE/gCV2U67ePTl4IAefqG5yD3DM01T72Nv766NXHwDusSC7jOnGmtD44kF5o34g9LKHOCoWPcIp/XRaiz6Uw+zzXsJZ8DwSQuwhSdjH5oRRGMJfhoxHIXgwomovIGrQrLvVHJGa4cPd+uUbYF+N4XAfcPw76nQTsZIkBz8+yoDjoPJg1Jm3Q+8RNUX5CAg0w2ara7LyBftcfcyuCBB4D4tlb514cewKP+d27+ThB1wYtGl3ghFJblz6ktvN9xXyZ1+oNt83nT7xNWJMnl09//mx49qL45xXF4a/Rk5fuXi//Pm3YOoLL8GudBvvnlGOQ/8ufk6QgoOK8o7KjaMpXAO8so01Qs27MUk6iJAHEU+AxNGWUBJKMw6TPlni2mtUJsSiRkkjBYrhKBxmmg6Rc2VTKdW+Khi9V6sbzOpoO24buHZxc+RcMxROVF8n4Vyw3ctLp8b78qzJGSe5mSJLjrv7LI0QN+2IrXpurVPG7HDd1UibCGy8LAcsplN3fb4I2tdlHHfympzthDHVXXejDrbbjnnleNExlSFkCeIaAd4CEgjhbEkTqY3aTGQsWVNP4XdL7fHzxVCeAL+o8HCvmBEUW61pANTp5LzziQP5HakScFBajCQZlkzhJZ5yXDUW7+TrM3K7Qfbokwlf/vY6gkZNNX7G5qQ8Zv7dHQMZi5rmGpsjpw052uUwWfWJJbHWGR/Ln1/3BFpHeckxmGc0JHxWcsZmo3RxZD7xRHgblvPBcrqatDmW1jo5wLm4roH1+gz9foV34ChCS4LDZGhF1UNElZYUV+UftUsF92x6oFDdWOhvFl2LK9e2+mdXhgor+ZVtcy0M7R337IqrULZMrM+mw4tly2S/uThQWOmu94/uhBdh3nZTtZtbQ4n02GJ1fUVdF+4O19dRUfXXt0eLZ69VN3q2E/mhaxegv74ZhvfrqXV4j+YubPTob4rc3L3Nsf57+aEivHe3E+mf4Ast1HRwXLJB+w0ogvES3W6cxrvs+0f/RH07VBy45kzKUsDiGkdBRG6QLIGgJobu62RhHqTIstW1VR26AndJUuYaZGcHyUdIejNcrG6OrquSbls611GboScGrx2PZagDjThfSq3VZkPe1NQlmmuwBgIDEfKMpbPDuLmyPlocSKr3jMcC77OYJAdshg7cTuc8liO+280QTiCYxAVIll1eUyWFs8vHkn55+OjwQH4s78KpR5XHzcN2EwAiKWnkKOw0AnClLTcPe+mcKbbVZOycExOEFKLsQaeZzp0Qt5ouO3c2VqJ/6EyumvBj57fpF5fv66Dz4Lt1wuHqbRJX3nrJRHPYOtQAhYTbvEEa98Ejqal8JvA4oong7ZwW3tlA+Y0UgOtoZ/RhSmfKKZ/IQSJC1E9YSRROtqB8AntW0kQdURJHdCZa+UQKG+E6+gggOAGz53iidIQmYAajoHwaba/KbXkXKP+Pvw/+BQ==";
	private const int BINARY = 2;
	private const int RAW_BITS_COUNT = 7;
	private const int BYTE_BITS_COUNT = 8;
	private const int PACKED_REPEAT_BITS_COUNT = 6;
	private const int PACKED_REPEAT_COUNT = 63;

	public static string PNGToBase64String(string pngPath)
	{
		var img = new Image(pngPath);
		var rawBits = "";
		for (uint y = 0; y < img.Size.Y; y++)
			for (uint x = 0; x < img.Size.X; x++)
			{
				var value = img.GetPixel(x, y).A > byte.MaxValue / 2 ? "1" : "0";
				rawBits += value;
			}

		var isPacked = false;
		var prevBit = rawBits.Length > 1 ? rawBits[0] : default;
		var sameBitSequence = 0;
		var readIndexCounter = 0;

		var w = Convert.ToString(img.Size.X, BINARY).PadLeft(BYTE_BITS_COUNT * 2, '0');
		var h = Convert.ToString(img.Size.Y, BINARY).PadLeft(BYTE_BITS_COUNT * 2, '0');
		var bytes = new List<byte>
			{
				Convert.ToByte(w[0..BYTE_BITS_COUNT], BINARY),
				Convert.ToByte(w[BYTE_BITS_COUNT..^0], BINARY),
				Convert.ToByte(h[0..BYTE_BITS_COUNT], BINARY),
				Convert.ToByte(h[BYTE_BITS_COUNT..^0], BINARY),
			};

		for (int i = 0; i < rawBits.Length; i++)
		{
			var bit = rawBits[i];
			var hasProcessed = false;

			if (bit == prevBit)
				sameBitSequence++;
			// if start of new sequence while packed
			else if (isPacked)
				ProcessPackedSequence();

			// if end of repeated sequence (max 63 bits)
			if (hasProcessed == false && sameBitSequence == PACKED_REPEAT_COUNT)
				ProcessPackedSequence();

			// if end of image on repeated sequence
			if (isPacked && hasProcessed == false && i == rawBits.Length - 1)
			{
				readIndexCounter++;
				ProcessPackedSequence();
				break;
			}

			isPacked = sameBitSequence >= RAW_BITS_COUNT;

			// if end of raw sequence (max 7 bits)
			if (hasProcessed == false && isPacked == false &&
				readIndexCounter == RAW_BITS_COUNT && sameBitSequence < RAW_BITS_COUNT)
				ProcessRawSequence();

			isPacked = sameBitSequence >= RAW_BITS_COUNT;
			prevBit = bit;
			readIndexCounter++;

			void ProcessPackedSequence()
			{
				var length = Convert.ToByte(readIndexCounter);
				var byteStr = Convert.ToString(length, BINARY);
				byteStr = byteStr.PadLeft(PACKED_REPEAT_BITS_COUNT, '0');
				var finalByteStr = $"1{prevBit}{byteStr}".PadLeft(BYTE_BITS_COUNT, '0');
				var newByte = Convert.ToByte(finalByteStr, BINARY);

				hasProcessed = true;
				AddByte(newByte);
			}
			void ProcessRawSequence()
			{
				var raw = rawBits[(i - RAW_BITS_COUNT)..i];
				var finalByteStr = $"0{raw}";
				var newByte = Convert.ToByte(finalByteStr, BINARY);

				AddByte(newByte);
			}
		}

		// if end of image on raw sequence
		if (isPacked == false)
		{
			var raw = rawBits[^readIndexCounter..^0];
			raw = raw.PadLeft(RAW_BITS_COUNT, '0');
			var finalByteStr = $"0{raw}";
			var newByte = Convert.ToByte(finalByteStr, BINARY);

			AddByte(newByte);
		}

		return Convert.ToBase64String(Compress(bytes.ToArray()));

		void AddByte(byte newByte)
		{
			bytes.Add(newByte);
			sameBitSequence = 0;
			readIndexCounter = 0;
		}
	}
	public static Texture CreateTexture()
	{
		var bytes = Decompress(Convert.FromBase64String(DEFAULT_GRAPHICS_BASE64));
		var width = Convert.ToUInt16($"{bytes[0]}{bytes[1]}");
		var height = Convert.ToUInt16($"{bytes[2]}{bytes[3]}");
		var total = width * height;
		var decodedBits = "";
		var img = new Image(width, height);

		for (int i = 4; i < bytes.Length; i++)
		{
			var curByte = bytes[i];
			var bits = Convert.ToString(curByte, BINARY);
			bits = bits.PadLeft(BYTE_BITS_COUNT, '0');
			var isPacked = bits[0] == '1';
			var isLast = i == bytes.Length - 1;

			if (isPacked)
			{
				var color = bits[1];
				var repCountBinary = bits[2..^0];
				var repeatCount = Convert.ToByte(repCountBinary, BINARY);

				decodedBits += new string(color, repeatCount);
			}
			else
			{
				if (isLast)
				{
					var lastLength = total - decodedBits.Length;
					var lastRawBits = bits[^lastLength..^0];
					decodedBits += lastRawBits;
					break;
				}

				var rawBits = bits[1..^0];
				decodedBits += rawBits;
			}
		}

		for (uint i = 0; i < decodedBits.Length; i++)
		{
			var bit = decodedBits[(int)i];
			var color = bit == '1' ? Color.White : Color.Transparent;
			var x = i % width;
			var y = i / width;

			img.SetPixel(x, y, color);
		}

		return new Texture(img);
	}
	private static byte[] Compress(byte[] data)
	{
		var output = new MemoryStream();
		using (var stream = new DeflateStream(output, CompressionLevel.Optimal))
			stream.Write(data, 0, data.Length);

		return output.ToArray();
	}
	private static byte[] Decompress(byte[] data)
	{
		var input = new MemoryStream(data);
		var output = new MemoryStream();
		using (var stream = new DeflateStream(input, CompressionMode.Decompress))
			stream.CopyTo(output);

		return output.ToArray();
	}
}

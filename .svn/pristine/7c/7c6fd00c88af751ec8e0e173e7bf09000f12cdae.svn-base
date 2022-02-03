using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;


namespace Connection
{
    /// <summary>
    /// Шифрует одну последовательность [data] другой [password]. Один пароль можно другим зашифровать.
    /// Вроде с RSDN взят.
    /// </summary>
	public class CryptoProvider
	{
		public CryptoProvider()
		{
		}

		public static byte[] Encrypt(byte[] data,string password)
		{
			SymmetricAlgorithm sa = Rijndael.Create();
			ICryptoTransform ct = sa.CreateEncryptor(
				(new PasswordDeriveBytes(password,null)).GetBytes(16),
				new byte[16]);

			MemoryStream ms = new MemoryStream();
			CryptoStream cs = new CryptoStream(ms,ct,CryptoStreamMode.Write);

			cs.Write(data,0,data.Length);
			cs.FlushFinalBlock();

			return ms.ToArray();
		}

		public static string Encrypt(string data,string password)
		{
			if (data!="") return Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(data),password));
			else return "";
		}

		static public byte[] Decrypt(byte[] data,string password)
		{
			BinaryReader br = new BinaryReader(InternalDecrypt(data,password));
			return br.ReadBytes((int)br.BaseStream.Length);
		}

		static public string Decrypt(string data,string password)
		{
			String result = "";	
			try
			{
				CryptoStream cs = InternalDecrypt(Convert.FromBase64String(data),password);
				StreamReader sr = new StreamReader(cs);
				result = sr.ReadToEnd();
				return result;
			} 
			catch
			{
				return result;
			}
		}

		static CryptoStream InternalDecrypt(byte[] data,string password)
		{
			SymmetricAlgorithm sa = Rijndael.Create();
			ICryptoTransform ct = sa.CreateDecryptor(
				(new PasswordDeriveBytes(password,null)).GetBytes(16),
				new byte[16]);

			MemoryStream ms = new MemoryStream(data);
			return new CryptoStream(ms,ct,CryptoStreamMode.Read);
		}

	}
}

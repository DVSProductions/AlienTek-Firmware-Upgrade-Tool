using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
namespace AlienTek_Firmware_Upgrade_Tool;

internal static class Utils {
	public static async Task<byte[]> GetUrlFileToBytes(string Url) {
		try {
			using var client = new HttpClient();
			var response = await client.GetAsync(new Uri(Url)).ConfigureAwait(false);
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
		}
		catch(Exception) {
			return [];
		}
	}
	public static string GetString(object buffer) {
		var list = new List<byte>();
		for(var i = 0; i < ((IEnumerable<byte>)buffer).Count(); i++) {
			if(((byte[])buffer)[i] is not byte.MaxValue and not 0)
				list.Add(((byte[])buffer)[i]);
		}
		return Encoding.Default.GetString([.. list]);
	}

	public static object? BytesToStuct(byte[] bytes, Type type, int offset) {
		var num = Marshal.SizeOf(type);
		if(num <= bytes.Length - offset) {
			var intPtr = Marshal.AllocHGlobal(num);
			Marshal.Copy(bytes, offset, intPtr, num);
			var result = Marshal.PtrToStructure(intPtr, type);
			Marshal.FreeHGlobal(intPtr);
			return result;
		}
		return null;
	}

	public static int GetStuctSize(Type type) => Marshal.SizeOf(type);

	public static void ValueToArray(object dstArray, byte value, ref int offset) {
		((sbyte[])dstArray)[offset] = (sbyte)value;
		offset++;
	}

	public static void ValueToArray(Array dstArray, ushort value, ref int offset) {
		Buffer.BlockCopy(BitConverter.GetBytes(value), 0, dstArray, offset, 2);
		offset += 2;
	}

	public static void ValueToArray(Array dstArray, Array value, ref int offset) {
		Buffer.BlockCopy(value, 0, dstArray, offset, value.Length);
		offset += value.Length;
	}

	public static byte[] GetSubArray(Array srcArray, int start, int size) {
		var array = new byte[size];
		Buffer.BlockCopy(srcArray, start, array, 0, size);
		return array;
	}

	public static ushort ByteArrayToUInt16(Array src, int startIndex) {
		var array = new byte[2];
		Buffer.BlockCopy(src, startIndex, array, 0, 2);
		return BitConverter.ToUInt16(array, 0);
	}

	public static object? BytesToStuct(byte[] bytes, Type type) {
		if(bytes == null)
			return null;
		var num = Marshal.SizeOf(type);
		if(num <= bytes.Length) {
			var intPtr = Marshal.AllocHGlobal(num);
			Marshal.Copy(bytes, 0, intPtr, num);
			var result = Marshal.PtrToStructure(intPtr, type);
			Marshal.FreeHGlobal(intPtr);
			return result;
		}
		return null;
	}

	public static byte[] StructToBytes(object structObj) {
		var num = Marshal.SizeOf(structObj);
		var array = new byte[num];
		var intPtr = Marshal.AllocHGlobal(num);
		Marshal.StructureToPtr(structObj, intPtr, fDeleteOld: false);
		Marshal.Copy(intPtr, array, 0, num);
		Marshal.FreeHGlobal(intPtr);
		return array;
	}
}

////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Provider.Services.Cloud
{
	public interface ICloudService : IService
	{
		/// <summary>
		/// Reads data into the data array.
		/// </summary>
		/// <param name="path">The file path to read from.</param>
		/// <param name="data">The array to read into.</param>
		/// <returns>Whether the read succesfully executed.</returns>
		bool read(string path, byte[] data);

		/// <summary>
		/// Writes data out of data array.
		/// </summary>
		/// <param name="path">The file path to write to.</param>
		/// <param name="data">The array to write from.</param>
		/// <param name="size">The length of the array with data.</param>
		/// <returns>Whether the write succesfully executed.</returns>
		bool write(string path, byte[] data, int size);

		/// <summary>
		/// Checks the size of a file.
		/// </summary>
		/// <param name="path">The file path to check.</param>
		/// <param name="size">The size of the file.</param>
		/// <returns>Whether the check succesfully executed.</returns>
		bool getSize(string path, out int size);

		/// <summary>
		/// Checks whether the path already exists.
		/// </summary>
		/// <param name="path">The file path to check.</param>
		/// <param name="exists">Whether the file exists.</param>
		/// <returns>Whether the check succesfully executed.</returns>
		bool exists(string path, out bool exists);

		/// <summary>
		/// Deletes the path.
		/// </summary>
		/// <param name="path">The file path to delete.</param>
		/// <returns>Whether the deletion succesfully executed.</returns>
		bool delete(string path);
	}
}
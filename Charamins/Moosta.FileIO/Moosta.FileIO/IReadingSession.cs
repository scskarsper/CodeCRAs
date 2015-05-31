using System;
namespace Moosta.FileIO
{
	public interface IReadingSession
	{
		object GetObject(int key);
	}
}

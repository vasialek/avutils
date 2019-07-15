using SymfonyHelper.Definition.Models;
using System.IO;

namespace SymfonyHelper.Definition.Interfaces
{
	public interface ISqlParser
	{
		FilesModel[] ParseInsertIntoFiles(Stream stream);
	}
}

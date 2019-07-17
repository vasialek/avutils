using SymfonyHelper.Definition.Models;
using System.IO;

namespace SymfonyHelper.Definition.Interfaces
{
	public interface ISqlParser
	{
		string[] SplitIntoFields(string sql);

		FilesModel[] ParseInsertIntoFiles(Stream stream);
	}
}

using SymfonyHelper.Definition.Interfaces;
using SymfonyHelper.Definition.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymfonyHelper.BLL
{
	public class MysqlParser : ISqlParser
	{
		public FilesModel[] ParseInsertIntoFiles(Stream stream)
		{
			throw new NotImplementedException();
		}
	}
}

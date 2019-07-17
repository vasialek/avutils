using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SymfonyHelper.BLL;
using SymfonyHelper.Definition.Enums;
using SymfonyHelper.Definition.Interfaces;
using SymfonyHelper.Definition.Models;

namespace SymfonyHelper.Tests
{
	[TestClass]
	public class SqlParserTest
	{
		private const int Id = 112;
		private readonly ISqlParser _parser = null;
		private readonly string[] _lines = new string[]
		{
			"(112, 1, 0, 'A8011204.jpg', '0197836aba9ad1edf126c3bff2e39218.jpg', 1, NULL, 112, 5, '2010-10-18 21:19:31', '2010-10-18 21:00:12'),",
			"(63, 1, 0, 'Picture_572.jpg', '52c7f3a23e72e6dbe34e4e6d85672bc4.jpg', 1, NULL, 63, 6, '2010-10-18 08:56:44', '0000-00-00 00:00:00'),",
			"(850, 1, 0, 'R2075659.jpg', '81aecc118bab5f73f41c0105f93cc3ce.jpg', 1, NULL, 850, 5, '2010-10-18 21:19:33', '0000-00-00 00:00:00'),",
		};


		public SqlParserTest()
		{
			_parser = new MysqlParser();
		}

		#region Split into fields

		[TestMethod]
		public void SplitIntoFields_GetOne()
		{
			string[] fields = _parser.SplitIntoFields("123");

			fields.Single().Should().Be("123");
		}

		[TestMethod]
		public void SplitIntoFields_GetOne_WhenEnclosed()
		{
			string[] fields = _parser.SplitIntoFields("'1 3'");

			fields.Single().Should().Be("1 3");
		}

		[TestMethod]
		public void SplitIntoFields_GetTwo()
		{
			string[] fields = _parser.SplitIntoFields("123, 456");

			fields[1].Should().Be("456");
		}

		[TestMethod]
		public void SplitIntoFields_GetTwo_WhenEnclosed()
		{
			string sql = "12615, 'juosta.jpg'";

			string[] fields = _parser.SplitIntoFields(sql);

			fields.Should().HaveCount(2);
		}

		[TestMethod]
		public void SplitIntoFields_One_WhenEnclosedWithCommas()
		{
			string sql = "'1,2,3.jpg'";

			string[] fields = _parser.SplitIntoFields(sql);

			fields.Single().Should().Be("1,2,3.jpg");
		}

		[TestMethod]
		public void SplitIntoFields_One_WhenEscapedQuote()
		{
			string sql = "'1\\\'2'";

			string[] fields = _parser.SplitIntoFields(sql);

			fields.Single().Should().Be("1\\\'2");
		}

		[TestMethod]
		public void SplitIntoFields_CheckCount()
		{
			string sql = "1, '2,3,4'";

			string[] fields = _parser.SplitIntoFields(sql);

			fields.Should().HaveCount(2);
		}

		[TestMethod]
		public void SplitIntoFields_GetTwo_WhenTwoAreEnquoted()
		{
			string s = "'1', '23.jpg'";

			string[] fields = _parser.SplitIntoFields(s);

			fields[1].Should().Be("23.jpg");
		}

		#endregion

		#region Parse inserts into files table

		[TestMethod]
		public void ParseInsertIntoFiles_CheckCount()
		{
			var files = ReadSqlLines();

			files.Should().HaveCount(_lines.Length);
		}

		[TestMethod]
		public void ParseInsertIntoFiles_Id()
		{
			var files = ReadSqlLines();

			files[0].Id.Should().Be(Id);
		}

		[TestMethod]
		public void ParseInsertIntoFiles_StatusId()
		{
			var files = ReadSqlLines();

			// Expecting that Status ID 1 (from SQL) is New
			files.Single(x => x.Id == Id).StatusId.Should().Be(Statuses.New);
		}

		[TestMethod]
		public void ParseInsertIntoFiles_UniqueName()
		{
			var files = ReadSqlLines();

			files.Single(x => x.Id == Id).UniqueName.Should().Be("0197836aba9ad1edf126c3bff2e39218.jpg");
		}

		[TestMethod]
		public void ParseInsertIntoFiles_ObjectId()
		{
			var files = ReadSqlLines();

			files.Single(x => x.Id == Id).ObjectId.Should().Be(112);
		}

		[TestMethod]
		public void ParseInsertIntoFiles_ObjectType()
		{
			var files = ReadSqlLines();

			// Expecting object type 5 (from SQL) is Product
			files.Single(x => x.Id == Id).ObjectType.Should().Be(ObjectTypes.TypeProduct);
		}

		[TestMethod]
		public void ParseInsertIntoFiles_CreatedAt()
		{
			DateTime expected = DateTime.Parse("2010-10-18 21:00:12");

			var files = ReadSqlLines();

			files.Single(x => x.Id == Id).CreatedAt.Should().Be(expected);
		}

		[TestMethod]
		public void ParseInsertIntoFiles_UpdatedAt()
		{
			DateTime expected = DateTime.Parse("2010-10-18 21:19:31");

			var files = ReadSqlLines();

			files.Single(x => x.Id == Id).UpdatedAt.Should().Be(expected);
		}

		[TestMethod]
		public void ParseInsertIntoFiles_SkipCommentedDash()
		{
			string[] lines = new string[]
			{
				"-- Should be skipped",
				_lines[0],
			};

			var files = ReadSqlLines(lines);

			// Expecting first commented line is skipped
			files.Should().HaveCount(1);
		}

		[TestMethod]
		public void ParseInsertIntoFiles_SkipCommentedSharp()
		{
			string[] lines = new string[]
			{
				"#Should be skipped",
				_lines[0],
			};

			var files = ReadSqlLines(lines);

			// Expecting first commented line is skipped
			files.Should().HaveCount(1);
		}

		#endregion

		#region Fix exception for insert files

		[TestMethod]
		public void ParseInsertIntoFiles_StatusId_WhenIsNull()
		{
			string[] lines = new string[]
			{
				"(13354, NULL, NULL, 'hb01-x jellybean red bones.jpg', 'ddd2a21ad67aa2aaa4649c8b9fc20a96.jpg', 1, NULL, 2211, 6, '2015-04-21 06:31:44', '2015-04-21 06:31:44'),",
			};

			var files = ReadSqlLines(lines);

			// Expecting no exception when NULL status
			files.Should().HaveCount(1);
		}

		[TestMethod]
		public void ParseInsertIntoFiles_ObjectId_WhenIsNull()
		{
			string[] lines = new string[]
			{
				"11087, 2, NULL, 'gertuves 395, 396.jpg', '3e345cbe03ee4822f4a26d85985e5739.jpg', 265, NULL, 3580, 5, '2012-10-19 01:10:05', '2012-10-19 01:10:05'",
			};

			var files = ReadSqlLines(lines);

			files.Should().HaveCount(1);
		}

		[TestMethod]
		public void ParseInsertIntoFiles_SkipEmptyLine()
		{
			string[] lines = new string[]
			{
				_lines[0],
				" ",
			};

			var files = ReadSqlLines(lines);

			// Empty line should be skipped w/o exception
			files.Should().HaveCount(1);
		}

		[TestMethod]
		public void ParseInsertIntoFiles_EndWithSemicolon()
		{
			string[] lines = new string[]
			{
				"(550, 1, 0, 'Daily_Mini.jpg', '976414e4073ce80a15d4702f969596bf.jpg', 1, NULL, 550, 6, '2010-10-18 08:56:44', '0000-00-00 00:00:00');",
			};

			var files = ReadSqlLines(lines);

			files.Single().Id.Should().Be(550);
		}

		#endregion

		private FilesModel[] ReadSqlLines(string[] lines = null)
		{
			if (lines == null)
			{
				lines = _lines;
			}

			using (var linesStream = new MemoryStream())
			{
				using (var ms = new MemoryStream())
				{
					using (var w = new StreamWriter(ms))
					{
						for (int i = 0; i < lines.Length; i++)
						{
							w.WriteLine(lines[i]);
						}
						w.Flush();
						ms.WriteTo(linesStream);
					}
				}

				linesStream.Position = 0;
				return _parser.ParseInsertIntoFiles(linesStream);
			}
		}
	}
}

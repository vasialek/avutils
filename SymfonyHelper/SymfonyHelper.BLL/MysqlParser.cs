using SymfonyHelper.Definition.Enums;
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
		// Strip comma and braces from begining and end of line
		private static readonly char[] _sqlLineSeparators = "(),;".ToCharArray();

		// Strip quotes from fields
		private static readonly char[] _fieldSeparators = " '".ToCharArray();

		public string[] SplitIntoFields(string sql)
		{
			var fields = new List<string>();

			bool isField = false;
			string v = null;
			bool wasEscapce = false;
			char c = ' ';

			char? currentFieldSeparator = null;

			for (int i = 0; i < sql.Length; i++)
			{
				try
				{
					if (isField)
					{
						// Field could ends with comma (if not enclosed in quotes) or with quote (if enclosed with quotes)
						if (IsEndOfField(sql[i], currentFieldSeparator, wasEscapce))
						{
							// Get end of field value and put it in array
							fields.Add(v.Trim());
							v = "";
							isField = false;
							currentFieldSeparator = null;
						}
						else
						{
							// Collecting field value
							v += sql[i];
						}
					}
					else
					{
						// End of field, search for non-empty or for quote
						if (IsStartOfField(sql[i], ref currentFieldSeparator, wasEscapce))
						{
							isField = true;
							// Add to field if it is not quote
							if (currentFieldSeparator == null)
							{
								v += sql[i];
							}
						}
					}

					// TODO: handle several \
					wasEscapce = sql[i] == '\\';
				}
				catch (FormatException fex)
				{
					Console.WriteLine(fex.Message);
					Console.WriteLine(sql);
					Console.WriteLine("^".PadLeft(i + 1, ' '));
					throw;
				}
			}

			// Adds if value is
			if (String.IsNullOrEmpty(v) == false)
			{
				fields.Add(v.Trim());
			}

			return fields.ToArray();
		}

		public FilesModel[] ParseInsertIntoFiles(Stream stream)
		{
			var files = new List<FilesModel>();
			int nr = 0;
			string s;
			string[] ar = null;

			using (TextReader r = new StreamReader(stream))
			{
				while ((s = r.ReadLine()) != null)
				{
					try
					{
						s = s.Trim();
						if (s.Length > 10 && IsCommented(s) == false)
						{
							s = s.Trim(_sqlLineSeparators);
							ar = SplitIntoFields(s);

							var f = new FilesModel();
							f.Id = int.Parse(ar[0]);
							f.StatusId = (Statuses)(ParseIntIfNotNull(ar[1]) ?? 0);
							f.UniqueName = ar[4]?.Trim(_fieldSeparators);
							f.ObjectId = ParseIntIfNotNull(ar[7]) ?? 0;
							f.ObjectType = (ObjectTypes)(ParseIntIfNotNull(ar[8]) ?? 0);
							f.UpdatedAt = ParseDateTimeField(ar[9]);
							f.CreatedAt = ParseDateTimeField(ar[10]);

							files.Add(f);
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine("Error parsing line #{0}. {1}", nr, ex.Message);
						Console.WriteLine(s);
						Console.WriteLine("Splitted SQL:");
						for (int i = 0; i < ar?.Length; i++)
						{
							Console.WriteLine("{0}:  `{1}`", i, ar[i]);
						}
						Console.WriteLine(ex.ToString());
						Console.WriteLine("----------------------------------------------------------------------");
						throw;
					}
					nr++;
				}
			}

			return files.ToArray();
		}

		private int? ParseIntIfNotNull(string s)
		{
			s = s.Trim().ToUpperInvariant();

			return s == "NULL" ? default(int?) : int.Parse(s);
		}

		private DateTime ParseDateTimeField(string f)
		{
			if (DateTime.TryParse(f.Trim(_fieldSeparators), out DateTime dt))
			{
				return dt;
			}
			return default(DateTime);
		}

		private bool IsCommented(string s)
		{
			return s[0] == '-' || s[0] == '#';
		}

		private bool IsStartOfField(char c, ref char? currenctSeparator, bool wasEscaped)
		{
			if (Char.IsWhiteSpace(c))
			{
				return false;
			}

			// Let know that field is enclosed
			if (wasEscaped == false && (c == '\'' || c == '"'))
			{
				currenctSeparator = c;
				return true;
			}

			// SQL field should start with quote, digit or N (as NULL)
			if (Char.IsDigit(c) || c == 'n' || c == 'N')
			{
				return true;
			}

			// Skip comma after field
			if (c == ',')
			{
				return false;
			}

			throw new FormatException($"SQL field has incorrect symbol `{c}`.");
		}

		private bool IsEndOfField(char c, char? currentSeparator, bool wasEscape)
		{
			// If field is not enclosed in quotes
			if (currentSeparator == null)
			{
				return c == ',';
			}

			return wasEscape == false && currentSeparator == c;
		}

	}
}

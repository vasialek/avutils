using SymfonyHelper.Definition.Enums;
using System;

namespace SymfonyHelper.Definition.Models
{
	public class FilesModel
	{
		public int Id { get; set; }

		public Statuses StatusId { get; set; }

		public string UniqueName { get; set; }

		public int ObjectId { get; set; }

		public ObjectTypes ObjectType { get; set; }

		public DateTime CreatedAt { get; set; }

		public DateTime UpdatedAt { get; set; }
	}
}

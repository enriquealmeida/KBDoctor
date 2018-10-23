using System;
using Artech.Common.Helpers;

namespace Artech.Packages.TeamDevClient.CommandLine
{
	class TeamDevCommandLineParser : CommandLineParser
	{
		[ValueUsage("history|update", MatchPosition=true, Optional = false, LegalValues = "(?i)history|update")]
		private string Command = string.Empty;

		[ValueUsage("GXserver URL", Optional = true, AlternateName1 = "s")]
		public string ServerUrl = string.Empty;

		[ValueUsage("GXserver user name", Optional = true, AlternateName1 = "u")]
		public string ServerUsername = string.Empty;

		[ValueUsage("GXserver user password", Optional = true, AlternateName1 = "p")]
		public string ServerPassword = string.Empty;

		[ValueUsage("GXserver KB alias", Optional = true, AlternateName1 = "kb")]
		public string ServerKbAlias = string.Empty;

		[ValueUsage("GXserver KB version", Optional = true, AlternateName1 = "v")]
		public string ServerKbVersion = string.Empty;

		[ValueUsage("Minimum date", Optional = true, AlternateName1 = "from")]
		public DateTime FromDate = DateTime.MinValue;

		[ValueUsage("Maximum date", Optional = true, AlternateName1 = "to")]
		public DateTime ToDate = DateTime.MaxValue;

		[FlagUsage("Output in XML", Optional = true, AlternateName1 = "x", AlternateName2 = "xml")]
		public bool XmlOutput = false;

		[FlagUsage("Times in UTC", Optional = true, AlternateName1 = "utc")]
		public bool UtcTimes = false;

		public TeamDevOperation Operation;

		protected override void Parse(string[] args, bool ignoreFirstArg)
		{
			base.Parse(args, ignoreFirstArg);

			switch (Command.ToLower())
			{
				case "history":
					Operation = TeamDevOperation.History;
					break;

				case "update":
					Operation = TeamDevOperation.Update;
					break;

				default:
					throw new UsageException(Command.ToLower(), "Command not implemented");
			}

			if (FromDate != DateTime.MinValue)
				FromDate = DateTime.SpecifyKind(FromDate, UtcTimes ? DateTimeKind.Utc : DateTimeKind.Local);

			if (ToDate != DateTime.MaxValue)
				ToDate = DateTime.SpecifyKind(ToDate, UtcTimes ? DateTimeKind.Utc : DateTimeKind.Local);
		}
	}
}

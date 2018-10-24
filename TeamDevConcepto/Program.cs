using System;
using System.IO;
//using Artech.Common.Caching;
using Artech.Common.Exceptions;
using Artech.Common.Helpers.Assemblies;
using Artech.Common.Helpers.IO;

namespace Artech.Packages.TeamDevClient.CommandLine
{
	class Program
	{
		static int Main(string[] args)
		{
			TeamDevCommandLineParser parser = new TeamDevCommandLineParser();
			try
			{
				parser.Parse(args);
			}
			catch (TeamDevCommandLineParser.UsageException ex)
			{
				Console.Error.WriteLine(parser.GetUsage(ex.Message));
				return 1;
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex.Message);
				return 1;
			}

			if (!parser.XmlOutput)
			{
				Console.WriteLine("TeamDev {0}", parser.Operation);
				Console.WriteLine("\tServerUrl = {0}", parser.ServerUrl);
				Console.WriteLine("\tServerUsername = {0}", parser.ServerUsername);
				Console.WriteLine("\tServerPassword = {0}", parser.ServerPassword);
				Console.WriteLine("\tServerKbAlias = {0}", parser.ServerKbAlias);
				Console.WriteLine("\tServerKbVersion = {0}", parser.ServerKbVersion);
				Console.WriteLine("\tFromDate = {0} ({1})", parser.FromDate, parser.FromDate.Kind);
				Console.WriteLine("\tToDate = {0}", parser.ToDate);
				Console.WriteLine("\tXmlOutput = {0}", parser.XmlOutput);
			}

			StartGxBL();

			return ExecuteCommand(parser);
		}

		private static void StartGxBL()
		{
			string configurationFile = Path.Combine(AssemblyHelper.GetAssemblyDirectory(), "genexus.exe.config");
			ExceptionManager.ConfigurationFile = configurationFile;
			//CacheManager.ConfigurationFile = configurationFile;
			PathHelper.SetAssemblyInfo("Artech", "Genexus", "10Ev1");
			Artech.Core.Connector.StartBL();
		}

		private static int ExecuteCommand(TeamDevCommandLineParser parser)
		{
			try
			{
				switch (parser.Operation)
				{
					case TeamDevOperation.History:
						return ExecuteHistory(parser);
					case TeamDevOperation.Update:
						return ExecuteUpdate(parser);
					default:
						throw new Exception("Operation not implemented");
				}
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex.Message);
				Console.Error.WriteLine(ex.StackTrace);
				return 1;
			}
		}

		private static int ExecuteHistory(TeamDevCommandLineParser parser)
		{
			HistoryOperation operation = new HistoryOperation(parser.ServerUrl,
				parser.ServerUsername,
				parser.ServerPassword,
				parser.ServerKbAlias,
				parser.ServerKbVersion,
				parser.FromDate,
				parser.ToDate);

			operation.XmlOutput = parser.XmlOutput;
			operation.Execute();
			return 0;
		}

		private static int ExecuteUpdate(TeamDevCommandLineParser parser)
		{
			throw new NotImplementedException();
		}
	}
}

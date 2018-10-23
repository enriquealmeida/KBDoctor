using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
//using Artech.Packages.TeamDevClient.Clients;
//using Artech.Packages.TeamDevClient.Data;
using GeneXus.Server.Contracts;
using Artech.Packages.TeamDevClient.BL.Data;
using Artech.Packages.TeamDevClient.BL.Clients;

namespace Artech.Packages.TeamDevClient.CommandLine
{
	public class HistoryOperation
	{
		private string ServerUrl { get; set; }
		private string ServerUsername { get; set; }
		private string ServerPassword { get; set; }
		private string ServerKbAlias { get; set; }
		private string ServerKbVersion { get; set; }
		private DateTime FromDate { get; set; }
		private DateTime ToDate { get; set; }

        public IEnumerable<KBRevisionData> revisions { get; set; }

        public bool XmlOutput { get; set; }

		public HistoryOperation(string serverUrl, string serverUsername, string serverPassword, string serverKbAlias, string serverKbVersion, DateTime fromDate, DateTime toDate)
		{
			ServerUrl = serverUrl;
			ServerUsername = serverUsername;
			ServerPassword = serverPassword;
			ServerKbAlias = serverKbAlias;
			ServerKbVersion = serverKbVersion;
			FromDate = fromDate;
			ToDate = toDate;

			XmlOutput = false;
		}

		public HistoryOperation(string serverUrl, string serverUsername, string serverPassword, string serverKbAlias, string serverKbVersion)
			: this(serverUrl, serverUsername, serverPassword, serverKbAlias, serverKbVersion, DateTime.MinValue, DateTime.MaxValue)
		{
		}

		List<KBVersionData> GetKBVersions(ProxyData proxy)
		{
			try
			{
				return TeamWorkServiceClient.Server.GetKBVersions(proxy);
			}
			catch
			{
				Console.Error.Write("GetKBVersions(): ");
				throw;
			}
		}

		public List<KBRevisionData> GetRevisions(ProxyData proxy)
		{
			try
			{
				return TeamWorkServiceClient.Server.GetRevisions(proxy);
			}
			catch
			{
				Console.Error.Write("GetRevisions(): ");
				throw;
			}
		}


		public int Execute()
		{
			ITeamWorkServer service = new TeamWorkServiceClient();
			ProxyData proxy = new ProxyData() { 
				Url = ServerUrl,
				KnowledgeBase = ServerKbAlias,
				User = ServerUsername,
				Password = ServerPassword };

			List<KBVersionData> versions = GetKBVersions(proxy);
			proxy.VersionId = FindKBVersion(versions, ServerKbVersion);

			List<KBRevisionData> revisions = GetRevisions(proxy);
            this.revisions = FilterRevisions(revisions);

			return 0;
		}

		private int FindKBVersion(IEnumerable<KBVersionData> versions, string serverKbVersion)
		{
			if (string.IsNullOrEmpty(serverKbVersion))
				return FindTrunkVersion(versions);

			foreach (KBVersionData data in versions)
			{
				if (string.Compare(data.Name, serverKbVersion, true) == 0)
				{
					return data.Id;
				}
			}
			throw new Exception(string.Format("Could not find KBversion '{0}'", serverKbVersion));
		}

		private int FindTrunkVersion(IEnumerable<KBVersionData> versions)
		{
			foreach (KBVersionData data in versions)
			{
				if (data.IsTrunk)
				{
					return data.Id;
				}
			}
			throw new Exception("Could not find Trunk KBversion");
		}

		private IEnumerable<KBRevisionData> FilterRevisions(IEnumerable<KBRevisionData> revisions)
		{
			foreach(KBRevisionData data in revisions)
			{
				if (data.Timestamp < FromDate || data.Timestamp > ToDate)
					continue;

				yield return data;
			}
		}

		private void ListRevisions(IEnumerable<KBRevisionData> revisions)
		{
			if (XmlOutput)
				ListRevisionsAsXml(revisions);
			else
				ListRevisionsAsList(revisions);
		}

		private static void ListRevisionsAsList(IEnumerable<KBRevisionData> revisions)
		{
            foreach (KBRevisionData data in revisions)
                Console.WriteLine("{0} {1} - {2}",
                    data.Timestamp.ToLocalTime().ToString(Thread.CurrentThread.CurrentCulture.DateTimeFormat),
                    data.UserName,
                    data.Comments);
     

		}

		private static void ListRevisionsAsXml(IEnumerable<KBRevisionData> revisions)
		{
            Console.WriteLine("<?xml version=\"1.0\" encoding=\"iso-8859-1\" ?>");
			Console.WriteLine("<log>");

			foreach (KBRevisionData data in revisions)
			{
				Console.WriteLine("\t<logentry>");

				Console.WriteLine("\t\t<author>{0}</author>", data.UserName);
				Console.WriteLine("\t\t<date>{0}</date>", data.Timestamp.ToString("O"));
                string msg2 = data.Comments;
                if (msg2.Length > 100 )
                    msg2= msg2.Substring(0, 100);
                msg2 = msg2.Replace("<", "");
                msg2 = msg2.Replace(">", "");
                msg2 = msg2.Replace("&", " ");
                msg2 = msg2.Replace("ó", "o");
                msg2 = msg2.Replace("á", "a");
                msg2 = msg2.Replace("í", "i");
                msg2 = msg2.Replace(Environment.NewLine, " ");

                //string sPattern = "/^[\w .-]+$/";
                Console.WriteLine("\t\t<msg2>{0}</msg2>", msg2);
               // Console.WriteLine("\t\t<msg><![CDATA[ {0} ]]></msg>",data.Comments );
				Console.WriteLine("\t\t<actions>");

				foreach (KBRevisionActionData action in data.Actions)
				{
					Console.WriteLine("\t\t\t<action>");
                        Console.WriteLine("\t\t\t\t<operation>{0}</operation>", action.Operation);
					    Console.WriteLine("\t\t\t\t<objectType>{0}</objectType>", action.ObjectType);
					    Console.WriteLine("\t\t\t\t<objectName>{0}</objectName>", action.ObjectName); 
					    Console.WriteLine("\t\t\t\t<author>{0}</author>", action.UserName);
					    Console.WriteLine("\t\t\t\t<date>{0}</date>", action.Timestamp.ToString("o"));

					Console.WriteLine("\t\t\t</action>");
				}

                Console.WriteLine("\t\t</actions>");
				Console.WriteLine("\t</logentry>");
			}

			Console.WriteLine("</log>");
		}
	}
}

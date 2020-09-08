using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZapperWeb
{
	public class ProjectModel
	{
		public Guid ProjectID { get; set; }
		//public List<Ticket> Tickets {get; set;} // Only comment due to having no class with the name ticket
		public String Title { get; set; }
		public List<String> FilePaths { get; set; }
		public DateTime DateCreated { get; set; }
		public DateTime LastUpdate { get; set; }
		public List<Changelog> ChangeLog { get; set; }// list of (Name, Date) Past Date
		public List<RoadmapFeature> Roadmap { get; set; }// list of (Name, Date) Future Date
		public List<Fork> Forks { get; set; }// list of (Name, Path, Date, Version Number)
		public List<User> TriageUsers { get; set; }
	}


	public class Fork
	{

	}

	public class RoadmapFeature
	{

	}

	public class Changelog
	{
		public string ChangeTitle { get; set; }
		public Guid ChangeID { get; set; }
		public string Change { get; set; }
		public User ChangeUser { get; set; }
	}
}

﻿using System;
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
		//public ChangeLog { get; set; }// list of (Name, Date) Past Date
		//public RoadMap { get; set; }// list of (Name, Date) Future Date
		//public Forks { get; set; }// list of (Name, Path, Date, Version Number)
		//public List<User> TriageUsers { get; set; }
	}
}

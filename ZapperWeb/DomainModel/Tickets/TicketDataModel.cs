using DomainModel.CAST;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomainModel.Tickets
{
    public class TicketDataModel : ICASTDataModel
    {
        /// <summary>
        /// Unique identifier for the device record
        /// </summary>
        [BsonId(IdGenerator = typeof(CombGuidGenerator))]
        public Guid Id { get; set; }
        public string Title { get; set; }
        public int Severity { get; set; }
        public List<string> Reproduction { get; set; }
        public List<string> Files { get; set; }
        public List<string> Comments { get; set; }
        public int Status { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime DateCreated { get; set; }
        public string ProjectBuild { get; set; }
        public string ProjectVersion { get; set; }
        public Guid AssigneeID { get; set; }
        public Guid StatusHistory { get; set; }
    }
}

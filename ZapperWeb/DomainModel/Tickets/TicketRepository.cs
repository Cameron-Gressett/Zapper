using DomainModel.CAST;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomainModel.Tickets
{
    public class TicketRepository : CASTMongoRepository<TicketDataModel>
    {
        protected override IMongoCollection<TicketDataModel> CreateCollection(string tenant)
        {
            throw new NotImplementedException();
        }
    }
}

using DomainModel.CAST;
using DomainModel.HelperClasses;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomainModel.Tickets
{
    public class TicketRepository : MongoRepository<TicketDataModel>
    {
        protected override IMongoCollection<TicketDataModel> CreateCollection()
        {
            throw new NotImplementedException();
        }

        public TicketRepository()
        {

        }
    }
}

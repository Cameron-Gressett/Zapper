using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZapperWeb
{
    public class User
    {
        public Guid UserID { get; set; }
        public string Name { get; set; }
        public int AccountType { get; set; }
        public HashCode PasswordHash { get; set; }
        public List<Guid> TicketIDs { get; set; }
        public List<Guid> ProjectIDs { get; set; }
        public List<Guid> FavoriteTicketIDs { get; set; }
        public List<Guid> FavoriteProjectIDs { get; set; }
        public List<Notification> Notifications { get; set; } 
        //public List<SearchFilter> SavedFilters { get; set; }
    }

    public class Notification
    {
        public Guid NotificationID { get; set; }
        public string NotificationText { get; set; }
        public Guid UserID { get; set; }
    }

    public class SearchFilter
    {
        string TextSearch { get; set; }
        Guid OrganizationID { get; set; }

    }
}

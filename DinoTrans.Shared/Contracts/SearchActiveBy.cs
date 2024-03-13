using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.Contracts
{
    public enum SearchActiveByMachines
    {
        All,
        LessThan8Tons,
        From8To22Tons,
        From22Tons
    }

    public enum SearchActiveByOffers
    {
        All,
        NoOffers,
        Max5Offers,
        MoreThan5Offers
    }
}

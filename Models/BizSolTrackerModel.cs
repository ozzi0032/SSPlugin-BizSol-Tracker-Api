using SmartStore.Web.Framework.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BizSol.Tracker.Api.Models
{
    public class BizSolTrackerModel : ModelBase
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string PublishingDate { get; set; }
    }
}
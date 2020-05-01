using Assets.Scripts.Local;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Messages
{
    public class MessageModel
    {
        public Guid MessageId { get; set; }

        public ActionObject Action { get; set; }
    }
}

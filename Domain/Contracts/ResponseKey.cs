using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Contracts
{
    public class ResponseKey
    {
        public ResponseKey()
        {

        }
        public ResponseKey(string key)
        {
            this.key = key;
        }
        public string? key { get; set; }
        public class ResponseKeyContract : ResponseKey
        {
            public bool emailConfirmationRequired { get; set; }
            public UserApprovalStatusEnum UserApprovalStatus { get; set; }
            public string? RejectionReason { get; set; }
        }
    }
}

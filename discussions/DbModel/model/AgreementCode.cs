using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Discussions.model
{
    public enum AgreementCode
    {
        Unsolved = 0,
        Agreed = 1,
        Disagreed = 2,
        UnsolvedAndGrouped = 3,
        AgreedAndGrouped = 4,
        DisagreedAndGrouped = 5
    }
}

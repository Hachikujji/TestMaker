using System;
using System.Collections.Generic;
using System.Text;

namespace TestMaker.Database.Models
{
    public class GetTestRequest
    {
        public GetTestRequest(int testId)
        {
            TestId = testId;
        }

        public int TestId { get; set; }
    }
}
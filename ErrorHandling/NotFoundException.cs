using System;
using System.Collections.Generic;
using System.Text;

namespace WebAPITools.ErrorHandling
{
    public class NotFoundException: Exception
    {
        public NotFoundException()
        {

        }

        public NotFoundException(string AMessage): base(AMessage)
        {

        }
    }
}

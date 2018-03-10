using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA3
{
    public class ContainerClass
    {
        #region Variables
        Person user;
        #endregion

        public ContainerClass()
        {
            user = new Person();
        }

        public void setPerson(Person user)
        {
            this.user = user;
        }

        public Person getPerson()
        {
            return this.user;
        }
    }
}

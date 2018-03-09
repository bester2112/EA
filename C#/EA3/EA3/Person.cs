using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA3
{
    enum Gender { man, woman, na };
    class Person
    {
        private int age;
        private Gender sex;

        public Person()
        {
            this.age = -1;
            this.sex = Gender.na;
        }

        public Person(int age, Gender sex)
        {
            this.age = age;
            this.sex = sex;
        }

        public Gender getGender()
        {
            return this.sex;
        }

        public int getAge()
        {
            return this.age;
        }

        public void setGender(Gender sex)
        {
            this.sex = sex;
        }

        public void setAge(int age)
        {
            this.age = age;
        }
    }
}

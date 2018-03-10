using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA3
{
    public enum Gender { MAN, WOMAN, NA };
    public enum Emotion { VERYMAD, MAD, OK, GOOD, VERYGOOD, NA };
    public class Person
    {
        private int age;
        private Gender sex;
        private List<Emotion> emotes;

        public Person()
        {
            this.age = -1;
            this.sex = Gender.NA;
            emotes = new List<Emotion>();
        }

        public Person(int age, Gender sex)
        {
            this.age = age;
            this.sex = sex;
        }

        #region getter
        public Gender getGender()
        {
            return this.sex;
        }

        public int getAge()
        {
            return this.age;
        }
        #endregion

        #region setter
        public void setGender(Gender sex)
        {
            this.sex = sex;
        }

        public void setAge(int age)
        {
            this.age = age;
        }
        #endregion

        public void addEmotion(Emotion emote)
        {
            emotes.Add(emote);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

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
            emotes = new List<Emotion>();
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


        // https://msdn.microsoft.com/de-de/library/a0h36syw(v=vs.110).aspx
        public override string ToString()
        {
            string str = "";

            str += string.Format("{0},{1},", this.age, this.sex) + Environment.NewLine;
            
            for (int i = 0; i < this.emotes.Count; i++)
            {
                str += string.Format("{0},", this.emotes[i].ToString("F"));
            }
            str += Environment.NewLine;

            return str;
        }
    }
}

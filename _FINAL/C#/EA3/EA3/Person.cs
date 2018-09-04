using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace EA3
{
    public enum Gender { MAN, WOMAN, NA, NOINPUT };
    public enum Emotion { VERYMAD, MAD, OK, GOOD, VERYGOOD, NA };
    public class Person
    {
        private int age;
        private Gender sex;
        private bool musically;
        private bool tactile;
        private bool watch;
        private bool games;
        private List<Emotion> emotes;

        public Person()
        {
            this.age = -1;
            this.sex = Gender.NA;
            this.musically = false;
            this.tactile = false;
            this.watch = false;
            this.games = false;
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

        public List<Emotion> getEmotion()
        {
            return this.emotes;
        }

        public bool isMusically()
        {
            return this.musically;
        }

        public bool playedGames()
        {
            return this.games;
        }

        public bool usedTactile()
        {
            return this.tactile;
        }

        public bool usedWatch()
        {
            return this.watch;
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

        public void setMusically(bool musically)
        {
            this.musically = musically;
        }

        public void setTactile(bool usedTactile)
        {
            this.tactile = usedTactile;
        }
        public void setWatch(bool usedWatch)
        {
            this.watch = usedWatch;
        }
        public void setGames(bool playedGames)
        {
            this.games = playedGames;
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

            str += string.Format("Alter : {0}" + Environment.NewLine + "Geschlecht: {1}", this.age, this.sex) + Environment.NewLine;
            str += string.Format("Musikalisch ? = {0}" + Environment.NewLine + "spielt Spiele ? = {1}" + Environment.NewLine + 
                                 "used Watch ? = {2}" + Environment.NewLine + "used tactile ? = {3}", this.musically, this.games, this.watch, this.tactile);

            str += "Stimmung in der " + Environment.NewLine;
            for (int i = 0; i < this.emotes.Count; i++)
            {
                str += string.Format("{0}. Itteration = {1}", i, this.emotes[i].ToString("F"));
            }
            str += Environment.NewLine;

            return str;
        }
    }
}

using System;
using System.Collections.Generic;

namespace EasyChampionSelection.ECS {

    /// <summary>
    /// A class that manages champions (as string only)
    /// </summary>
    [Serializable]
    public class ChampionList {
        private string strName = "";
        private List<string> lstChampions;

        /// <summary>
        /// Returns the number of champions in the list
        /// </summary>
        /// <returns></returns>
        public int getCount() {
            return lstChampions.Count;
        }

        /// <summary>
        /// Get the name of the <c>ChampionList</c>
        /// </summary>
        public string getName() {
            return strName;
        }

        /// <summary>
        /// Set the name of the <c>ChampionList</c>
        /// </summary>
        public void setName(string newName) {
            strName = newName;
        }

        /// <summary>
        /// Get the count of the ammount of champions in the list
        /// </summary>
        public int ChampionCount {
            get { return lstChampions.Count; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Set the name of the <c>ChampionList</c>. Can be changed later!</param>
        public ChampionList(string name) {
            strName = name;
            lstChampions = new List<string>();
        }

        /// <summary>
        /// Get a champion based on index
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Throws an ArgumentOutOfRangeException if the given index is &lt; 0 or &gt; the amount of champions in the list
        /// </exception>
        public string getChampion(int index) {
            if(index > 0 && index < ChampionCount) {
                return lstChampions[index];
            } else {
                throw new ArgumentOutOfRangeException("Index: " + index.ToString() + " is out of range!");
            }
        }

        /// <summary>
        /// Get the list of champions
        /// </summary>
        public List<string> getChampions() {
            return lstChampions;
        }

        /// <summary>
        /// Searches if a champion is to be found in the list
        /// </summary>
        /// <returns></returns>
        public bool Contains(string championName) {
            return lstChampions.Contains(championName);
        }

        /// <summary>
        /// Adds a champion if it is not already in the list
        /// </summary>
        public void AddChampion(string name) {
            if(!lstChampions.Contains(name)) {
                lstChampions.Add(name);
                lstChampions.Sort();
            }
        }

        private void AddChampionNoSort(string name) {
            if(!lstChampions.Contains(name)) {
                lstChampions.Add(name);
            }
        }

        /// <summary>
        /// Adds a list of champions if they are not already in the list
        /// </summary>
        /// <param name="names"></param>
        public void AddChampions(List<string> names) {
            for (int i = 0; i < names.Count; i++)
			{
                AddChampionNoSort(names[i]);
			}
            lstChampions.Sort();
        }

        /// <summary>
        /// Remove a champion based on it's name.
        /// </summary>
        public void RemoveChampion(string name) {
            lstChampions.Remove(name);
        }

        /// <summary>
        /// Removes a list of champions based on their name.
        /// </summary>
        /// <param name="names"></param>
        public void RemoveChampions(List<string> names) {
            for(int i = 0; i < names.Count; i++) {
                RemoveChampion(names[i]);
            }
        }

        /// <summary>
        /// Returns the name of the list
        /// </summary>
        public override string ToString() {
            return strName;
        }

        /// <summary>
        /// Determines if the list are equal.
        /// </summary>
        /// <exception cref="ArgumentNullException">Throws an ArgumentNullException if your argument is null</exception>
        public override bool Equals(ChampionList p) {
            if(p == null) {
                throw new ArgumentNullException();
            }

            if(this.getName() == p.getName()) { // Name
                if(this.getCount() == p.getCount()) { // Equal count of champions in list
                    for(int i = 0; i < p.getCount(); i++) {
                        if(!(this.getChampion(i) == p.getChampion(i))) { // If not champName(i) == champName(i)
                            return false;
                        }
                    }
                } else {
                    return false;
                }
            } else {
                return false;
            }
            
            return true;
        }
    }
}

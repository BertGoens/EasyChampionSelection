using System;
using System.Collections.Generic;

namespace EasyChampionSelection.ECS.RiotGameData {

    /// <summary>
    /// A class that manages champions (as string only)
    /// </summary>
    [Serializable]
    public class ChampionList {
        private string strName = "";
        private List<string> lstChampions;

        public delegate void ChangedEventHandler(ChampionList sender, EventArgs e);

        /// <summary>
        /// The list has changed name.
        /// </summary>
        [field: NonSerialized]
        public event ChangedEventHandler NameChanged;

        /// <summary>
        /// The list has changed it's amount of champions.
        /// </summary>
        [field: NonSerialized]
        public event ChangedEventHandler ChampionsChanged;

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
            if(NameChanged != null) {
                NameChanged(this, new EventArgs());
            }
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
            if(index > -1 && index < ChampionCount) {
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

                if(ChampionsChanged != null) {
                    ChampionsChanged(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Adds a champion if not already in the list without sorting afterwards.
        /// </summary>
        /// <returns>True = new champion added</returns>
        private bool AddChampionNoSort(string name) {
            if(!lstChampions.Contains(name)) {
                lstChampions.Add(name);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds a list of champions if they are not already in the list
        /// </summary>
        /// <param name="names"></param>
        public void AddChampions(List<string> names) {
            bool changed = false;
            for (int i = 0; i < names.Count; i++)
			{
                if(AddChampionNoSort(names[i])) {
                    changed = true;
                }
			}
            if(changed) {
                lstChampions.Sort();
                if(ChampionsChanged != null) {
                    ChampionsChanged(this, new EventArgs());
                }   
            }
        }

        /// <summary>
        /// Remove a champion based on it's name.
        /// </summary>
        public void RemoveChampion(string name) {
            int champsPreRemove = getCount();
            lstChampions.Remove(name);
            if(getCount() != champsPreRemove) {
                if(ChampionsChanged != null) {
                    ChampionsChanged(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Remove a champion based on it's name.
        /// Private use for removing a list without triggering the event a million times while removing.
        /// </summary>
        private bool RemoveChampionNoEvent(string name) {
            int champsPreRemove = getCount();
            lstChampions.Remove(name);
            if(getCount() != champsPreRemove) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes a list of champions based on their name.
        /// </summary>
        /// <param name="names"></param>
        public void RemoveChampions(List<string> names) {
            bool changed = false;
            for(int i = 0; i < names.Count; i++) {
                if(RemoveChampionNoEvent(names[i])) {
                    changed = true;
                }
            }
            if(changed) {
                if(ChampionsChanged != null) {
                    ChampionsChanged(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Removes all champions in the list
        /// </summary>
        public void RemoveAllChampions() {
            bool changed = false;
            if(lstChampions.Count > 0) {
                changed = true;
                lstChampions.Clear();
            }
            if(changed) {
                if(ChampionsChanged != null) {
                    ChampionsChanged(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Returns the name of the list
        /// </summary>
        public override string ToString() {
            return strName;
        }
    }
}

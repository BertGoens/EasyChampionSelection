using System;
using System.Collections.Generic;

namespace EasyChampionSelection.ECS {

    [Serializable]
    public class ChampionList {
        private string strName = "";
        private List<string> lstChampions;

        public string getName() {
            return strName;
        }

        public void setName(string newName) {
            strName = newName;
        }

        public int ChampionCount {
            get { return lstChampions.Count; }
        }

        public ChampionList(string name) {
            strName = name;
            lstChampions = new List<string>();
        }

        public string getChampion(int index) {
            if(index > -1 && index < ChampionCount) {
                return lstChampions[index];
            } else {
                return null;
            }
        }

        public List<string> getChampions() {
            return lstChampions;
        }

        public bool Contains(string champ) {
            return lstChampions.Contains(champ);
        }

        public void AddChampion(string name) {
            if(!lstChampions.Contains(name)) {
                lstChampions.Add(name);
                lstChampions.Sort();
            }
        }

        public void AddChampions(List<string> names) {
            for (int i = 0; i < names.Count; i++)
			{
                AddChampion(names[i]);
			}
        }

        public void RemoveChampion(string name) {
            lstChampions.Remove(name);
        }

        public void RemoveChampions(List<string> names) {
            for(int i = 0; i < names.Count; i++) {
                RemoveChampion(names[i]);
            }
        }

        public override string ToString() {
            return strName;
        }
    }
}

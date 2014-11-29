using System;
using System.Collections.Generic;

namespace EasyChampionSelection.ECS {

    [Serializable]
    public class GroupManager {
        private const int intMaxGroups = 12;
        private List<ChampionList> lstGroupList;

        public GroupManager() {
            lstGroupList = new List<ChampionList>(intMaxGroups);
        }

        public int MaxGroups {
            get { return intMaxGroups; }
        }

        public int GroupCount {
            get { return lstGroupList.Count; }
        }

        public ChampionList getChampionList(int index) {
            if(index > -1 && index < GroupCount) {
                return lstGroupList[index];
            } else {
                return null;
            }
        }

        public List<ChampionList> getGroupList() {
            return lstGroupList;
        }

        public ChampionList getGroup(int index) {
            if(index > -1 && index < lstGroupList.Count) {
                return lstGroupList[index];
            }
            return null;
        }

        public ChampionList getGroup(string name) {
            for(int i = 0; i < lstGroupList.Count; i++) {
                if(lstGroupList[i].getName() == name) {
                    return lstGroupList[i];
                }
            }
            return null;
        }

        public void RemoveGroup(int index) {
            if(index > -1 && index < GroupCount) {
                lstGroupList.RemoveAt(index);
            }
        }

        public void RemoveGroup(ChampionList group) {
            foreach(ChampionList item in lstGroupList) {
                if(group.getName().Equals(item.getName())) {
                    lstGroupList.Remove(item);
                }
            }
        }

        public void AddGroup(ChampionList newGroup) {
            if(lstGroupList.Count < intMaxGroups) {
                foreach(ChampionList item in lstGroupList) {
                    if(item.getName().Equals(newGroup.getName())) {
                        return;
                    }
                }
                lstGroupList.Add(newGroup);
            }
        }

        public void ReOrder(List<ChampionList> groups) {
            lstGroupList.Clear();
            for (int i = 0; i < groups.Count; i++) {
			    lstGroupList.Add(groups[i]);
			}
        }

        public void ReOrder(ChampionList cList, int newPosition) {

            if(lstGroupList.IndexOf(cList) == newPosition) {
                return; // same index
            }

            if(newPosition < 0 || newPosition >= lstGroupList.Count) {
                return; // Index out of range - nothing to do
            }

            lstGroupList.Remove(cList); // Removing removable element
            lstGroupList.Insert(newPosition, cList); // Insert it in new position
        }

        public override string ToString() {
            return lstGroupList.Count + "/" + intMaxGroups + " groups";
        }
    }
}

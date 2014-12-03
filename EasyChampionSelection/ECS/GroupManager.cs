using System;
using System.Collections.Generic;

namespace EasyChampionSelection.ECS {

    /// <summary>
    /// A managing class that handles <c>ChampionList</c>
    /// </summary>
    [Serializable]
    public class GroupManager {
        private const int intMaxGroups = 12;
        private List<ChampionList> lstGroupList;

        /// <summary>
        /// Constructor
        /// </summary>
        public GroupManager() {
            lstGroupList = new List<ChampionList>(intMaxGroups);
        }

        /// <summary>
        /// Returns the maximum amount of groups allowed
        /// </summary>
        public int MaxGroups {
            get { return intMaxGroups; }
        }

        /// <summary>
        /// Count the current number of groups
        /// </summary>
        public int GroupCount {
            get { return lstGroupList.Count; }
        }

        /// <summary>
        /// Get all groups
        /// </summary>
        public List<ChampionList> getAllGroups() {
            return lstGroupList;
        }

        /// <summary>
        /// Get a group based on the given name
        /// </summary>
        /// <exception cref="ArgumentException">Throws an ArgumentException if the group is not found</exception>
        public ChampionList getGroup(int index) {
            if(index > -1 && index < lstGroupList.Count) {
                return lstGroupList[index];
            }
            throw new ArgumentException("Group @ " + index + " not found");
        }

        /// <summary>
        /// Get a group based on it's name.
        /// </summary>
        /// <exception cref="ArgumentException">Throws an ArgumentException if the group is not found</exception>
        public ChampionList getGroup(string name) {
            for(int i = 0; i < lstGroupList.Count; i++) {
                if(lstGroupList[i].getName() == name) {
                    return lstGroupList[i];
                }
            }
            throw new ArgumentException("Group: " + name + " not found");
        }

        /// <summary>
        /// Remove a group based on index
        /// </summary>
        public void RemoveGroup(int index) {
            if(index > -1 && index < GroupCount) {
                lstGroupList.RemoveAt(index);
            }
        }

        /// <summary>
        /// Remove a group based on name
        /// </summary>
        public void RemoveGroup(string name) {
            for(int i = 0; i < lstGroupList.Count; i++) {
                if(lstGroupList[i].getName() == name) {
                    lstGroupList.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Add a new group.
        /// </summary>
        /// <exception cref="ArgumentExcception">Throws an ArgumentException if the group name is not unique.</exception>
        public void AddGroup(ChampionList newGroup) {
            if(lstGroupList.Count < intMaxGroups) {
                foreach(ChampionList item in lstGroupList) {
                    if(item.getName().Equals(newGroup.getName())) {
                        throw new ArgumentException("Name duplicate!");
                    }
                }
                lstGroupList.Add(newGroup);
            }
        }

        /// <summary>
        /// Move a group up or down in the list.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Throws an ArgumentOutOfRangeException 
        /// if the newPosition parameter is greater than the amount of groups or &lt; 1</exception>
        public void ReOrder(ChampionList cList, int newPosition) {

            if(lstGroupList.IndexOf(cList) == newPosition) {
                return; // same index
            }

            if(newPosition < 0 || newPosition >= lstGroupList.Count) {
                throw new ArgumentOutOfRangeException("Out of range: " + newPosition.ToString()); // Index out of range - nothing to do
            }

            lstGroupList.Remove(cList); // Removing removable element
            lstGroupList.Insert(newPosition, cList); // Insert it in new position
        }

        public override string ToString() {
            return lstGroupList.Count + "/" + intMaxGroups + " groups";
        }
    }
}

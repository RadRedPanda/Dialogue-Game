using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts {
    [Serializable]
    public class DialogueNode {
        public string choice; // dialogue option that triggers this node
        public string speaker; // name of character currently speaking
        public List<string> dialogue; // list that contains all the dialogue for the node
        public List<int> options; // list of nodes that this one connects to
        
        public DialogueNode(string c, string s, List<string> d, List<int> o) {
            choice = c;
            speaker = s;
            dialogue = d;
            options = o;
        }
    }
}

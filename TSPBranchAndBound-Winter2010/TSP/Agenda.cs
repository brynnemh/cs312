using Priority_Queue;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP
{
    class Agenda
    {
        Dictionary<int, HeapPriorityQueue<State>> statespace = new Dictionary<int, HeapPriorityQueue<State>>();
        public Agenda() {}

        public void clear()
        {
            statespace.Clear();
        }

        public bool empty()
        {
            if(statespace.Count==0)
            {
                return true;
            }
            return false;
        }

        public void add (State s)
        {
            int key = s.getTour().Count;

            if(statespace.ContainsKey(key))
            {
                statespace[key].Enqueue(s, s.bound);
            }
            else
            {
                HeapPriorityQueue<State> entry = new HeapPriorityQueue<State>(65536);
                entry.Enqueue(s, s.bound);
                statespace.Add(key, entry);
            }
        }

        public State remove_first()
        {
            //Copy keys of map
            int[] levels = new int[statespace.Count];
            statespace.Keys.CopyTo(levels, 0);

            //Find the highest key
            int champ = -1;
            for (int i = 0; i<levels.Length; i++)
            {
                if(levels[i]>champ)
                {
                    champ = levels[i];
                }
            }

            //Then return it
            if(champ != -1)
            {
                State result=statespace[champ].Dequeue();
                if (statespace[champ].Count==0)
                {
                    statespace.Remove(champ);
                }
                return result;
            }
            return null;
        }

        public State first()
        {
            //Copy keys of map
            int[] levels = new int[statespace.Count];
            statespace.Keys.CopyTo(levels, 0);

            //Find the highest key
            int champ = -1;
            for (int i = 0; i < levels.Length; i++)
            {
                if (levels[i] > champ)
                {
                    champ = levels[i];
                }
            }

            //THen return it
            if (champ != -1)
            {
                return statespace[champ].First;
            }
            return null;
        }
    }
}

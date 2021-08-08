using System;
using System.Collections.Generic;
using Random = System.Random;

namespace Utilities
{
    // https://gamedev.stackexchange.com/questions/162976/how-do-i-create-a-weighted-collection-and-then-pick-a-random-element-from-it
    public class WeightedRandomBag<T>  {
        public struct Entry {
            public double accumulatedWeight;
            public double weight;
            public int item;
        }

        private List<Entry> entries = new List<Entry>();
        private double accumulatedWeight;
        private Random rand;

        public WeightedRandomBag(Random rand, int sizeOfBag)
        {
            this.rand = rand;
            for (int i = 0; i < sizeOfBag; i++)
            {
                AddEntry(i, 1f/ sizeOfBag);
            }
        
        }

        public void AddEntry(int item, double weight) {
            accumulatedWeight += weight;
            entries.Add(new Entry { item = item, weight = weight , accumulatedWeight = accumulatedWeight });
        }

        public void RemoveEntry(Entry item)
        {
            Entry beforeItemToRemove = new Entry(){item = -1, weight = 0 , accumulatedWeight = 0};
            int index = 0;
            while (!(entries[index].accumulatedWeight > item.accumulatedWeight))
            {
                index++;
            }

            if (index != 0)
            {
                beforeItemToRemove = entries[index - 1];
            }
        
            var entryToRemove = entries[index];

            while (index + 1 < entries.Count)
            {
                var entry = entries[index + 1];
                entry.accumulatedWeight = beforeItemToRemove.accumulatedWeight + entry.weight;
                entries[index + 1] = entry;
                beforeItemToRemove = entry;
                index++;
            }
        
            entries.Remove(entryToRemove);
            accumulatedWeight -= entryToRemove.weight;
        }

        public void UpdateWeights(float[] weightMap)
        {
            accumulatedWeight = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                Entry tmp = entries[i];
                tmp.weight += weightMap[i];
                accumulatedWeight += tmp.weight;
                tmp.accumulatedWeight = accumulatedWeight;
                entries[i] = tmp;
            }
        }
        
        public int GetRandom() {
            double r = rand.NextDouble() * accumulatedWeight;

            foreach (Entry entry in entries) {
                if (entry.accumulatedWeight >= r) {
                    return entry.item;
                }
            }
            throw new Exception("Weights have been computed incorrectly");
            // return -1; //should only happen when there are no entries
        }
    }
}
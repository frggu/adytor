using System.Collections.ObjectModel;

namespace Adytor
{
    public static class CollectionExtensions
    {
        //Add new pair at the correct index to keep collection sorted
        public static void SortedAdd(this ObservableCollection<FreqAmpPair> collect, FreqAmpPair newPair)
        {
            if (newPair is null)
                return;

            if (collect.Count == 0)
            {
                collect.Add(newPair);
                return;
            }

            for (int element = 0; element < collect.Count; element++)
            {
                if (element == 0 && newPair.Frequency <= collect[element].Frequency)
                {
                    collect.Insert(0, newPair);
                    return;
                }

                if (element == collect.Count - 1 && newPair.Frequency >= collect[element].Frequency)
                {
                    collect.Add(newPair);
                    return;
                }

                if (element > 0 && newPair.Frequency >= collect[element - 1].Frequency && newPair.Frequency <= collect[element].Frequency)
                {
                    collect.Insert(element, newPair);
                    return;
                }
            }
            collect.Add(newPair);
        }

        //Removes given element and re-inserts it at the correct location
        public static void ResortWrongElement(this ObservableCollection<FreqAmpPair> collect, FreqAmpPair checkPair)
        {
            if (checkPair is null)
                return;

            if (collect.Count == 0)
                return;

            collect.Remove(checkPair);
            collect.SortedAdd(checkPair);
        }

    }
}

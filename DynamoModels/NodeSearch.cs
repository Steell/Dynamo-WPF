using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace Dynamo.UI.Models
{
    public interface INodeSource
    {
        IObservable<Node> NewNodeStream { get; }
    }

    public class CompositeNodeSource : INodeSource
    {
        public IObservable<Node> NewNodeStream { get; private set; }

        public CompositeNodeSource(IEnumerable<INodeSource> sources)
        {
            NewNodeStream = sources.Select(nodeSource => nodeSource.NewNodeStream).Merge();
        }
    }

    public class NodeEntry : INodeSource
    {
        public NodeEntry(NodeInfo info)
        {
            NewNodeStream = Observable.Return(info.ToNode());
        }

        public IObservable<Node> NewNodeStream { get; private set; }
        public string Name { get; private set; }
    }

    public class NodeCategory : CompositeNodeSource
    {
        public NodeCategory(string name, ICollection<INodeSource> sources) : base(sources)
        {
            Name = name;
            Contents = sources;
        }

        public IEnumerable<INodeSource> Contents { get; private set; }
        public string Name { get; private set; }
    }

    public class NodeSearch : INodeSource
    {
        public IObservable<Node> NewNodeStream { get; private set; }
        public IObservable<IEnumerable<NodeEntry>> LibraryStream { get; private set; }

        public NodeSearch(
            IObservable<string> searchQueryStream,
            IObservable<NodeEntry> newNodeEntryStream,
            IObservable<NodeEntry> deletedNodeEntryStream)
        {
            var libraryChangeStream = 
                Observable.Merge(
                    newNodeEntryStream.Select(LibraryUpdate.Add),
                    deletedNodeEntryStream.Select(LibraryUpdate.Remove));

            var libraryStream = libraryChangeStream.Scan(
                ImmutableHashSet<NodeEntry>.Empty, 
                LibraryUpdate.UpdateLibrary);

            LibraryStream = libraryStream.CombineLatest(searchQueryStream, GetSearchResults);
            NewNodeStream =
                LibraryStream
                    .Select(library => library.Select(x => x.NewNodeStream).Merge())
                    .Switch();
        }

        private static IEnumerable<NodeEntry> GetSearchResults(
            IEnumerable<NodeEntry> library, string searchQuery)
        {
            if (string.IsNullOrEmpty(searchQuery))
                return library;

            return
                library.Select(
                    entry => 
                        new { Entry = entry, Distance = StringDistance(entry.Name, searchQuery) })
                    .OrderByDescending(result => result.Distance)
                    .Select(result => result.Entry);
        }

        private struct LibraryUpdate
        {
            private enum LibraryChange { Add, Remove }

            private LibraryChange changeType;
            private NodeEntry entry;

            public static LibraryUpdate Add(NodeEntry entry)
            {
                return new LibraryUpdate { changeType = LibraryChange.Add, entry = entry };
            }

            public static LibraryUpdate Remove(NodeEntry entry)
            {
                return new LibraryUpdate { changeType = LibraryChange.Remove, entry = entry };
            }

            public static ImmutableHashSet<NodeEntry> UpdateLibrary(
                ImmutableHashSet<NodeEntry> library, 
                LibraryUpdate update)
            {
                switch (update.changeType)
                {
                    case LibraryChange.Add:
                        return library.Add(update.entry);
                    case LibraryChange.Remove:
                        return library.Remove(update.entry);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        #region Jaro-Winkler String Similarity Algorithm
        private static double StringDistance(string firstWord, string secondWord)
        {
            const double prefixAdustmentScale = 0.1;

            if ((firstWord != null) && (secondWord != null))
            {
                double dist = GetJaroDistance(firstWord, secondWord);
                int prefixLength = GetPrefixLength(firstWord, secondWord);
                return dist + prefixLength * prefixAdustmentScale * (1.0 - dist);
            }
            return 0.0;
            
        }

        private static double GetJaroDistance(string firstWord, string secondWord)
        {
            const double defaultMismatchScore = 0;

            if ((firstWord != null) && (secondWord != null))
            {
                //get half the length of the string rounded up 
                //(this is the distance used for acceptable transpositions)
                int halflen = Math.Min(firstWord.Length, secondWord.Length) / 2 + 1;

                //get common characters
                StringBuilder common1 = GetCommonCharacters(firstWord, secondWord, halflen);
                int commonMatches = common1.Length;

                //check for zero in common
                if (commonMatches == 0)
                {
                    return defaultMismatchScore;
                }

                StringBuilder common2 = GetCommonCharacters(secondWord, firstWord, halflen);

                //check for same length common strings returning 0.0f is not the same
                if (commonMatches != common2.Length)
                {
                    return defaultMismatchScore;
                }

                //get the number of transpositions
                int transpositions = 0;
                for (int i = 0; i < commonMatches; i++)
                {
                    if (common1[i] != common2[i])
                    {
                        transpositions++;
                    }
                }

                //calculate jaro metric
                transpositions /= 2;
                double tmp1 = commonMatches / (3.0 * firstWord.Length) 
                    + commonMatches / (3.0 * secondWord.Length) 
                    + (commonMatches - transpositions) / (3.0 * commonMatches);
                return tmp1;
            }
            return defaultMismatchScore;
        }

        private static StringBuilder GetCommonCharacters(
            string firstWord, string secondWord, int distanceSep)
        {
            if ((firstWord != null) && (secondWord != null))
            {
                var returnCommons = new StringBuilder();
                var copy = new StringBuilder(secondWord);
                for (int i = 0; i < firstWord.Length; i++)
                {
                    char ch = firstWord[i];
                    bool foundIt = false;
                    for (int j = Math.Max(0, i - distanceSep);
                         !foundIt && j < Math.Min(i + distanceSep, secondWord.Length);
                         j++)
                    {
                        if (copy[j] == ch)
                        {
                            foundIt = true;
                            returnCommons.Append(ch);
                            copy[j] = '#';
                        }
                    }
                }

                return returnCommons;
            }
            return null;
        }

        private static int GetPrefixLength(string firstWord, string secondWord)
        {
            const int minPrefixTestLength = 4;

            if ((firstWord != null) && (secondWord != null))
            {
                int n = 
                    Math.Min(
                        minPrefixTestLength, 
                        Math.Min(firstWord.Length, secondWord.Length));
                
                for (int i = 0; i < n; i++)
                {
                    if (firstWord[i] != secondWord[i])
                        return i;
                }

                return n;
            }
            return minPrefixTestLength;
        }
        #endregion
    }
}
